import json
import glob
import os
from collections import Counter, defaultdict

TELEMETRY_DIR = "Telemetry"

# ----- What we consider "must-have" for your project -----
REQUIRED_EVENT_TYPES = {"Action", "HitAttempt", "Miss", "DamageApplied"}

REQUIRED_META_KEYS_ANY_OF = {
    "p1Id": {"p1Id", "player1Id"},
    "p2Id": {"p2Id", "player2Id"},
    "p1Character": {"p1Character", "player1Character"},
    "p2Character": {"p2Character", "player2Character"},
    "winnerId": {"winnerId", "winner", "winnerPlayerId"},
    "roundNumber": {"roundNumber"},
    "map": {"map", "stage", "mapName"},
    "mode": {"mode"},
}

REQUIRED_TOP_LEVEL_KEYS = {"events"}  # durationSeconds/matchId recommended but can be in meta too

# Required fields per eventType (based on your actual JSON sample)
REQUIRED_FIELDS_BY_EVENT = {
    "Action": {"t", "frame", "actorId", "actionType"},
    "HitAttempt": {"t", "frame", "attackerId", "defenderId", "moveType"},
    "Miss": {"t", "frame", "attackerId", "moveType"},
    "DamageApplied": {"t", "frame", "attackerId", "defenderId", "moveType", "sourceType", "finalDamage", "wasBlocked", "wasDodged"},
}

# Optional-but-important fields (recommended additions)
RECOMMENDED_FIELDS_BY_EVENT = {
    # For playstyle/ML richness
    "DamageApplied": {"hpDefenderBefore", "hpDefenderAfter", "distance"},
    "HitAttempt": {"distance"},
    "Miss": {"distance"},
    # If you implement snapshots:
    "PosSnapshot": {"t", "frame", "p1x", "p1y", "p2x", "p2y", "distance"},
}


def pick_first_key(dct, keyset):
    for k in keyset:
        if isinstance(dct, dict) and k in dct and dct[k] not in ("", None):
            return k, dct[k]
    return None, None


def main():
    files = sorted(glob.glob(os.path.join(TELEMETRY_DIR, "*.json")))
    if not files:
        print(f"[ERROR] No JSON files found in '{TELEMETRY_DIR}/'.")
        return

    total_files = 0
    bad_files = 0

    event_type_counts = Counter()
    fields_seen = defaultdict(Counter)          # eventType -> field -> count of appearance
    events_with_missing_required = defaultdict(int)  # eventType -> count missing required fields
    sample_missing_required = []                # collect a few examples

    meta_presence = Counter()
    missing_meta_by_file = []

    top_level_missing = 0
    duration_missing = 0
    matchid_missing = 0
    meta_missing = 0

    for fp in files:
        total_files += 1
        try:
            with open(fp, "r", encoding="utf-8") as f:
                doc = json.load(f)
        except Exception as ex:
            bad_files += 1
            print(f"[WARN] Failed to parse {fp}: {ex}")
            continue

        # top-level checks
        if not isinstance(doc, dict):
            bad_files += 1
            print(f"[WARN] {fp} is not a JSON object.")
            continue

        for k in REQUIRED_TOP_LEVEL_KEYS:
            if k not in doc:
                top_level_missing += 1

        if "durationSeconds" not in doc and (not isinstance(doc.get("meta"), dict) or "durationSeconds" not in doc["meta"]):
            duration_missing += 1
        if "matchId" not in doc and (not isinstance(doc.get("meta"), dict) or "matchId" not in doc["meta"]):
            matchid_missing += 1

        meta = doc.get("meta")
        if not isinstance(meta, dict):
            meta_missing += 1
            meta = {}

        # meta checks (any-of keys)
        file_missing_meta = []
        for canonical, options in REQUIRED_META_KEYS_ANY_OF.items():
            kk, vv = pick_first_key(meta, options)
            if kk is None:
                file_missing_meta.append(canonical)
            else:
                meta_presence[canonical] += 1

        if file_missing_meta:
            missing_meta_by_file.append((os.path.basename(fp), file_missing_meta))

        # event parsing
        events = doc.get("events", [])
        if not isinstance(events, list):
            bad_files += 1
            print(f"[WARN] {fp} 'events' is not a list.")
            continue

        for e in events:
            if not isinstance(e, dict):
                continue

            etype = e.get("eventType") or e.get("type")
            if not etype:
                continue

            event_type_counts[etype] += 1

            # collect fields seen
            for field in e.keys():
                fields_seen[etype][field] += 1

            # required fields check per event type
            req = REQUIRED_FIELDS_BY_EVENT.get(etype)
            if req:
                missing = [f for f in req if f not in e]
                if missing:
                    events_with_missing_required[etype] += 1
                    if len(sample_missing_required) < 12:
                        sample_missing_required.append({
                            "file": os.path.basename(fp),
                            "eventType": etype,
                            "missingFields": missing,
                            "exampleKeys": sorted(list(e.keys()))[:20]
                        })

    # ---------------- Report ----------------
    print("\n================= TELEMETRY CHECK REPORT =================\n")
    print(f"Files scanned: {total_files}")
    print(f"Bad/unparsed files: {bad_files}\n")

    print("---- Event Types Present ----")
    if not event_type_counts:
        print("[ERROR] No events found across files.")
    else:
        for et, c in event_type_counts.most_common():
            print(f"{et:14}  {c}")
    print()

    # Required event types presence
    print("---- Required Event Types Check ----")
    present_types = set(event_type_counts.keys())
    missing_types = sorted(list(REQUIRED_EVENT_TYPES - present_types))
    if missing_types:
        print("[FAIL] Missing required event types:", ", ".join(missing_types))
    else:
        print("[OK] All required event types exist.")
    print()

    # Meta coverage
    print("---- Meta Coverage (must-have keys) ----")
    for canonical in REQUIRED_META_KEYS_ANY_OF.keys():
        print(f"{canonical:12}: present in {meta_presence[canonical]}/{max(total_files - bad_files,1)} files")
    if missing_meta_by_file:
        print("\n[WARN] Some files missing meta fields (showing up to 8):")
        for fn, miss in missing_meta_by_file[:8]:
            print(f"  - {fn}: missing {miss}")
    else:
        print("\n[OK] Meta fields look complete across files.")
    print()

    # Top-level checks
    print("---- Top-level sanity ----")
    if top_level_missing:
        print(f"[WARN] Some files missing top-level 'events' key: {top_level_missing}")
    if duration_missing:
        print(f"[WARN] durationSeconds missing (top-level or meta) in {duration_missing} files")
    else:
        print("[OK] durationSeconds found in all files (top-level or meta)")
    if matchid_missing:
        print(f"[WARN] matchId missing (top-level or meta) in {matchid_missing} files")
    else:
        print("[OK] matchId found in all files (top-level or meta)")
    if meta_missing:
        print(f"[WARN] meta missing or not a dict in {meta_missing} files")
    else:
        print("[OK] meta exists as object in all files")
    print()

    # Required fields per event
    print("---- Required Fields per EventType ----")
    for etype, req in REQUIRED_FIELDS_BY_EVENT.items():
        if etype not in fields_seen:
            print(f"{etype:14} [MISSING EVENT TYPE IN DATA]")
            continue
        missing_fields_globally = [f for f in req if fields_seen[etype].get(f, 0) == 0]
        if missing_fields_globally:
            print(f"{etype:14} [FAIL] Never seen fields: {missing_fields_globally}")
        else:
            print(f"{etype:14} [OK] Required fields are present somewhere in dataset.")
        miss_count = events_with_missing_required.get(etype, 0)
        if miss_count:
            print(f"  [WARN] {miss_count} events of type {etype} are missing some required fields (see samples below).")
    print()

    if sample_missing_required:
        print("---- Samples of Events Missing Required Fields (up to 12) ----")
        for s in sample_missing_required:
            print(f"- {s['file']} | {s['eventType']} missing {s['missingFields']} | keys={s['exampleKeys']}")
        print()

    # Recommended fields
    print("---- Recommended Fields (Optional but valuable) ----")
    for etype, rec in RECOMMENDED_FIELDS_BY_EVENT.items():
        if etype not in fields_seen:
            print(f"{etype:14} [INFO] EventType not present (ok if you didn't implement it). Recommended fields: {sorted(list(rec))}")
            continue
        missing = [f for f in rec if fields_seen[etype].get(f, 0) == 0]
        if missing:
            print(f"{etype:14} [SUGGEST] Consider adding fields: {missing}")
        else:
            print(f"{etype:14} [OK] All recommended fields present.")
    print()

    # Field inventory (quick view)
    print("---- Field Inventory (Top 20 fields per eventType) ----")
    for etype, counter in fields_seen.items():
        top = counter.most_common(20)
        print(f"{etype}: {', '.join([f'{k}({v})' for k,v in top])}")
    print("\n==========================================================\n")


if __name__ == "__main__":
    main()
