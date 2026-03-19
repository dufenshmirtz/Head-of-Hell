import pandas as pd
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans

df = pd.read_csv("out/dataset_profile_level.csv")

# --- EMA feature set (stable) ---
ema_cols = [
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

X = df[ema_cols].astype(float)
scaler = StandardScaler()
Xz = scaler.fit_transform(X)
idx = {c:i for i,c in enumerate(ema_cols)}

# --- Style scores (scaled) ---
df["aggr_intent"] = 0.7*Xz[:, idx["ema_attack_rate"]] + 0.3*Xz[:, idx["ema_dps_dealt"]]
df["def_intent"]  = 0.8*Xz[:, idx["ema_defense_rate"]] + 0.2*Xz[:, idx["ema_mobility_rate"]]

df["off_eff"] = 0.5*Xz[:, idx["ema_hit_rate"]] + 0.5*Xz[:, idx["ema_dps_dealt"]]
df["def_eff"] = (
    0.4*Xz[:, idx["ema_block_rate"]] +
    0.4*Xz[:, idx["ema_dodge_rate"]] +
    0.2*(-Xz[:, idx["ema_dps_taken"]])   # less taken -> better defense
)

df["risk"] = 0.6*Xz[:, idx["ema_miss_rate"]] + 0.4*Xz[:, idx["ema_dps_taken"]]
df["mobility"] = Xz[:, idx["ema_mobility_rate"]]

# --- Clustering space ---
style_cols = ["aggr_intent", "def_intent", "off_eff", "def_eff", "risk", "mobility"]
S = df[style_cols].astype(float)

# Choose k sensibly (don’t overcluster tiny data)
n = len(df)
k = 2 if n < 8 else 3 if n < 25 else 4
kmeans = KMeans(n_clusters=k, random_state=42, n_init="auto")
df["cluster"] = kmeans.fit_predict(S)

# --- Cluster means for interpretation ---
means = df.groupby("cluster")[style_cols].mean()

def label_cluster(row):
    """
    Rule-based archetype labeling from cluster mean style scores.
    Works best when k>=3 and you have enough data; still ok for k=2 as coarse labels.
    """
    aggr = row["aggr_intent"]
    defend = row["def_intent"]
    mob = row["mobility"]
    risk = row["risk"]
    off = row["off_eff"]
    deff = row["def_eff"]

    # 1) Mobile/Evasive: mobility dominates
    if mob >= 0.6 and defend >= -0.2:
        return "Mobile / Evasive"

    # 2) Defensive/Turtle: high defense intent + low risk (safe play)
    if defend >= 0.5 and risk <= 0.2:
        return "Defensive / Turtle"

    # 3) Aggressive/Rushdown: high aggression intent + good offense effectiveness
    if aggr >= 0.5 and off >= 0.0:
        return "Aggressive / Rushdown"

    # 4) Risky: high risk regardless of other things
    if risk >= 0.7:
        return "Risky"

    # 5) Balanced/Neutral: near the center or mixed signals
    return "Balanced / Neutral"

# Apply labels
cluster_label_map = {c: label_cluster(means.loc[c]) for c in means.index}
df["playstyle_label"] = df["cluster"].map(cluster_label_map)

# --- Print: cluster summary + assigned labels ---
print("\n=== Cluster means (scaled style scores) ===")
print(means.round(3))

print("\n=== Auto labels per cluster ===")
for c in sorted(cluster_label_map.keys()):
    print(f"Cluster {c} -> {cluster_label_map[c]}")

# --- Print: each row assignment (who got what) ---
print("\n=== Assignments ===")
print(df[["match_id","round_number","player_id","cluster","playstyle_label"] + style_cols].round(3))
