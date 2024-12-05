using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartButtonScript : MonoBehaviour
{
    public TMP_Text setting;
    
    public void CheckForDefault()
    {
        if(setting.text == "Default")
        {
            CustomRuleset ruleset= new CustomRuleset();

            ruleset.health = 100;
            ruleset.powerupsEnabled = true;
            ruleset.rounds = 1;
            ruleset.playerSpeed = 4;
            ruleset.quickDisabled = false;
            ruleset.heavyDisabled = false;
            ruleset.blockDisabled = false;
            ruleset.chargeDisabled = false;
            ruleset.specialDisabled = false;
            ruleset.hideHealth = false;
            ruleset.devTools = false;

            // Save the ruleset data to PlayerPrefs as JSON
            string json = JsonUtility.ToJson(ruleset);
            PlayerPrefs.SetString("SelectedRuleset", json);
            PlayerPrefs.Save(); // Make sure data is saved
        }
    }
}
