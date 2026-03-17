import os
import torch
import torch.nn as nn


# =========================
# PATHS
# =========================
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
BASE_DIR = os.path.dirname(SCRIPT_DIR)

MODEL_PATH = os.path.join(BASE_DIR, "models", "bc_policy_best.pt")
EXPORT_PATH = os.path.join(BASE_DIR, "models", "bc_policy_barracuda.onnx")


# =========================
# MODEL DEFINITION
# MUST MATCH train_bc.py
# =========================
class BCPolicy(nn.Module):
    def __init__(self, input_dim: int, hidden_dim: int, dropout: float, branch_sizes):
        super().__init__()

        self.backbone = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),

            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),

            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
        )

        self.heads = nn.ModuleList([
            nn.Linear(hidden_dim, branch_size)
            for branch_size in branch_sizes
        ])

    def forward(self, x):
        z = self.backbone(x)
        outputs = [head(z) for head in self.heads]
        return tuple(outputs)


def main():
    checkpoint = torch.load(MODEL_PATH, map_location="cpu")

    input_dim = checkpoint["input_dim"]
    hidden_dim = checkpoint["hidden_dim"]
    branch_sizes = checkpoint["branch_sizes"]

    # dropout=0 for export graph simplicity
    model = BCPolicy(
        input_dim=input_dim,
        hidden_dim=hidden_dim,
        dropout=0.0,
        branch_sizes=branch_sizes
    )

    model.load_state_dict(checkpoint["model_state_dict"], strict=False)
    model.eval()

    dummy_input = torch.randn(1, input_dim, dtype=torch.float32)

    output_names = [
        "move_logits",
        "jump_logits",
        "drop_logits",
        "light_logits",
        "heavy_logits",
        "block_logits",
        "special_logits",
        "charge_logits",
        "parry_logits",
    ]

    with torch.no_grad():
        torch.onnx.export(
            model,
            dummy_input,
            EXPORT_PATH,
            export_params=True,
            opset_version=11,
            do_constant_folding=True,
            input_names=["obs"],
            output_names=output_names,
            training=torch.onnx.TrainingMode.EVAL,
            dynamo=False,
        )

    print(f"[INFO] Exported Barracuda-friendly ONNX model to: {EXPORT_PATH}")


if __name__ == "__main__":
    main()