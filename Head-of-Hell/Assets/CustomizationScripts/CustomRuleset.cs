[System.Serializable]
public class CustomRuleset
{
    public string slotName;         // Name of the custom slot
    public int rounds;              // Number of rounds (1, 2, or 3)
    public bool powerupsEnabled;    // Whether powerups are Disabled
    public int health;              // Player's health
    public bool hideHealth;
    public int playerSpeed;         //Player'speed
    public bool devTools;
    public int portals;

    //Disable abilitiess
    public bool quickDisabled;       
    public bool heavyDisabled;
    public bool blockDisabled;
    public bool specialDisabled;
    public bool chargeDisabled;
}
