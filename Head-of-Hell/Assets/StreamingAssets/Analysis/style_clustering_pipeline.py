from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Dict, List, Tuple

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.decomposition import PCA
from sklearn.metrics import silhouette_score
from sklearn.preprocessing import StandardScaler

ROUND_FEATURES = [
    "attack_rate",
    "mobility_rate",
    "defense_rate",
    "hit_rate",
    "miss_rate",
    "dps_dealt",
    "dps_taken",
    "block_rate",
    "dodge_rate",
    "quick_rate",
    "heavy_rate",
    "special_rate",
    "charge_rate",
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Cluster round/session playstyles and aggregate them per player.")
    parser.add_argument("--round-csv", type=Path, required=True, help="Path to dataset_round_level.csv")
    parser.add_argument("--elo-csv", type=Path, required=True, help="Path to elo_overall.csv")
    parser.add_argument("--outdir", type=Path, default=Path("style_out"), help="Output directory")
    parser.add_argument("--k", type=int, default=None, help="Fixed number of clusters. If omitted, choose automatically.")
    parser.add_argument("--min-k", type=int, default=2, help="Minimum k for auto-search")
    parser.add_argument("--max-k", type=int, default=5, help="Maximum k for auto-search")
    return parser.parse_args()


def load_and_prepare(round_csv: Path, elo_csv: Path) -> Tuple[pd.DataFrame, pd.DataFrame]:
    rounds = pd.read_csv(round_csv)
    elo = pd.read_csv(elo_csv)

    missing = [c for c in ROUND_FEATURES if c not in rounds.columns]
    if missing:
        raise ValueError(f"Missing required round features: {missing}")

    if "player_id" not in rounds.columns and "profile_id" in rounds.columns:
        rounds = rounds.rename(columns={"profile_id": "player_id"})

    if "player_id" not in elo.columns and "profile_id" in elo.columns:
        elo = elo.rename(columns={"profile_id": "player_id"})

    if "player_id" not in rounds.columns:
        raise ValueError("round dataset must include player_id (or profile_id)")

    if "player_id" not in elo.columns:
        raise ValueError("elo csv must include player_id (or profile_id) and overall_elo")

    if "overall_elo" not in elo.columns:
        raise ValueError("elo csv must include overall_elo")

    keep_cols = [
        c for c in ["match_id", "round_number", "stage", "mode", "duration_s", "won_round", "player_id"] + ROUND_FEATURES
        if c in rounds.columns
    ]
    rounds = rounds[keep_cols].copy()

    rounds[ROUND_FEATURES] = rounds[ROUND_FEATURES].replace([np.inf, -np.inf], np.nan)
    rounds = rounds.dropna(subset=ROUND_FEATURES + ["player_id"]).reset_index(drop=True)

    return rounds, elo


def compute_style_axes(df: pd.DataFrame) -> pd.DataFrame:
    out = df.copy()
    out["aggression"] = 0.40 * out["attack_rate"] + 0.35 * out["dps_dealt"] + 0.25 * out["hit_rate"]
    out["defense_style"] = 0.45 * out["defense_rate"] + 0.30 * out["block_rate"] + 0.25 * out["dodge_rate"]
    out["mobility_style"] = out["mobility_rate"]
    out["risk_style"] = 0.50 * out["miss_rate"] + 0.50 * out["dps_taken"]
    return out


def choose_k(X_scaled: np.ndarray, min_k: int, max_k: int) -> Tuple[int, List[Dict[str, float]]]:
    n_samples = len(X_scaled)
    if n_samples < 3:
        return 1, []

    lo = max(2, min_k)
    hi = min(max_k, n_samples - 1)
    if hi < lo:
        return 1, []

    rows: List[Dict[str, float]] = []
    best_k = lo
    best_score = -1.0

    for k in range(lo, hi + 1):
        model = KMeans(n_clusters=k, random_state=42, n_init=20)
        labels = model.fit_predict(X_scaled)
        if len(set(labels)) < 2:
            continue
        score = silhouette_score(X_scaled, labels)
        inertia = float(model.inertia_)
        rows.append({"k": k, "silhouette": float(score), "inertia": inertia})
        if score > best_score:
            best_score = score
            best_k = k
    return best_k, rows


def summarize_clusters(df: pd.DataFrame) -> pd.DataFrame:
    summary = df.groupby("cluster")[ROUND_FEATURES + ["aggression", "defense_style", "mobility_style", "risk_style"]].mean()
    summary["n_rounds"] = df.groupby("cluster").size()
    return summary.reset_index()


def assign_cluster_labels(summary: pd.DataFrame) -> Dict[int, str]:
    labels: Dict[int, str] = {}
    global_means = summary[["aggression", "defense_style", "mobility_style", "risk_style"]].mean()

    for _, row in summary.iterrows():
        cluster_id = int(row["cluster"])
        aggr = row["aggression"]
        defense = row["defense_style"]
        mobility = row["mobility_style"]
        risk = row["risk_style"]

        if aggr >= max(defense, mobility) and aggr > global_means["aggression"]:
            label = "Aggressive"
        elif defense >= max(aggr, mobility) and defense > global_means["defense_style"]:
            label = "Defensive"
        elif mobility >= max(aggr, defense) and mobility > global_means["mobility_style"]:
            label = "Mobile"
        elif risk > global_means["risk_style"] and aggr > global_means["aggression"]:
            label = "Risky"
        else:
            label = "Balanced"
        labels[cluster_id] = label
    return labels


def build_player_style_distribution(df: pd.DataFrame, elo: pd.DataFrame) -> pd.DataFrame:
    counts = (
        df.groupby(["player_id", "playstyle"]).size().rename("n_rounds").reset_index()
    )
    totals = counts.groupby("player_id")["n_rounds"].transform("sum")
    counts["style_share"] = counts["n_rounds"] / totals

    pivot = counts.pivot(index="player_id", columns="playstyle", values="style_share").fillna(0.0)
    pivot.columns = [f"share_{c.lower()}" for c in pivot.columns]
    pivot = pivot.reset_index()

    dominant = counts.sort_values(["player_id", "style_share", "n_rounds"], ascending=[True, False, False])
    dominant = dominant.groupby("player_id").first().reset_index()[["player_id", "playstyle", "style_share"]]
    dominant = dominant.rename(columns={"playstyle": "dominant_playstyle", "style_share": "dominant_playstyle_share"})

    player_means = df.groupby("player_id")[ROUND_FEATURES + ["aggression", "defense_style", "mobility_style", "risk_style"]].mean().reset_index()

    out = player_means.merge(pivot, on="player_id", how="left")
    out = out.merge(dominant, on="player_id", how="left")
    out = out.merge(elo, on="player_id", how="left")
    return out


def save_pca_plot(df: pd.DataFrame, outdir: Path) -> None:
    if len(df) < 2:
        return
    X = df[ROUND_FEATURES].to_numpy(dtype=float)
    X_scaled = StandardScaler().fit_transform(X)
    pca = PCA(n_components=2, random_state=42)
    coords = pca.fit_transform(X_scaled)

    plot_df = df.copy()
    plot_df["pc1"] = coords[:, 0]
    plot_df["pc2"] = coords[:, 1]

    fig, ax = plt.subplots(figsize=(8, 6))
    for playstyle, group in plot_df.groupby("playstyle"):
        ax.scatter(group["pc1"], group["pc2"], label=playstyle, alpha=0.8)
    ax.set_title("Round Style Map (PCA)")
    ax.set_xlabel(f"PC1 ({pca.explained_variance_ratio_[0]*100:.1f}% var)")
    ax.set_ylabel(f"PC2 ({pca.explained_variance_ratio_[1]*100:.1f}% var)")
    ax.legend()
    fig.tight_layout()
    fig.savefig(outdir / "round_style_map_pca.png", dpi=160)
    plt.close(fig)


def save_style_distribution_plot(player_styles: pd.DataFrame, outdir: Path) -> None:
    share_cols = [c for c in player_styles.columns if c.startswith("share_")]
    if not share_cols:
        return
    plot_df = player_styles.set_index("player_id")[share_cols]
    ax = plot_df.plot(kind="bar", stacked=True, figsize=(9, 5))
    ax.set_title("Player Style Distribution")
    ax.set_xlabel("Player")
    ax.set_ylabel("Share of rounds")
    plt.tight_layout()
    plt.savefig(outdir / "player_style_distribution.png", dpi=160)
    plt.close()


def main() -> None:
    args = parse_args()
    outdir = args.outdir
    outdir.mkdir(parents=True, exist_ok=True)

    rounds, elo = load_and_prepare(args.round_csv, args.elo_csv)
    rounds = compute_style_axes(rounds)

    X = rounds[ROUND_FEATURES].to_numpy(dtype=float)
    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    if args.k is not None:
        k = max(1, int(args.k))
        search_rows: List[Dict[str, float]] = []
    else:
        k, search_rows = choose_k(X_scaled, args.min_k, args.max_k)

    if k <= 1 or len(rounds) < 3:
        rounds["cluster"] = 0
    else:
        model = KMeans(n_clusters=k, random_state=42, n_init=20)
        rounds["cluster"] = model.fit_predict(X_scaled)

    cluster_summary = summarize_clusters(rounds)
    cluster_labels = assign_cluster_labels(cluster_summary)
    rounds["playstyle"] = rounds["cluster"].map(cluster_labels).fillna("Balanced")

    cluster_summary["playstyle"] = cluster_summary["cluster"].map(cluster_labels)
    player_styles = build_player_style_distribution(rounds, elo)

    rounds.to_csv(outdir / "rounds_with_clusters.csv", index=False)
    cluster_summary.to_csv(outdir / "cluster_summary.csv", index=False)
    player_styles.to_csv(outdir / "player_style_distribution_with_elo.csv", index=False)

    with open(outdir / "k_search.json", "w", encoding="utf-8") as f:
        json.dump(search_rows, f, indent=2)
    with open(outdir / "cluster_label_map.json", "w", encoding="utf-8") as f:
        json.dump({str(k): v for k, v in cluster_labels.items()}, f, indent=2)

    save_pca_plot(rounds, outdir)
    save_style_distribution_plot(player_styles, outdir)

    report = {
        "n_rounds": int(len(rounds)),
        "n_players": int(rounds["player_id"].nunique()),
        "chosen_k": int(k),
        "features": ROUND_FEATURES,
        "note": "If n_rounds is very small, clustering results are exploratory only.",
    }
    with open(outdir / "run_report.json", "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2)

    print("Saved outputs to:", outdir)
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
