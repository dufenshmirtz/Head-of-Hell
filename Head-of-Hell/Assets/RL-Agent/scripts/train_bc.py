import os
import math
import random
from dataclasses import dataclass

import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import Dataset, DataLoader


# =========================
# PATHS
# =========================
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
BASE_DIR = os.path.dirname(SCRIPT_DIR)

DATA_PATH = os.path.join(BASE_DIR, "processed", "imitation_dataset.npz")
MODELS_DIR = os.path.join(BASE_DIR, "models")
os.makedirs(MODELS_DIR, exist_ok=True)


# =========================
# CONFIG
# =========================
@dataclass
class Config:
    seed: int = 42
    batch_size: int = 512
    lr: float = 1e-3
    weight_decay: float = 1e-5
    epochs: int = 25
    val_ratio: float = 0.15
    hidden_dim: int = 256
    dropout: float = 0.15
    device: str = "cuda" if torch.cuda.is_available() else "cpu"


CFG = Config()

# branch sizes in exact order of your action vector
BRANCH_SIZES = [3, 2, 2, 2, 2, 2, 2, 3, 2]
BRANCH_NAMES = [
    "move", "jump", "drop", "light", "heavy",
    "block", "special", "charge", "parry"
]


# =========================
# UTILS
# =========================
def set_seed(seed: int):
    random.seed(seed)
    np.random.seed(seed)
    torch.manual_seed(seed)
    torch.cuda.manual_seed_all(seed)


def accuracy_from_logits(logits: torch.Tensor, targets: torch.Tensor) -> float:
    preds = torch.argmax(logits, dim=1)
    return (preds == targets).float().mean().item()


# =========================
# DATASET
# =========================
class ImitationDataset(Dataset):
    def __init__(self, x_obs: np.ndarray, y_act: np.ndarray):
        self.x = torch.tensor(x_obs, dtype=torch.float32)
        self.y = torch.tensor(y_act, dtype=torch.long)

    def __len__(self):
        return self.x.shape[0]

    def __getitem__(self, idx):
        return self.x[idx], self.y[idx]


def load_dataset(path: str):
    data = np.load(path)
    x_obs = data["X_obs"]
    y_act = data["Y_act"]

    assert x_obs.ndim == 2, f"X_obs should be 2D, got {x_obs.shape}"
    assert y_act.ndim == 2, f"Y_act should be 2D, got {y_act.shape}"
    assert y_act.shape[1] == len(BRANCH_SIZES), f"Expected 9 action branches, got {y_act.shape[1]}"

    return x_obs, y_act


def make_splits(x_obs: np.ndarray, y_act: np.ndarray, val_ratio: float):
    n = len(x_obs)
    indices = np.arange(n)
    np.random.shuffle(indices)

    val_size = int(n * val_ratio)
    val_idx = indices[:val_size]
    train_idx = indices[val_size:]

    return (
        x_obs[train_idx], y_act[train_idx],
        x_obs[val_idx], y_act[val_idx]
    )


# =========================
# MODEL
# =========================
class BCPolicy(nn.Module):
    def __init__(self, input_dim: int, hidden_dim: int, dropout: float):
        super().__init__()

        self.backbone = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(dropout),

            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(dropout),

            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
        )

        self.heads = nn.ModuleList([
            nn.Linear(hidden_dim, branch_size)
            for branch_size in BRANCH_SIZES
        ])

    def forward(self, x):
        z = self.backbone(x)
        return [head(z) for head in self.heads]


# =========================
# TRAIN / EVAL
# =========================
def compute_loss_and_metrics(model, batch_x, batch_y, device, class_weights=None):
    batch_x = batch_x.to(device)
    batch_y = batch_y.to(device)

    logits_per_branch = model(batch_x)

    total_loss = 0.0
    branch_losses = []
    branch_accs = []

    for i, logits in enumerate(logits_per_branch):
        targets = batch_y[:, i]

        weight_i = None
        if class_weights is not None:
            weight_i = class_weights[i]

        loss_i = nn.functional.cross_entropy(logits, targets, weight=weight_i)
        acc_i = accuracy_from_logits(logits, targets)

        total_loss = total_loss + loss_i
        branch_losses.append(loss_i.item())
        branch_accs.append(acc_i)

    return total_loss, branch_losses, branch_accs


def run_epoch(model, loader, optimizer, device, train: bool, class_weights=None):
    if train:
        model.train()
    else:
        model.eval()

    total_loss_sum = 0.0
    total_samples = 0

    branch_loss_sums = np.zeros(len(BRANCH_SIZES), dtype=np.float64)
    branch_acc_sums = np.zeros(len(BRANCH_SIZES), dtype=np.float64)

    for batch_x, batch_y in loader:
        bs = batch_x.size(0)

        if train:
            optimizer.zero_grad()

        with torch.set_grad_enabled(train):
            total_loss, branch_losses, branch_accs = compute_loss_and_metrics(
                model, batch_x, batch_y, device, class_weights=class_weights
            )

            if train:
                total_loss.backward()
                optimizer.step()

        total_loss_sum += total_loss.item() * bs
        total_samples += bs
        branch_loss_sums += np.array(branch_losses) * bs
        branch_acc_sums += np.array(branch_accs) * bs

    avg_total_loss = total_loss_sum / total_samples
    avg_branch_losses = branch_loss_sums / total_samples
    avg_branch_accs = branch_acc_sums / total_samples

    mean_branch_acc = float(np.mean(avg_branch_accs))

    return avg_total_loss, avg_branch_losses, avg_branch_accs, mean_branch_acc

def compute_branch_class_weights(y_act: np.ndarray, branch_sizes, power: float = 0.5, min_weight: float = 1.0, max_weight: float = 8.0):
    """
    Computes inverse-frequency-ish class weights per branch.

    power:
        1.0 = full inverse frequency
        0.5 = sqrt inverse frequency (safer / milder)
    """
    weights_per_branch = []

    for branch_idx, num_classes in enumerate(branch_sizes):
        counts = np.bincount(y_act[:, branch_idx], minlength=num_classes).astype(np.float64)

        # avoid division by zero
        counts[counts == 0] = 1.0

        total = counts.sum()
        freqs = counts / total

        # inverse frequency with softening
        weights = (1.0 / freqs) ** power

        # normalize so mean weight ~= 1
        weights = weights / weights.mean()

        # clamp to avoid insane weights
        weights = np.clip(weights, min_weight, max_weight)

        weights_per_branch.append(torch.tensor(weights, dtype=torch.float32))

        print(f"[INFO] Branch {branch_idx} class counts: {counts.astype(int).tolist()}")
        print(f"[INFO] Branch {branch_idx} class weights: {weights.tolist()}")

    return weights_per_branch


def main():
    set_seed(CFG.seed)

    print(f"[INFO] Loading dataset from: {DATA_PATH}")
    x_obs, y_act = load_dataset(DATA_PATH)

    print(f"[INFO] X shape: {x_obs.shape}")
    print(f"[INFO] Y shape: {y_act.shape}")
    print(f"[INFO] Device: {CFG.device}")

    x_train, y_train, x_val, y_val = make_splits(x_obs, y_act, CFG.val_ratio)

    # ===== CLASS WEIGHTS =====
    class_weights = compute_branch_class_weights(
        y_train,
        BRANCH_SIZES,
        power=0.5,
        min_weight=1.0,
        max_weight=6.0
    )

    class_weights = [w.to(CFG.device) for w in class_weights]

    train_ds = ImitationDataset(x_train, y_train)
    val_ds = ImitationDataset(x_val, y_val)

    train_loader = DataLoader(train_ds, batch_size=CFG.batch_size, shuffle=True)
    val_loader = DataLoader(val_ds, batch_size=CFG.batch_size, shuffle=False)

    model = BCPolicy(
        input_dim=x_obs.shape[1],
        hidden_dim=CFG.hidden_dim,
        dropout=CFG.dropout
    ).to(CFG.device)

    optimizer = torch.optim.Adam(
        model.parameters(),
        lr=CFG.lr,
        weight_decay=CFG.weight_decay
    )

    best_val_loss = math.inf
    best_path = os.path.join(MODELS_DIR, "bc_policy_best.pt")
    last_path = os.path.join(MODELS_DIR, "bc_policy_last.pt")

    for epoch in range(1, CFG.epochs + 1):
        train_loss, train_branch_losses, train_branch_accs, train_mean_acc = run_epoch(
            model, train_loader, optimizer, CFG.device, train=True, class_weights=class_weights
        )

        val_loss, val_branch_losses, val_branch_accs, val_mean_acc = run_epoch(
            model, val_loader, optimizer, CFG.device, train=False, class_weights=class_weights
        )

        print(f"\nEpoch {epoch}/{CFG.epochs}")
        print(f"  Train Loss: {train_loss:.4f} | Train Mean Branch Acc: {train_mean_acc:.4f}")
        print(f"  Val   Loss: {val_loss:.4f} | Val   Mean Branch Acc: {val_mean_acc:.4f}")

        for i, name in enumerate(BRANCH_NAMES):
            print(
                f"    {name:<8} "
                f"train_acc={train_branch_accs[i]:.4f} "
                f"val_acc={val_branch_accs[i]:.4f}"
            )

        torch.save({
            "model_state_dict": model.state_dict(),
            "input_dim": x_obs.shape[1],
            "hidden_dim": CFG.hidden_dim,
            "dropout": CFG.dropout,
            "branch_sizes": BRANCH_SIZES,
            "branch_names": BRANCH_NAMES,
            "class_weights": [w.detach().cpu() for w in class_weights],
        }, last_path)

        if val_loss < best_val_loss:
            best_val_loss = val_loss
            torch.save({
                "model_state_dict": model.state_dict(),
                "input_dim": x_obs.shape[1],
                "hidden_dim": CFG.hidden_dim,
                "dropout": CFG.dropout,
                "branch_sizes": BRANCH_SIZES,
                "branch_names": BRANCH_NAMES,
                "class_weights": [w.detach().cpu() for w in class_weights],
            }, best_path)
            print(f"  [INFO] Saved new best model -> {best_path}")

    print("\n[INFO] Training complete.")
    print(f"[INFO] Best model: {best_path}")
    print(f"[INFO] Last model: {last_path}")


if __name__ == "__main__":
    main()