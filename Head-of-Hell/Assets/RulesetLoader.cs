using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesetLoader : MonoBehaviour
{
    public TMPro.TMP_Text choice;
    int rulesetNum=-1;

    void Start()
    {
        rulesetNum = RulesetManager.Instance.GetRulesetNum();

        if(rulesetNum!=-1){
            CustomRuleset ruleset = RulesetManager.Instance.LoadCustomRuleset(rulesetNum);
            choice.text = ruleset.slotName; 
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
