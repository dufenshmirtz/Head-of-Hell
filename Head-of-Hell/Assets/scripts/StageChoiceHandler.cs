using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageChoiceHandler : MonoBehaviour
{
    public GameObject[] stages;
    string stageName;
    // Start is called before the first frame update
    void Start()
    {
        stageName = PlayerPrefs.GetString("SelectedStage");
        if (stageName == "Stage 1")
        {
            stages[0].SetActive(true);
        }
        else if (stageName == "Stage 2")
        {
            stages[1].SetActive(true);
        }
        else if (stageName == "Stage 3")
        {
            stages[2].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
