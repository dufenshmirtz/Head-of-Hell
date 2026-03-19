import json
import glob
import os
from dataclasses import dataclass
from typing import Dict, Any, List, Optional, Tuple
import pandas as pd
import numpy as np


# ----------------------------
# Configuration (edit me)
# ----------------------------

TELEMETRY_DIR = "Telemetry"  # folder with per-round JSON files
OUT_DIR = "out"
ROUND_CSV = os.path.join(OUT_DIR, "dataset_round_level.csv")
PROFILE_CSV = os.path.join(OUT_DIR, "dataset_profile_level.csv")

# Map your Action.actionType strings into categories.
# Keep it deterministic and stable across the whole dataset.
ACTION_CATEGORY_MAP = {
    # Attacks
    "Quick": "attack",
    "Heavy": "attack",
    "Special": "attack",
    "ChargeStart": "attack",
    "ChargeRelease": "attack",

    # Mobility (add your actual actionType names!)
    "Dash": "mobility",
    "BackDash": "mobility",
    "Jump": "mobility",
    "MoveLeft": "mobility",
    "MoveRight": "mobility",
    "MoveStop": "mobility",
    "DropPlatform": "mobility",

    # Defense (add your actual names!)
    "BlockStart": "defense",
    "BlockHold": "defense",
    "Parry": "defense",
    "Dodge": "defense",
    "Roll" : "defense",
}

# Optional: weights for EMA profiling
EMA_ALPHA = 0.35  # 0.2–0.4 typical


# ----------------------------
# Helpers / parsing
# ----------------------------

def safe_get(d: Dict[str, Any], *keys, default=None):
    cur = d
    for k in keys:
        if not isinstance(cur, dict) or k not in cur:
            return default
        cur = cur[k]
    return cur

def infer_player_ids(meta: Dict[str, Any]) -> Tuple[Optional[str], Optional[str]]:
    # tries common patterns
    p1 = meta.get("p1Id") or meta.get("player1Id") or meta.get("p1") or None
    p2 = meta.get("p2Id") or meta.get("player2Id") or meta.get("p2") or None
    return p1, p2

def infer_winner_id(meta: Dict[str, Any]) -> Optional[str]:
    return meta.get("winnerId") or meta.get("winner") or meta.get("winnerPlayerId") or None

def as_float(x, default=0.0):
    try:
        return float(x)
    except Exception:
        return default

def normalize_rate(count: float, duration: float) -> float:
    if duration <= 1e-9:
        return 0.0
    return count / duration

def ratio(num: float, den: float) -> float:
    if den <= 1e-9:
        return 0.0
    return num / den


# ----------------------------
# Core feature extraction per round
# ----------------------------

def extract_round_features(doc: Dict[str, Any], filename: str) -> List[Dict[str, Any]]:
    meta = doc.get("meta", {}) if isinstance(doc.get("meta", {}), dict) else {}
    events = doc.get("events", []) if isinstance(doc.get("events", []), list) else []

    duration = as_float(doc.get("durationSeconds") or meta.get("durationSeconds") or 0.0)
    match_id = doc.get("matchId") or meta.get("matchId") or os.path.basename(filename)

    round_number = meta.get("roundNumber", None)
    stage = meta.get("map") or meta.get("stage") or meta.get("mapName") or None
    mode = meta.get("mode") or None

    p1, p2 = infer_player_ids(meta)
    winner_id = infer_winner_id(meta)
        # --- NEW: profile identity from meta ---
    p1_profile_id = meta.get("p1ProfileId") or ""
    p1_profile_name = meta.get("p1ProfileName") or ""
    p2_profile_id = meta.get("p2ProfileId") or ""
    p2_profile_name = meta.get("p2ProfileName") or ""

    def identity_for_slot(slot_id: str):
        """Return (profile_id, profile_name) for slot_id ('P1'/'P2')."""
        if slot_id == p1:
            return p1_profile_id, p1_profile_name
        if slot_id == p2:
            return p2_profile_id, p2_profile_name
        return "", ""

    def player_key(slot_id: str, profile_id: str):
        """Primary key used for grouping across matches."""
        if profile_id and profile_id not in ("GUEST", "NONE"):
            return profile_id
        return slot_id  # fallback
    # We'll aggregate per playerId
    agg: Dict[str, Dict[str, float]] = {}

    def ensure(pid: str):
        if pid not in agg:
            agg[pid] = {
                "actions_total": 0,
                "actions_attack": 0,
                "actions_mobility": 0,
                "actions_defense": 0,

                "hit_attempts": 0,
                "misses": 0,

                "damage_dealt": 0.0,
                "damage_taken": 0.0,

                "blocked_hits_as_defender": 0,  # from DamageApplied where blocked==true
                "dodged_hits_as_defender": 0,   # from DamageApplied where dodged==true
                "hits_received": 0,             # total DamageApplied events against defender (including 0 dmg)
                "hits_landed": 0,               # DamageApplied events by attacker with actualDamage>0 (or could count all interactions)
            }

    # Parse events
    for e in events:
        etype = e.get("eventType") or e.get("type")  # tolerate both
        if etype == "Action":
            pid = e.get("playerId") or e.get("actorId") or e.get("attackerId")
            if not pid:
                continue
            ensure(pid)
            agg[pid]["actions_total"] += 1

            a = e.get("actionType") or e.get("action") or ""
            cat = ACTION_CATEGORY_MAP.get(a, None)
            if cat == "attack":
                agg[pid]["actions_attack"] += 1
            elif cat == "mobility":
                agg[pid]["actions_mobility"] += 1
            elif cat == "defense":
                agg[pid]["actions_defense"] += 1

        elif etype == "HitAttempt":
            attacker = e.get("attackerId") or e.get("playerId")
            if not attacker:
                continue
            ensure(attacker)
            agg[attacker]["hit_attempts"] += 1

        elif etype == "Miss":
            attacker = e.get("attackerId") or e.get("playerId")
            if not attacker:
                continue
            ensure(attacker)
            agg[attacker]["misses"] += 1

        elif etype == "DamageApplied":
            attacker = e.get("attackerId")
            defender = e.get("defenderId")
            dmg = as_float(e.get("actualDamage") or e.get("finalDamage") or 0.0)
            blocked = bool(e.get("blocked") or e.get("wasBlocked") or False)
            dodged = bool(e.get("dodged") or e.get("wasDodged") or False)

            if attacker:
                ensure(attacker)
                agg[attacker]["damage_dealt"] += dmg
                if dmg > 0:
                    agg[attacker]["hits_landed"] += 1

            if defender:
                ensure(defender)
                agg[defender]["damage_taken"] += dmg
                agg[defender]["hits_received"] += 1
                if blocked:
                    agg[defender]["blocked_hits_as_defender"] += 1
                if dodged:
                    agg[defender]["dodged_hits_as_defender"] += 1

        else:
            # ignore unknown events
            pass

    # If meta has players but no events for one (rare), ensure rows exist
    for pid in [p1, p2]:
        if pid:
            ensure(pid)

    # Build rows per player
    rows: List[Dict[str, Any]] = []
    for pid, a in agg.items():
        # rates
        attack_rate = normalize_rate(a["actions_attack"], duration)
        mobility_rate = normalize_rate(a["actions_mobility"], duration)
        defense_rate = normalize_rate(a["actions_defense"], duration)

        hit_rate = ratio(a["hits_landed"], a["hit_attempts"])
        miss_rate = ratio(a["misses"], a["hit_attempts"])
        dps_dealt = normalize_rate(a["damage_dealt"], duration)
        dps_taken = normalize_rate(a["damage_taken"], duration)

        block_rate = ratio(a["blocked_hits_as_defender"], a["hits_received"])
        dodge_rate = ratio(a["dodged_hits_as_defender"], a["hits_received"])

        rows.append({
            "match_id": match_id,
            "round_number": round_number,
            "stage": stage,
            "mode": mode,
            "duration_s": duration,

                      # --- identity ---
            "slot_id": pid,  # original P1/P2 from gameplay
            "is_p1": (pid == p1) if p1 else None,
            "is_p2": (pid == p2) if p2 else None,
            "won_round": (pid == winner_id) if winner_id else None,  # winnerId is slot-based

            "profile_id": identity_for_slot(pid)[0],
            "profile_name": identity_for_slot(pid)[1],

            # IMPORTANT: player_id becomes profile-based key for aggregation/EMA/clustering
            "player_id": player_key(pid, identity_for_slot(pid)[0]),

            # primitive counts
            "actions_total": a["actions_total"],
            "actions_attack": a["actions_attack"],
            "actions_mobility": a["actions_mobility"],
            "actions_defense": a["actions_defense"],
            "hit_attempts": a["hit_attempts"],
            "misses": a["misses"],
            "hits_landed": a["hits_landed"],
            "hits_received": a["hits_received"],
            "damage_dealt": a["damage_dealt"],
            "damage_taken": a["damage_taken"],
            "blocked_hits_as_defender": a["blocked_hits_as_defender"],
            "dodged_hits_as_defender": a["dodged_hits_as_defender"],

            # ML-ready normalized features
            "attack_rate": attack_rate,
            "mobility_rate": mobility_rate,
            "defense_rate": defense_rate,
            "hit_rate": hit_rate,
            "miss_rate": miss_rate,
            "dps_dealt": dps_dealt,
            "dps_taken": dps_taken,
            "block_rate": block_rate,
            "dodge_rate": dodge_rate,
        })

    return rows


# ----------------------------
# Profiling layer (EMA per player)
# ----------------------------

FEATURE_COLS = [
    "attack_rate", "mobility_rate", "defense_rate",
    "hit_rate", "miss_rate",
    "dps_dealt", "dps_taken",
    "block_rate", "dodge_rate"
]

def build_ema_profiles(df_round: pd.DataFrame, alpha: float = EMA_ALPHA) -> pd.DataFrame:
    # Sort by (player_id, round order). If round_number is missing, fallback to match_id as string.
    sort_cols = ["player_id"]
    if df_round["round_number"].notna().any():
        sort_cols.append("round_number")
    else:
        sort_cols.append("match_id")
    df = df_round.sort_values(sort_cols).copy()

    # EMA per player
    out_rows = []
    for pid, g in df.groupby("player_id", sort=False):
        g = g.copy()
        # Start EMA at first observation
        ema = None
        for _, row in g.iterrows():
            x = row[FEATURE_COLS].astype(float).to_numpy()
            if ema is None:
                ema = x
            else:
                ema = alpha * x + (1 - alpha) * ema

            r = row.to_dict()
            for i, col in enumerate(FEATURE_COLS):
                r[f"ema_{col}"] = float(ema[i])
            out_rows.append(r)

    return pd.DataFrame(out_rows)


# ----------------------------
# Main
# ----------------------------

def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    files = sorted(glob.glob(os.path.join(TELEMETRY_DIR, "*.json")))
    if not files:
        raise FileNotFoundError(f"No JSON files found in: {TELEMETRY_DIR}")

    all_rows = []
    bad_files = 0

    for fp in files:
        try:
            with open(fp, "r", encoding="utf-8") as f:
                doc = json.load(f)
            rows = extract_round_features(doc, fp)
            all_rows.extend(rows)
        except Exception as ex:
            bad_files += 1
            print(f"[WARN] Failed parsing {fp}: {ex}")

    if not all_rows:
        raise RuntimeError("Parsed 0 rows. Check schema / Telemetry JSON content.")

    df_round = pd.DataFrame(all_rows)

    # Basic cleanup for ML: fill missing won_round with NaN, keep as float
    if "won_round" in df_round.columns:
        df_round["won_round"] = df_round["won_round"].astype("float")

    df_round.to_csv(ROUND_CSV, index=False)
    print(f"[OK] Round-level dataset saved: {ROUND_CSV}  (rows={len(df_round)})")
    if bad_files:
        print(f"[INFO] Bad files skipped: {bad_files}")

    df_profile = build_ema_profiles(df_round, alpha=EMA_ALPHA)
    df_profile.to_csv(PROFILE_CSV, index=False)
    print(f"[OK] Profile-level dataset saved: {PROFILE_CSV}  (rows={len(df_profile)})")

    # Quick sanity stats
    print("\n--- Feature means (round-level) ---")
    print(df_round[FEATURE_COLS].mean(numeric_only=True).round(4))


if __name__ == "__main__":
    main()
