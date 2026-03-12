import os
import json
import glob
from collections import Counter, defaultdict

import numpy as np

# =========================
# CONFIG
# =========================
RAW_LOGS_DIR = os.path.join(
    os.path.expanduser("~"),
    "Documents",
    "My Games",
    "Head of Hell",
    "ImitationLogs",
    "game_logs",
)

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
BASE_DIR = os.path.dirname(SCRIPT_DIR)

LOG_DIR = RAW_LOGS_DIR
OUTPUT_DIR = os.path.join(BASE_DIR, "processed")
SAVE_DATASET = True


# =========================
# HELPERS
# =========================
ACTION_NAMES = [
    "move",
    "jump",
    "drop",
    "light",
    "heavy",
    "block",
    "special",
    "charge",
    "parry",
]

CHARACTER_ID_TO_NAME = {
    0: "Steelager",
    1: "Vander",
    2: "Rager",
    3: "Skipler",
    4: "Fin",
    5: "LazyBigus",
    6: "Lithra",
    7: "Chiback",
    8: "Lupen",
    9: "Visvia",
}


def safe_mkdir(path: str):
    os.makedirs(path, exist_ok=True)


def load_jsonl_file(path: str):
    rows = []
    with open(path, "r", encoding="utf-8-sig") as f:
        for line_num, line in enumerate(f, start=1):
            line = line.strip()
            if not line:
                continue
            try:
                rows.append(json.loads(line))
            except json.JSONDecodeError as e:
                print(f"[WARN] JSON decode error in {path} line {line_num}: {e}")
    return rows


def discover_jsonl_files(log_dir: str):
    return sorted(
        glob.glob(os.path.join(log_dir, "**", "*.jsonl"), recursive=True)
    )


def action_tuple(act):
    if not isinstance(act, list):
        return None
    return tuple(act)


def get_char_name(cid: int):
    return CHARACTER_ID_TO_NAME.get(cid, f"Unknown({cid})")


def summarize_counter(counter: Counter, top_k=None):
    items = counter.most_common(top_k)
    return items


# =========================
# MAIN ANALYSIS
# =========================
def analyze_logs(log_dir: str, output_dir: str, save_dataset: bool = True):
    safe_mkdir(output_dir)

    files = discover_jsonl_files(log_dir)
    if not files:
        print(f"[ERROR] No .jsonl files found in: {log_dir}")
        return

    all_rows = []
    file_to_rows = {}
    obs_lengths = Counter()
    act_lengths = Counter()

    episode_keys = set()
    file_frame_counts = {}

    controlled_player_counter = Counter()
    self_char_counter = Counter()
    opp_char_counter = Counter()
    matchup_counter = Counter()
    outcome_counter = Counter()
    action_counter = Counter()

    branch_value_counters = [Counter() for _ in range(len(ACTION_NAMES))]
    episode_lengths = defaultdict(int)

    done_count = 0
    invalid_rows = 0

    print(f"[INFO] Found {len(files)} jsonl files.")

    for path in files:
        rows = load_jsonl_file(path)
        file_to_rows[path] = rows
        file_frame_counts[path] = len(rows)

        for row in rows:
            all_rows.append(row)

            obs = row.get("obs", [])
            act = row.get("act", [])
            session_id = row.get("session_id", "unknown_session")
            episode = row.get("episode", -1)
            step = row.get("step", -1)

            obs_lengths[len(obs)] += 1
            act_lengths[len(act)] += 1

            ep_key = (session_id, episode)
            episode_keys.add(ep_key)
            episode_lengths[ep_key] += 1

            controlled_player_counter[row.get("controlled_player", "unknown")] += 1

            self_cid = row.get("self_character_id", -1)
            opp_cid = row.get("opp_character_id", -1)
            self_char_counter[self_cid] += 1
            opp_char_counter[opp_cid] += 1
            matchup_counter[(self_cid, opp_cid)] += 1

            outcome = row.get("outcome", "none")
            outcome_counter[outcome] += 1

            if row.get("done", False):
                done_count += 1

            a_tuple = action_tuple(act)
            if a_tuple is None:
                invalid_rows += 1
                continue

            action_counter[a_tuple] += 1

            if len(act) == len(ACTION_NAMES):
                for i, value in enumerate(act):
                    branch_value_counters[i][value] += 1
            else:
                invalid_rows += 1

    total_frames = len(all_rows)
    total_episodes = len(episode_keys)

    print("\n==============================")
    print("DATASET SUMMARY")
    print("==============================")
    print(f"Files              : {len(files)}")
    print(f"Frames             : {total_frames}")
    print(f"Episodes           : {total_episodes}")
    print(f"Rows with done=True: {done_count}")
    print(f"Invalid rows       : {invalid_rows}")

    print("\nObservation lengths:")
    for k, v in obs_lengths.most_common():
        print(f"  obs len {k}: {v} rows")

    print("\nAction lengths:")
    for k, v in act_lengths.most_common():
        print(f"  act len {k}: {v} rows")

    if len(obs_lengths) > 1:
        print("\n[WARN] Observation vector length is not constant across dataset.")
        print("       Αυτό συνήθως σημαίνει διαφορετικό schema version στα logs.")

    if len(act_lengths) > 1:
        print("\n[WARN] Action vector length is not constant across dataset.")

    if total_episodes > 0:
        avg_ep_len = np.mean(list(episode_lengths.values()))
        min_ep_len = np.min(list(episode_lengths.values()))
        max_ep_len = np.max(list(episode_lengths.values()))
        print("\nEpisode lengths:")
        print(f"  avg: {avg_ep_len:.2f}")
        print(f"  min: {min_ep_len}")
        print(f"  max: {max_ep_len}")

    print("\nControlled player distribution:")
    for k, v in controlled_player_counter.most_common():
        print(f"  {k}: {v}")

    print("\nSelf character distribution:")
    for cid, v in self_char_counter.most_common():
        print(f"  {get_char_name(cid)}: {v}")

    print("\nOpponent character distribution:")
    for cid, v in opp_char_counter.most_common():
        print(f"  {get_char_name(cid)}: {v}")

    print("\nTop 20 matchups:")
    for (self_cid, opp_cid), v in matchup_counter.most_common(20):
        print(f"  {get_char_name(self_cid)} vs {get_char_name(opp_cid)}: {v}")

    print("\nOutcome distribution:")
    for k, v in outcome_counter.most_common():
        print(f"  {k}: {v}")

    print("\nTop 20 exact actions:")
    for act, v in action_counter.most_common(20):
        print(f"  {act}: {v}")

    print("\nPer-branch value distribution:")
    for i, name in enumerate(ACTION_NAMES):
        print(f"  {i} - {name}: {dict(branch_value_counters[i])}")

        print("\n==============================")
        
    print("ACTION BALANCING CHECK")
    print("==============================")

    if total_frames > 0:
        # exact action concentration
        top_actions = action_counter.most_common(10)
        top10_count = sum(count for _, count in top_actions)
        top10_ratio = top10_count / total_frames

        print(f"Top-10 exact actions cover: {top10_ratio:.2%} of all frames")

        if top10_ratio > 0.90:
            print("[WARN] Πολύ μεγάλη συγκέντρωση σε λίγα exact actions.")
            print("       Το dataset ίσως είναι υπερβολικά repetitive / movement-heavy.")

        # helper to get positive rate for binary-ish branches
        def positive_rate(counter, positive_values):
            total = sum(counter.values())
            if total == 0:
                return 0.0
            pos = sum(counter.get(v, 0) for v in positive_values)
            return pos / total

        jump_rate = positive_rate(branch_value_counters[1], {1})
        drop_rate = positive_rate(branch_value_counters[2], {1})
        light_rate = positive_rate(branch_value_counters[3], {1})
        heavy_rate = positive_rate(branch_value_counters[4], {1})
        block_rate = positive_rate(branch_value_counters[5], {1})
        special_rate = positive_rate(branch_value_counters[6], {1})
        charge_hold_rate = positive_rate(branch_value_counters[7], {1})
        charge_release_rate = positive_rate(branch_value_counters[7], {2})
        parry_rate = positive_rate(branch_value_counters[8], {1})

        print(f"Jump rate          : {jump_rate:.4%}")
        print(f"Drop rate          : {drop_rate:.4%}")
        print(f"Light rate         : {light_rate:.4%}")
        print(f"Heavy rate         : {heavy_rate:.4%}")
        print(f"Block rate         : {block_rate:.4%}")
        print(f"Special rate       : {special_rate:.4%}")
        print(f"Charge hold rate   : {charge_hold_rate:.4%}")
        print(f"Charge release rate: {charge_release_rate:.4%}")
        print(f"Parry rate         : {parry_rate:.4%}")

        rare_actions = []

        # αυτά είναι heuristics, όχι απόλυτοι κανόνες
        if light_rate < 0.002:
            rare_actions.append("light")
        if heavy_rate < 0.002:
            rare_actions.append("heavy")
        if block_rate < 0.002:
            rare_actions.append("block")
        if special_rate < 0.001:
            rare_actions.append("special")
        if charge_hold_rate < 0.001:
            rare_actions.append("charge_hold")
        if charge_release_rate < 0.0005:
            rare_actions.append("charge_release")
        if parry_rate < 0.0005:
            rare_actions.append("parry")

        if rare_actions:
            print(f"[WARN] Πολύ σπάνιες actions: {', '.join(rare_actions)}")
            print("       Το BC model ίσως να μην τις μάθει καλά χωρίς περισσότερα demos.")
        else:
            print("[OK] Όλες οι βασικές combat actions εμφανίζονται σε λογικό βαθμό.")

        locomotion_frames = (
            action_counter.get((0,0,0,0,0,0,0,0,0), 0) +
            action_counter.get((1,0,0,0,0,0,0,0,0), 0) +
            action_counter.get((2,0,0,0,0,0,0,0,0), 0)
        )
        locomotion_ratio = locomotion_frames / total_frames

        print(f"Pure locomotion/idle frames: {locomotion_ratio:.2%}")

        if locomotion_ratio > 0.80:
            print("[WARN] Το dataset είναι πολύ movement-dominated.")
            print("       Θέλεις περισσότερα combat-heavy / defensive interactions.")
        elif locomotion_ratio > 0.65:
            print("[INFO] Το dataset είναι αρκετά movement-heavy, αλλά όχι απαραίτητα κακό.")
        else:
            print("[OK] Το dataset έχει αρκετό combat variety.")
    # =========================
    # Build clean arrays
    # =========================
    valid_obs_len = obs_lengths.most_common(1)[0][0]
    valid_act_len = act_lengths.most_common(1)[0][0]

    clean_rows = [
        r for r in all_rows
        if isinstance(r.get("obs", None), list)
        and isinstance(r.get("act", None), list)
        and len(r["obs"]) == valid_obs_len
        and len(r["act"]) == valid_act_len
    ]

    print("\n==============================")
    print("CLEAN DATASET")
    print("==============================")
    print(f"Using obs length: {valid_obs_len}")
    print(f"Using act length: {valid_act_len}")
    print(f"Clean rows      : {len(clean_rows)}")

    X_obs = np.array([r["obs"] for r in clean_rows], dtype=np.float32)
    Y_act = np.array([r["act"] for r in clean_rows], dtype=np.int64)

    self_ids = np.array([r.get("self_character_id", -1) for r in clean_rows], dtype=np.int64)
    opp_ids = np.array([r.get("opp_character_id", -1) for r in clean_rows], dtype=np.int64)
    done_flags = np.array([1 if r.get("done", False) else 0 for r in clean_rows], dtype=np.int64)

    print(f"X_obs shape: {X_obs.shape}")
    print(f"Y_act shape: {Y_act.shape}")

    if save_dataset:
        out_path = os.path.join(output_dir, "imitation_dataset.npz")
        np.savez_compressed(
            out_path,
            X_obs=X_obs,
            Y_act=Y_act,
            self_ids=self_ids,
            opp_ids=opp_ids,
            done_flags=done_flags,
        )
        print(f"\n[INFO] Saved dataset to: {out_path}")

    # Save a small JSON summary too
    summary = {
        "num_files": len(files),
        "num_frames": int(total_frames),
        "num_episodes": int(total_episodes),
        "done_count": int(done_count),
        "obs_lengths": dict(obs_lengths),
        "act_lengths": dict(act_lengths),
        "clean_rows": int(len(clean_rows)),
        "x_shape": list(X_obs.shape),
        "y_shape": list(Y_act.shape),
        "controlled_player_distribution": dict(controlled_player_counter),
        "self_character_distribution": {get_char_name(k): int(v) for k, v in self_char_counter.items()},
        "opp_character_distribution": {get_char_name(k): int(v) for k, v in opp_char_counter.items()},
        "outcome_distribution": dict(outcome_counter),
        "top_actions": [
            {"action": list(act), "count": int(count)}
            for act, count in action_counter.most_common(20)
        ],
    }

    summary_path = os.path.join(output_dir, "dataset_summary.json")
    with open(summary_path, "w", encoding="utf-8-sig") as f:
        json.dump(summary, f, indent=2)

    print(f"[INFO] Saved summary to: {summary_path}")


if __name__ == "__main__":
    analyze_logs(LOG_DIR, OUTPUT_DIR, SAVE_DATASET)