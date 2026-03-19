import os
import glob
import json
import math
from collections import defaultdict
import argparse

import pandas as pd

R0 = 1500.0
K = 32.0


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--telemetry-dir", default="Telemetry")
    parser.add_argument("--out-dir", default="out_elo")
    return parser.parse_args()


def expected(ra, rb):
    return 1.0 / (1.0 + 10 ** ((rb - ra) / 400.0))


def update(ra, rb, sa):
    ea = expected(ra, rb)
    return ra + K * (sa - ea)


def safe_get(d, *keys, default=None):
    for k in keys:
        if isinstance(d, dict) and k in d and d[k] not in (None, ""):
            return d[k]
    return default


def parse_file(fp):
    with open(fp, "r", encoding="utf-8") as f:
        doc = json.load(f)

    meta = doc.get("meta", {}) if isinstance(doc.get("meta", {}), dict) else {}

    # raw ids
    raw_p1 = safe_get(meta, "p1Id", "player1Id", "p1")
    raw_p2 = safe_get(meta, "p2Id", "player2Id", "p2")
    winner = safe_get(meta, "winnerId", "winner", "winnerPlayerId")

    # profile ids (preferred identity if present)
    p1_profile = safe_get(meta, "p1ProfileId")
    p2_profile = safe_get(meta, "p2ProfileId")

    # profile names
    p1_profile_name = safe_get(meta, "p1ProfileName", default=p1_profile)
    p2_profile_name = safe_get(meta, "p2ProfileName", default=p2_profile)

    # choose identity keys
    p1 = p1_profile if p1_profile not in (None, "", "GUEST") else raw_p1
    p2 = p2_profile if p2_profile not in (None, "", "GUEST") else raw_p2

    # character context
    p1_char = safe_get(meta, "p1Character", "p1Char", "player1Character")
    p2_char = safe_get(meta, "p2Character", "p2Char", "player2Character")

    match_id = safe_get(meta, "matchId") or safe_get(doc, "matchId") or os.path.basename(fp)
    round_number = safe_get(meta, "roundNumber")

    # --- DAMAGE AGGREGATION ---
    damage = {
        raw_p1: 0.0,
        raw_p2: 0.0,
    }

    events = doc.get("events", [])
    for e in events:
        if not isinstance(e, dict):
            continue

        if e.get("type") == "DamageApplied" or e.get("eventType") == "DamageApplied":
            attacker = safe_get(e, "attacker", "attackerId", "sourceId")
            dmg = safe_get(e, "damage", "finalDamage", default=0.0)

            try:
                dmg = float(dmg)
            except Exception:
                dmg = 0.0

            if attacker in damage:
                damage[attacker] += dmg

    damage_p1 = damage.get(raw_p1, 0.0)
    damage_p2 = damage.get(raw_p2, 0.0)

    # --- FINAL HP EXTRACTION ---
    final_hp = {
        raw_p1: None,
        raw_p2: None,
    }

    max_hp_seen = {
        raw_p1: None,
        raw_p2: None,
    }

    for e in events:
        if not isinstance(e, dict):
            continue

        if e.get("type") != "DamageApplied" and e.get("eventType") != "DamageApplied":
            continue

        defender = safe_get(e, "defenderId", "defender")
        hp_before = safe_get(e, "hpDefenderBefore", default=None)
        hp_after = safe_get(e, "hpDefenderAfter", default=None)

        if defender not in final_hp:
            continue

        try:
            if hp_before is not None:
                hp_before = float(hp_before)
            if hp_after is not None:
                hp_after = float(hp_after)
        except Exception:
            continue

        if hp_before is not None:
            if max_hp_seen[defender] is None:
                max_hp_seen[defender] = hp_before
            else:
                max_hp_seen[defender] = max(max_hp_seen[defender], hp_before)

        if hp_after is not None:
            final_hp[defender] = max(0.0, hp_after)

    # determine score for p1 (classic fallback, overridden later)
    sa = None
    if winner == p1:
        sa = 1.0
    elif winner == p2:
        sa = 0.0
    elif winner == raw_p1:
        sa = 1.0
    elif winner == raw_p2:
        sa = 0.0

    return {
        "file": os.path.basename(fp),
        "match_id": match_id,
        "round_number": round_number,
        "p1": p1,
        "p2": p2,
        "raw_p1": raw_p1,
        "raw_p2": raw_p2,
        "p1_profile_name": p1_profile_name,
        "p2_profile_name": p2_profile_name,
        "winner": winner,
        "p1_char": p1_char,
        "p2_char": p2_char,
        "sa": sa,
        "damage_p1": damage_p1,
        "damage_p2": damage_p2,
        "final_hp_p1": final_hp.get(raw_p1),
        "final_hp_p2": final_hp.get(raw_p2),
        "max_hp_p1": max_hp_seen.get(raw_p1),
        "max_hp_p2": max_hp_seen.get(raw_p2),
    }


def main():
    args = parse_args()
    TELEMETRY_DIR = args.telemetry_dir
    OUT_DIR = args.out_dir

    os.makedirs(OUT_DIR, exist_ok=True)

    files = sorted(glob.glob(os.path.join(TELEMETRY_DIR, "*.json")))
    if not files:
        raise FileNotFoundError(f"No JSON files in {TELEMETRY_DIR}")

    overall = {}   # kept for match-by-match history/debug
    per_char = {}  # (profile_id, character) -> rating
    profile_name_map = {}  # profile_id -> profile_name
    hist = []
    used = 0
    skipped = 0

    for fp in files:
        try:
            info = parse_file(fp)
            p1, p2, sa = info["p1"], info["p2"], info["sa"]

            if info.get("p1_profile_name"):
                profile_name_map[p1] = info["p1_profile_name"]
            if info.get("p2_profile_name"):
                profile_name_map[p2] = info["p2_profile_name"]

            # skip invalid identities
            if not p1 or not p2 or p1 == "NONE" or p2 == "NONE":
                skipped += 1
                continue

            damage_a = float(info.get("damage_p1", 0.0))
            damage_b = float(info.get("damage_p2", 0.0))

            final_hp_p1 = info.get("final_hp_p1")
            final_hp_p2 = info.get("final_hp_p2")

            max_hp_candidates = [
                info.get("max_hp_p1"),
                info.get("max_hp_p2"),
                100.0,
            ]
            max_hp = max(float(x) for x in max_hp_candidates if x is not None)

            winner = info["winner"]

            # fallback if final hp missing
            if final_hp_p1 is None:
                if winner == info["p2"] or winner == info["raw_p2"]:
                    final_hp_p1 = 0.0
                else:
                    final_hp_p1 = max_hp

            if final_hp_p2 is None:
                if winner == info["p1"] or winner == info["raw_p1"]:
                    final_hp_p2 = 0.0
                else:
                    final_hp_p2 = max_hp

            final_hp_p1 = max(0.0, float(final_hp_p1))
            final_hp_p2 = max(0.0, float(final_hp_p2))

            margin = abs(final_hp_p1 - final_hp_p2) / max(max_hp, 1.0)
            margin = max(0.0, min(1.0, margin))

            # weighted score from final HP margin
            s_win = 0.75 + 0.25 * margin
            s_lose = 1.0 - s_win

            # override sa
            if winner == p1 or winner == info["raw_p1"]:
                sa = s_win
            elif winner == p2 or winner == info["raw_p2"]:
                sa = s_lose

            if sa is None:
                skipped += 1
                continue

            sb = 1.0 - sa

            # direct overall ladder kept only for history/debug
            r1 = overall.get(p1, R0)
            r2 = overall.get(p2, R0)
            r1_new = update(r1, r2, sa)
            r2_new = update(r2, r1, sb)

            overall[p1] = r1_new
            overall[p2] = r2_new

            # per-character Elo
            r1c_new = None
            r2c_new = None
            if info["p1_char"] and info["p2_char"]:
                k1 = (p1, info["p1_char"])
                k2 = (p2, info["p2_char"])

                r1c = per_char.get(k1, R0)
                r2c = per_char.get(k2, R0)

                r1c_new = update(r1c, r2c, sa)
                r2c_new = update(r2c, r1c, sb)

                per_char[k1] = r1c_new
                per_char[k2] = r2c_new

            winner_profile_id = p1 if sa > sb else p2
            winner_profile_name = profile_name_map.get(winner_profile_id, winner_profile_id)

            hist.append({
                "file": info["file"],
                "match_id": info["match_id"],
                "round_number": info["round_number"],

                "p1_profile_id": p1,
                "p1_profile_name": profile_name_map.get(p1, p1),
                "p1_character": info["p1_char"],

                "p2_profile_id": p2,
                "p2_profile_name": profile_name_map.get(p2, p2),
                "p2_character": info["p2_char"],

                "winner_profile_id": winner_profile_id,
                "winner_profile_name": winner_profile_name,

                "p1_score": sa,
                "p2_score": sb,

                "p1_elo_before": r1,
                "p2_elo_before": r2,
                "p1_elo_after": r1_new,
                "p2_elo_after": r2_new,

                "p1_char_elo_after": r1c_new,
                "p2_char_elo_after": r2c_new,

                "damage_p1": damage_a,
                "damage_p2": damage_b,
                "final_hp_p1": final_hp_p1,
                "final_hp_p2": final_hp_p2,
                "max_hp": max_hp,
                "hp_margin": margin,
            })
            used += 1

        except Exception as e:
            skipped += 1
            print(f"[SKIP] {os.path.basename(fp)} -> {e}")

    if used == 0:
        raise RuntimeError(
            "Parsed 0 usable matches for Elo. "
            "Likely winnerId/p1Id/p2Id mismatch. "
            "Open one JSON and confirm meta has p1Id, p2Id, winnerId."
        )

    # save history
    pd.DataFrame(hist).to_csv(os.path.join(OUT_DIR, "elo_history.csv"), index=False)

    # -----------------------------
    # Compute weighted overall Elo from per-character Elo
    # -----------------------------
    char_counts = defaultdict(int)
    total_matches = defaultdict(int)

    for h in hist:
        p1 = h["p1_profile_id"]
        p2 = h["p2_profile_id"]
        c1 = h["p1_character"]
        c2 = h["p2_character"]

        if p1 and c1 and p1 != "NONE":
            char_counts[(p1, c1)] += 1
            total_matches[p1] += 1

        if p2 and c2 and p2 != "NONE":
            char_counts[(p2, c2)] += 1
            total_matches[p2] += 1

    overall_weighted = {}

    for (p, c), elo in per_char.items():
        if not p or p == "NONE":
            continue

        matches = char_counts.get((p, c), 0)
        total = total_matches.get(p, 0)

        if matches <= 0 or total <= 0:
            continue

        pick_rate = matches / total
        weight = math.sqrt(pick_rate)

        if p not in overall_weighted:
            overall_weighted[p] = {
                "num": 0.0,
                "den": 0.0,
                "chars": set(),
            }

        overall_weighted[p]["num"] += elo * weight
        overall_weighted[p]["den"] += weight
        overall_weighted[p]["chars"].add(c)

    final_overall = {}

    for p, data in overall_weighted.items():
        if data["den"] <= 0:
            continue

        elo = data["num"] / data["den"]

        # very light diversity penalty
        diversity = len(data["chars"])
        if diversity < 2:
            elo *= 0.97
        elif diversity < 3:
            elo *= 0.985

        final_overall[p] = elo

    df_overall = pd.DataFrame([
        {
            "profile_id": p,
            "profile_name": profile_name_map.get(p, p),
            "overall_elo": final_overall[p],
        }
        for p in final_overall
    ])

    if not df_overall.empty:
        df_overall = df_overall.sort_values("overall_elo", ascending=False)

    df_overall.to_csv(os.path.join(OUT_DIR, "elo_overall.csv"), index=False)

    # per-character output
    df_char = pd.DataFrame([
        {
            "profile_id": p,
            "profile_name": profile_name_map.get(p, p),
            "character": c,
            "per_char_elo": r,
        }
        for (p, c), r in per_char.items()
        if p and p != "NONE"
    ])

    if not df_char.empty:
        df_char = df_char.sort_values("per_char_elo", ascending=False)

    df_char.to_csv(os.path.join(OUT_DIR, "elo_per_character.csv"), index=False)

    print(f"[OK] Elo computed. Used={used}  Skipped={skipped}")
    print("\nTop Overall Elo:")
    print(df_overall.head(10).to_string(index=False))
    print("\nTop Per-Character Elo:")
    print(df_char.head(10).to_string(index=False))


if __name__ == "__main__":
    main()