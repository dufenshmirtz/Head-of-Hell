import pandas as pd
import matplotlib.pyplot as plt
from sklearn.cluster import KMeans

# load dataset
df = pd.read_csv("out/dataset_round_level.csv")

# --- create style axes ---

df["aggression"] = (
    0.4 * df["attack_rate"] +
    0.4 * df["dps_dealt"] +
    0.2 * df["hit_rate"]
)

df["defense_style"] = (
    0.4 * df["defense_rate"] +
    0.3 * df["block_rate"] +
    0.3 * df["dodge_rate"]
)

df["mobility"] = df["mobility_rate"]

df["risk"] = (
    0.5 * df["miss_rate"] +
    0.5 * df["dps_taken"]
)

# --- clustering ---

features = df[["aggression", "defense_style", "mobility", "risk"]]

kmeans = KMeans(n_clusters=2, random_state=42)
df["cluster"] = kmeans.fit_predict(features)

# --- DEBUG PRINT (τιμές ανά point) ---
print("\n=== Points (rows) with features + cluster ===")
print(df[[
    "match_id", "round_number", "player_id",
    "attack_rate", "mobility_rate", "defense_rate",
    "hit_rate", "miss_rate",
    "dps_dealt", "dps_taken",
    "block_rate", "dodge_rate",
    "aggression", "defense_style", "mobility", "risk",
    "cluster"
]])

# --- plot style map ---

plt.figure(figsize=(7,7))

for c in df["cluster"].unique():
    sub = df[df["cluster"] == c]
    plt.scatter(
        sub["aggression"],
        sub["defense_style"],
        label=f"Cluster {c}",
        alpha=0.7
    )

# --- LABELS πάνω στα points ---
for _, r in df.iterrows():
    # διάλεξε ένα label:
    # 1) μόνο player:
    # label = str(r["player_id"])

    # 2) player + round:
    label = f'{r["player_id"]}-R{r["round_number"]}'

    # 3) player + match + round (αν θες full):
    # label = f'{r["player_id"]}-M{r["match_id"]}-R{r["round_number"]}'

    # μικρό offset για να μη σκεπάζει την τελεία
    plt.text(r["aggression"] + 0.01, r["defense_style"] + 0.01, label, fontsize=9)

plt.xlabel("Aggression")
plt.ylabel("Defense (style)")
plt.title("Player Style Map")

plt.legend()
plt.show()
