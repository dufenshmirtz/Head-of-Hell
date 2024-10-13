using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditButtonManager : MonoBehaviour
{
    public int editNumber;
    public CustomRulesetUI customRulesetUI;

    public void SlotInit()
    {
        print(editNumber+"___editb");
        customRulesetUI.Initialize(editNumber);
    }
}
