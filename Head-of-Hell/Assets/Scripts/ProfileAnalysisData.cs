using System;
using System.Collections.Generic;

[Serializable]
public class ProfileAnalysisCollection
{
    public string version;
    public string generated_at;
    public List<ProfileAnalysisEntry> profiles;
}

[Serializable]
public class ProfileAnalysisEntry
{
    public string profile_id;
    public string profile_name;

    public string style_label;

    public int matches_count;

    public float win_rate;
    public float hit_rate;
    public float miss_rate;

    public float avg_damage_dealt;
    public float avg_damage_taken;

    public float aggression_raw;
    public float defense_raw;
    public float mobility_raw;
    public float risk_raw;

    public float aggression;
    public float defense;
    public float mobility;
    public float risk;
}