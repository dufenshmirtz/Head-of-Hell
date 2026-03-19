import os
import json
import argparse
import shutil
from datetime import datetime, timezone

import numpy as np
import pandas as pd


# --------------------------------------------------
# Helpers
# --------------------------------------------------
def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def safe_minmax(series: pd.Series) -> pd.Series:
    s = pd.to_numeric(series, errors="coerce").fillna(0.0)
    mn = s.min()
    mx = s.max()
    if pd.isna(mn) or pd.isna(mx) or mx == mn:
        return pd.Series(np.zeros(len(s)), index=s.index, dtype=float)
    return (s - mn) / (mx - mn)


def validate_required_columns(df: pd.DataFrame, required: list[str]) -> None:
    missing = [c for c in required if c not in df.columns]
    if missing:
        raise ValueError(
            "Missing required columns in dataset_profile_level.csv:\n"
            + "\n".join(f"- {c}" for c in missing)
        )


def classify_style(row: pd.Series) -> str:
    a = float(row["Aggression_norm"])
    d = float(row["Defense_norm"])
    m = float(row["Mobility_norm"])
    r = float(row["Risk_norm"])

    scores = {
        "Aggressive": a,
        "Defensive": d,
        "Mobile": m,
        "Risky": r,
    }

    top_label = max(scores, key=scores.get)
    top_value = scores[top_label]

    ordered = sorted(scores.values(), reverse=True)
    gap = ordered[0] - ordered[1] if len(ordered) > 1 else ordered[0]

    if top_value < 0.55 or gap < 0.08:
        return "Balanced"

    if top_label == "Aggressive" and d > 0.60:
        return "Aggressive-Balanced"
    if top_label == "Defensive" and a > 0.60:
        return "Defensive-Balanced"
    if top_label == "Mobile" and r > 0.60:
        return "Mobile-Risky"
    if top_label == "Risky" and a > 0.60:
        return "Aggressive-Risky"

    return top_label


# --------------------------------------------------
# Style computation
# --------------------------------------------------
def compute_style_axes_from_profile_df(df: pd.DataFrame) -> pd.DataFrame:
    required = [
        "ema_attack_rate",
        "ema_mobility_rate",
        "ema_defense_rate",
        "ema_hit_rate",
        "ema_miss_rate",
        "ema_dps_dealt",
        "ema_dps_taken",
        "ema_block_rate",
        "ema_dodge_rate",
        "ema_quick_rate",
        "ema_heavy_rate",
        "ema_special_rate",
        "ema_charge_rate",
    ]
    validate_required_columns(df, required)

    out = df.copy()

    # numeric safety
    for col in required:
        out[col] = pd.to_numeric(out[col], errors="coerce").fillna(0.0)

    # normalize EMA damage terms across profiles
    out["norm_ema_dps_dealt"] = safe_minmax(out["ema_dps_dealt"])
    out["norm_ema_dps_taken"] = safe_minmax(out["ema_dps_taken"])

    # raw axes
    out["Aggression"] = (
        0.35 * out["ema_attack_rate"]
        + 0.20 * out["ema_hit_rate"]
        + 0.15 * out["ema_quick_rate"]
        + 0.15 * out["ema_heavy_rate"]
        + 0.10 * out["ema_charge_rate"]
        + 0.05 * out["norm_ema_dps_dealt"]
    )

    out["Defense"] = (
        0.35 * out["ema_defense_rate"]
        + 0.25 * out["ema_block_rate"]
        + 0.20 * out["ema_dodge_rate"]
        + 0.20 * (1.0 - out["norm_ema_dps_taken"])
    )

    out["Mobility"] = (
        0.85 * out["ema_mobility_rate"]
        + 0.15 * out["ema_dodge_rate"]
    )

    out["Risk"] = (
        0.35 * out["ema_miss_rate"]
        + 0.30 * out["norm_ema_dps_taken"]
        + 0.20 * out["ema_special_rate"]
        + 0.15 * out["ema_charge_rate"]
    )

    # normalized axes for Unity
    out["Aggression_norm"] = safe_minmax(out["Aggression"])
    out["Defense_norm"] = safe_minmax(out["Defense"])
    out["Mobility_norm"] = safe_minmax(out["Mobility"])
    out["Risk_norm"] = safe_minmax(out["Risk"])

    # style label
    out["style_label"] = out.apply(classify_style, axis=1)

    return out


# --------------------------------------------------
# Aggregation
# --------------------------------------------------
def build_profile_summary(df: pd.DataFrame) -> pd.DataFrame:
    required = [
        "match_id",
        "profile_id",
        "profile_name",
        "won_round",
        "hit_rate",
        "miss_rate",
        "damage_dealt",
        "damage_taken",
        "ema_attack_rate",
        "ema_mobility_rate",
        "ema_defense_rate",
        "ema_hit_rate",
        "ema_miss_rate",
        "ema_dps_dealt",
        "ema_dps_taken",
        "ema_block_rate",
        "ema_dodge_rate",
        "ema_quick_rate",
        "ema_heavy_rate",
        "ema_special_rate",
        "ema_charge_rate",
    ]
    validate_required_columns(df, required)

    work = df.copy()

    work["profile_id"] = work["profile_id"].fillna("UNKNOWN_PROFILE").astype(str)
    work["profile_name"] = work["profile_name"].fillna("UNKNOWN").astype(str)

    numeric_cols = [
        "won_round",
        "hit_rate",
        "miss_rate",
        "damage_dealt",
        "damage_taken",
        "ema_attack_rate",
        "ema_mobility_rate",
        "ema_defense_rate",
        "ema_hit_rate",
        "ema_miss_rate",
        "ema_dps_dealt",
        "ema_dps_taken",
        "ema_block_rate",
        "ema_dodge_rate",
        "ema_quick_rate",
        "ema_heavy_rate",
        "ema_special_rate",
        "ema_charge_rate",
    ]
    for col in numeric_cols:
        work[col] = pd.to_numeric(work[col], errors="coerce").fillna(0.0)

    grouped = (
        work.groupby(["profile_id", "profile_name"], dropna=False)
        .agg(
            matches_count=("match_id", "nunique"),
            win_rate=("won_round", "mean"),
            hit_rate=("hit_rate", "mean"),
            miss_rate=("miss_rate", "mean"),
            avg_damage_dealt=("damage_dealt", "mean"),
            avg_damage_taken=("damage_taken", "mean"),

            # keep last EMA snapshot per profile
            ema_attack_rate=("ema_attack_rate", "last"),
            ema_mobility_rate=("ema_mobility_rate", "last"),
            ema_defense_rate=("ema_defense_rate", "last"),
            ema_hit_rate=("ema_hit_rate", "last"),
            ema_miss_rate=("ema_miss_rate", "last"),
            ema_dps_dealt=("ema_dps_dealt", "last"),
            ema_dps_taken=("ema_dps_taken", "last"),
            ema_block_rate=("ema_block_rate", "last"),
            ema_dodge_rate=("ema_dodge_rate", "last"),
            ema_quick_rate=("ema_quick_rate", "last"),
            ema_heavy_rate=("ema_heavy_rate", "last"),
            ema_special_rate=("ema_special_rate", "last"),
            ema_charge_rate=("ema_charge_rate", "last"),
        )
        .reset_index()
    )

    return grouped

def load_elo_table(path: str) -> pd.DataFrame:
    if not os.path.exists(path):
        print(f"[WARN] Elo CSV not found: {path}")
        return pd.DataFrame(columns=["profile_id", "elo_rating"])

    elo = pd.read_csv(path)
    if elo.empty:
        return pd.DataFrame(columns=["profile_id", "elo_rating"])

    # προσαρμογή σε πιθανά ονόματα στηλών
    cols = {c.lower(): c for c in elo.columns}

    profile_col = None
    for candidate in ["profile_id", "player_id", "id"]:
        if candidate in cols:
            profile_col = cols[candidate]
            break

    rating_col = None
    for candidate in ["elo_rating", "overall_elo", "rating", "elo"]:
        if candidate in cols:
            rating_col = cols[candidate]
            break

    if profile_col is None or rating_col is None:
        print("[WARN] Elo CSV missing expected columns. Expected profile_id + elo/rating column.")
        return pd.DataFrame(columns=["profile_id", "elo_rating"])

    out = elo[[profile_col, rating_col]].copy()
    out.columns = ["profile_id", "elo_rating"]
    out["profile_id"] = out["profile_id"].astype(str)
    out["elo_rating"] = pd.to_numeric(out["elo_rating"], errors="coerce").fillna(1500.0)

    return out
# --------------------------------------------------
# JSON export
# --------------------------------------------------
def to_json_payload(df: pd.DataFrame) -> dict:
    profiles = []

    sort_df = df.sort_values(
        by=["profile_name", "profile_id"],
        ascending=[True, True]
    ).reset_index(drop=True)

    for _, row in sort_df.iterrows():
        profiles.append({
            "profile_id": str(row["profile_id"]),
            "profile_name": str(row["profile_name"]),

            "style_label": str(row["style_label"]),

            "matches_count": int(row["matches_count"]),
            "elo_rating": round(float(row["elo_rating"]), 2),

            "win_rate": round(float(row["win_rate"]), 4),
            "hit_rate": round(float(row["hit_rate"]), 4),
            "miss_rate": round(float(row["miss_rate"]), 4),

            "avg_damage_dealt": round(float(row["avg_damage_dealt"]), 4),
            "avg_damage_taken": round(float(row["avg_damage_taken"]), 4),

            "aggression_raw": round(float(row["Aggression"]), 4),
            "defense_raw": round(float(row["Defense"]), 4),
            "mobility_raw": round(float(row["Mobility"]), 4),
            "risk_raw": round(float(row["Risk"]), 4),

            # normalized values used by Unity chart
            "aggression": round(float(row["Aggression_norm"]), 4),
            "defense": round(float(row["Defense_norm"]), 4),
            "mobility": round(float(row["Mobility_norm"]), 4),
            "risk": round(float(row["Risk_norm"]), 4),
        })

    return {
        "version": "1.0",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "profiles": profiles
    }


def save_json(payload: dict, out_path: str) -> None:
    parent = os.path.dirname(out_path)
    if parent:
        ensure_dir(parent)

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)


# --------------------------------------------------
# Main
# --------------------------------------------------
def main() -> None:
    parser = argparse.ArgumentParser(
        description="Export profile_analysis.json for Unity Profile Analysis screen."
    )
    parser.add_argument(
        "--elo-input",
        default=os.path.join("out_elo", "elo_overall.csv"),
        help="Path to elo_overall.csv",
    )
    parser.add_argument(
        "--input",
        default=os.path.join("out", "dataset_profile_level.csv"),
        help="Path to dataset_profile_level.csv",
    )
    parser.add_argument(
        "--output-dir",
        default=os.path.join("out", "profile_analysis"),
        help="Directory to save profile_analysis.json",
    )
    parser.add_argument(
        "--output-name",
        default="profile_analysis.json",
        help="Output JSON filename",
    )
    parser.add_argument(
    "--unity-output",
    default=None,
    help="Path to Unity StreamingAssets profile_analysis.json"
    )
    parser.add_argument(
        "--skip-unity-copy",
        action="store_true",
        help="If set, do not export/copy JSON to Unity StreamingAssets path",
    )
    args = parser.parse_args()

    if not os.path.exists(args.input):
        raise FileNotFoundError(f"Input CSV not found: {args.input}")

    ensure_dir(args.output_dir)

    df = pd.read_csv(args.input)
    if df.empty:
        raise ValueError("Input CSV is empty.")

    profile_summary = build_profile_summary(df)
    elo_df = load_elo_table(args.elo_input)
    profile_summary["profile_id"] = profile_summary["profile_id"].astype(str)
    profile_summary = profile_summary.merge(elo_df, on="profile_id", how="left")
    profile_summary["elo_rating"] = pd.to_numeric(
        profile_summary["elo_rating"], errors="coerce"
    ).fillna(1500.0)
    scored_profiles = compute_style_axes_from_profile_df(profile_summary)
    payload = to_json_payload(scored_profiles)

    # save normal output
    out_path = os.path.join(args.output_dir, args.output_name)
    save_json(payload, out_path)
    print(f"[OK] Saved JSON: {out_path}")

    # save/copy to Unity StreamingAssets
    if not args.skip_unity_copy and args.unity_output:
        save_json(payload, args.unity_output)
        print(f"[OK] Exported to Unity: {args.unity_output}")

    print(f"[OK] Profiles exported: {len(payload['profiles'])}")


if __name__ == "__main__":
    main()