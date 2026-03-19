import os
import argparse
import warnings

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt


# -----------------------------
# Helpers
# -----------------------------
def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def safe_minmax(series: pd.Series) -> pd.Series:
    """Min-max normalize a numeric pandas Series to [0, 1].
    If max == min, return zeros to avoid division by zero.
    """
    s = pd.to_numeric(series, errors="coerce").fillna(0.0)
    min_v = s.min()
    max_v = s.max()
    if pd.isna(min_v) or pd.isna(max_v) or max_v == min_v:
        return pd.Series(np.zeros(len(s)), index=s.index, dtype=float)
    return (s - min_v) / (max_v - min_v)


def pick_first_existing(
    df: pd.DataFrame,
    candidates: list[str],
    default_name: str,
) -> tuple[pd.Series, str]:
    """Pick the first existing column from candidates, else return a default fallback series."""
    for c in candidates:
        if c in df.columns:
            return df[c].fillna(default_name).astype(str), c
    return pd.Series([default_name] * len(df), index=df.index, dtype=str), default_name


def validate_required_columns(df: pd.DataFrame, required: list[str]) -> None:
    missing = [c for c in required if c not in df.columns]
    if missing:
        raise ValueError(
            "Missing required columns in dataset_round_level.csv:\n"
            + "\n".join(f"- {c}" for c in missing)
        )


# -----------------------------
# Main computation
# -----------------------------
def compute_style_scores(df: pd.DataFrame) -> pd.DataFrame:
    required = [
        "attack_rate",
        "mobility_rate",
        "defense_rate",
        "hit_rate",
        "miss_rate",
        "block_rate",
        "dodge_rate",
        "quick_rate",
        "heavy_rate",
        "special_rate",
        "charge_rate",
        "dps_dealt",
        "dps_taken",
    ]
    validate_required_columns(df, required)

    out = df.copy()

    # Optional but useful for substyle exports.
    if "projectile_rate" not in out.columns:
        out["projectile_rate"] = 0.0

    numeric_cols = required + ["projectile_rate"]
    for col in numeric_cols:
        out[col] = pd.to_numeric(out[col], errors="coerce").fillna(0.0)

    # Normalized continuous combat outcome features
    out["norm_dps_dealt"] = safe_minmax(out["dps_dealt"])
    out["norm_dps_taken"] = safe_minmax(out["dps_taken"])

    # Derived style axes
    out["Aggression"] = (
        0.35 * out["attack_rate"]
        + 0.20 * out["hit_rate"]
        + 0.15 * out["quick_rate"]
        + 0.15 * out["heavy_rate"]
        + 0.10 * out["charge_rate"]
        + 0.05 * out["norm_dps_dealt"]
    )

    out["Defense"] = (
        0.35 * out["defense_rate"]
        + 0.25 * out["block_rate"]
        + 0.20 * out["dodge_rate"]
        + 0.20 * (1.0 - out["norm_dps_taken"])
    )

    out["Mobility"] = 0.85 * out["mobility_rate"] + 0.15 * out["dodge_rate"]

    out["Risk"] = (
        0.35 * out["miss_rate"]
        + 0.30 * out["norm_dps_taken"]
        + 0.20 * out["special_rate"]
        + 0.15 * out["charge_rate"]
    )

    # Normalized axes for cleaner comparison in scatter plots.
    out["Aggression_norm"] = safe_minmax(out["Aggression"])
    out["Defense_norm"] = safe_minmax(out["Defense"])
    out["Mobility_norm"] = safe_minmax(out["Mobility"])
    out["Risk_norm"] = safe_minmax(out["Risk"])

    return out


# -----------------------------
# Plotting
# -----------------------------
def make_color_map(labels: pd.Series) -> dict[str, tuple]:
    unique_labels = list(pd.unique(labels.astype(str)))
    cmap = plt.get_cmap("tab20")
    color_map: dict[str, tuple] = {}
    for i, label in enumerate(unique_labels):
        color_map[label] = cmap(i % 20)
    return color_map


def scatter_plot(
    df: pd.DataFrame,
    x_col: str,
    y_col: str,
    label_col: str,
    title: str,
    out_path: str,
    annotate: bool = False,
) -> None:
    labels = df[label_col].astype(str).fillna("UNKNOWN")
    color_map = make_color_map(labels)

    plt.figure(figsize=(10, 7))

    for label in pd.unique(labels):
        mask = labels == label
        plt.scatter(
            df.loc[mask, x_col],
            df.loc[mask, y_col],
            label=label,
            s=70,
            alpha=0.8,
            color=color_map[label],
        )

    if annotate:
        player_label_series, _ = pick_first_existing(
            df,
            ["profile_name", "player_id", "player_slot", "profile_id"],
            "point",
        )
        for i, row in df.iterrows():
            plt.annotate(
                str(player_label_series.loc[i]),
                (row[x_col], row[y_col]),
                fontsize=8,
                alpha=0.75,
                xytext=(4, 4),
                textcoords="offset points",
            )

    plt.xlabel(x_col, fontsize=12)
    plt.ylabel(y_col, fontsize=12)
    plt.title(title, fontsize=14)
    plt.grid(True, alpha=0.25)

    unique_count = labels.nunique()
    if unique_count <= 12:
        plt.legend(title=label_col, fontsize=9)
    else:
        plt.legend(
            title=label_col,
            fontsize=8,
            bbox_to_anchor=(1.02, 1),
            loc="upper left",
            borderaxespad=0.0,
        )

    plt.tight_layout()
    plt.savefig(out_path, dpi=200, bbox_inches="tight")
    plt.close()


# -----------------------------
# Entry point
# -----------------------------
def main() -> None:
    parser = argparse.ArgumentParser(
        description="Create round-level player style maps from dataset_round_level.csv"
    )
    parser.add_argument(
        "--input",
        default=os.path.join("out", "dataset_round_level.csv"),
        help="Path to dataset_round_level.csv",
    )
    parser.add_argument(
        "--output-dir",
        default="style_maps",
        help="Directory to save plots and CSV outputs",
    )
    parser.add_argument(
        "--annotate",
        action="store_true",
        help="Annotate scatter points with profile/player labels",
    )
    args = parser.parse_args()

    if not os.path.exists(args.input):
        raise FileNotFoundError(f"Input CSV not found: {args.input}")

    ensure_dir(args.output_dir)

    df = pd.read_csv(args.input)
    if df.empty:
        raise ValueError("Input CSV is empty.")

    label_series, label_col_name = pick_first_existing(
        df,
        ["profile_name", "profile_id", "player_id", "player_slot"],
        "UNKNOWN_PROFILE",
    )
    df["_plot_label"] = label_series

    profile_name_series, _ = pick_first_existing(
        df,
        ["profile_name"],
        "UNKNOWN_PROFILE_NAME",
    )
    player_slot_series, _ = pick_first_existing(
        df,
        ["player_slot", "player", "slot_id", "slot"],
        "UNKNOWN_SLOT",
    )
    round_series, _ = pick_first_existing(
        df,
        ["round_number", "round"],
        "UNKNOWN_ROUND",
    )

    scored = compute_style_scores(df)

    scored["_plot_label"] = df["_plot_label"]
    scored["_profile_name"] = profile_name_series
    scored["_player_slot"] = player_slot_series
    scored["_round_context"] = round_series

    scored_csv_path = os.path.join(args.output_dir, "round_style_scores.csv")
    scored.to_csv(scored_csv_path, index=False)

    scatter_plot(
        scored,
        x_col="Aggression_norm",
        y_col="Defense_norm",
        label_col="_plot_label",
        title="Round Style Map: Aggression vs Defense",
        out_path=os.path.join(args.output_dir, "aggression_vs_defense.png"),
        annotate=args.annotate,
    )

    scatter_plot(
        scored,
        x_col="Mobility_norm",
        y_col="Risk_norm",
        label_col="_plot_label",
        title="Round Style Map: Mobility vs Risk",
        out_path=os.path.join(args.output_dir, "mobility_vs_risk.png"),
        annotate=args.annotate,
    )

    summary_cols = [
        "Aggression",
        "Defense",
        "Mobility",
        "Risk",
        "Aggression_norm",
        "Defense_norm",
        "Mobility_norm",
        "Risk_norm",
        # substyle-support columns
        "quick_rate",
        "heavy_rate",
        "special_rate",
        "charge_rate",
        "projectile_rate",
        "mobility_rate",
        "dodge_rate",
        "block_rate",
        "miss_rate",
        "dps_taken",
    ]

    available_summary_cols = [c for c in summary_cols if c in scored.columns]
    missing_but_optional = [c for c in summary_cols if c not in scored.columns]
    if missing_but_optional:
        print("[WARN] Skipping missing summary columns:", ", ".join(missing_but_optional))

    summary = (
        scored.groupby("_plot_label", dropna=False)[available_summary_cols]
        .mean()
        .reset_index()
        .rename(columns={"_plot_label": label_col_name})
    )
    summary.to_csv(
        os.path.join(args.output_dir, "profile_style_summary_from_rounds.csv"),
        index=False,
    )

    print("Saved outputs to:", args.output_dir)
    print("Generated:")
    print("-", os.path.join(args.output_dir, "round_style_scores.csv"))
    print("-", os.path.join(args.output_dir, "profile_style_summary_from_rounds.csv"))
    print("-", os.path.join(args.output_dir, "aggression_vs_defense.png"))
    print("-", os.path.join(args.output_dir, "mobility_vs_risk.png"))
    print()
    print("Color grouping column:", label_col_name)
    print("Rows processed:", len(scored))
    print("Profiles/groups found:", scored["_plot_label"].nunique())


if __name__ == "__main__":
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        main()
