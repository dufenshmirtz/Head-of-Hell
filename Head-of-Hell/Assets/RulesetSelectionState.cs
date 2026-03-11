using UnityEngine;

public static class RulesetSelectionState
{
    // -1 = Default, 1..5 = Custom slots
    public static int SelectedSlot = -1;

    public static void SelectDefault()
    {
        SelectedSlot = -1;
    }

    public static void SelectSlot(int slotNumber)
    {
        SelectedSlot = slotNumber; // 1..5
    }
}