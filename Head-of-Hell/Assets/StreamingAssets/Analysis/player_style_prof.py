import pandas as pd

ROUNDS_FILE = "style_out/rounds_with_clusters.csv"
ELO_FILE = "out_elo/elo_overall.csv"
OUT_FILE = "style_out/player_style_profiles.csv"


df = pd.read_csv(ROUNDS_FILE)

# -----------------------
# STYLE DISTRIBUTION
# -----------------------

style_counts = (
    df.groupby(["player_id", "playstyle"])
      .size()
      .unstack(fill_value=0)
)

style_pct = style_counts.div(style_counts.sum(axis=1), axis=0)

style_pct.columns = [c + "_pct" for c in style_pct.columns]


# -----------------------
# BEHAVIOUR MEANS
# -----------------------

features = [
    "attack_rate",
    "mobility_rate",
    "defense_rate",
    "quick_rate",
    "heavy_rate",
    "special_rate",
    "charge_rate"
]

feature_means = df.groupby("player_id")[features].mean()


# -----------------------
# DOMINANT STYLE
# -----------------------

dominant_style = style_counts.idxmax(axis=1)
dominant_style = dominant_style.rename("dominant_style")


# -----------------------
# MERGE ALL
# -----------------------

profiles = pd.concat(
    [style_pct, feature_means, dominant_style],
    axis=1
).reset_index()


# -----------------------
# MERGE ELO
# -----------------------

elo = pd.read_csv(ELO_FILE)

profiles = profiles.merge(
    elo[["player_id", "overall_elo"]],
    on="player_id",
    how="left"
)


# -----------------------
# SAVE
# -----------------------

profiles.to_csv(OUT_FILE, index=False)

print("Saved:", OUT_FILE)
print(profiles)
