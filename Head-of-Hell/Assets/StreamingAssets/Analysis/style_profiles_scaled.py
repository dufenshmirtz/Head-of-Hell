import pandas as pd
import matplotlib.pyplot as plt
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans

# Load profile-level dataset
df = pd.read_csv("out/dataset_profile_level.csv")

# Use EMA features (stable player identity)
cols = [
    "ema_attack_rate",
    "ema_mobility_rate",
    "ema_defense_rate",
    "ema_hit_rate",
    "ema_miss_rate",
    "ema_dps_dealt",
    "ema_dps_taken",
    "ema_block_rate",
    "ema_dodge_rate",
]

X = df[cols].astype(float)

# Scaling (z-score)
scaler = StandardScaler()
Xz = scaler.fit_transform(X)

# Helper to index columns
idx = {c: i for i, c in enumerate(cols)}

# -------------------------
# Style scoring (Scaled)
# -------------------------

# INTENT (what the player tries to do)
df["aggr_intent"] = (
    0.7 * Xz[:, idx["ema_attack_rate"]] +
    0.3 * Xz[:, idx["ema_dps_dealt"]]
)

df["def_intent"] = (
    0.8 * Xz[:, idx["ema_defense_rate"]] +
    0.2 * Xz[:, idx["ema_mobility_rate"]]
)

# EFFECTIVENESS (how well it works)
df["off_eff"] = (
    0.5 * Xz[:, idx["ema_hit_rate"]] +
    0.5 * Xz[:, idx["ema_dps_dealt"]]
)

df["def_eff"] = (
    0.4 * Xz[:, idx["ema_block_rate"]] +
    0.4 * Xz[:, idx["ema_dodge_rate"]] +
    0.2 * (-Xz[:, idx["ema_dps_taken"]])  # less dmg taken => better defense
)

# RISK (recklessness / punishment)
df["risk"] = (
    0.6 * Xz[:, idx["ema_miss_rate"]] +
    0.4 * Xz[:, idx["ema_dps_taken"]]
)

# -------------------------
# Clustering (optional now)
# -------------------------
# With very few rows keep clusters small
n = len(df)
k = 2 if n < 8 else 3

features_cluster = df[["aggr_intent", "def_intent", "off_eff", "def_eff", "risk"]].astype(float)
kmeans = KMeans(n_clusters=k, random_state=42, n_init="auto")
df["cluster"] = kmeans.fit_predict(features_cluster)

# -------------------------
# Debug print
# -------------------------
print("\n=== Profile-level scaled style scores ===")
print(df[[
    "match_id", "round_number", "player_id", "won_round",
    "aggr_intent", "def_intent", "off_eff", "def_eff", "risk",
    "cluster"
]])

# -------------------------
# Plot 1: INTENT MAP
# -------------------------
plt.figure(figsize=(7, 7))
for c in sorted(df["cluster"].unique()):
    sub = df[df["cluster"] == c]
    plt.scatter(sub["aggr_intent"], sub["def_intent"], label=f"Cluster {c}", alpha=0.8)

for _, r in df.iterrows():
    label = f'{r["player_id"]}-R{r["round_number"]}'
    plt.text(r["aggr_intent"] + 0.02, r["def_intent"] + 0.02, label, fontsize=9)

plt.xlabel("Aggression (intent, scaled)")
plt.ylabel("Defense (intent, scaled)")
plt.title("Profile Style Map — Intent (EMA + Scaled)")
plt.legend()
plt.show()

# -------------------------
# Plot 2: EFFECTIVENESS MAP
# -------------------------
plt.figure(figsize=(7, 7))
for c in sorted(df["cluster"].unique()):
    sub = df[df["cluster"] == c]
    plt.scatter(sub["off_eff"], sub["def_eff"], label=f"Cluster {c}", alpha=0.8)

for _, r in df.iterrows():
    label = f'{r["player_id"]}-R{r["round_number"]}'
    plt.text(r["off_eff"] + 0.02, r["def_eff"] + 0.02, label, fontsize=9)

plt.xlabel("Offense effectiveness (scaled)")
plt.ylabel("Defense effectiveness (scaled)")
plt.title("Profile Style Map — Effectiveness (EMA + Scaled)")
plt.legend()
plt.show()
