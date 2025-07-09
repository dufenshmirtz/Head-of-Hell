using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageChoiceManager : MonoBehaviour
{
    public Button defaultButton;
    public Button customButton;
    public Button[] stageButtons; // 2x2 grid: 0 1
                                  //            2 3
    public Button proceedButton;

    public GameObject gameSetupMenu;           // Parent GameObject to disable (GameSetupMenu)
    public GameObject characterSelectionMenu;  // Character Selection UI to enable

    public bool stagePicked = false;
    public bool proceedActive = false;

    public TextMeshProUGUI selectionText;

    private int selectedModeIndex = 0; // 0 = default, 1 = custom
    private int selectedStageIndex = 0;
    private bool gameModePicked = false;

    void Start()
    {
        HighlightCurrentSelection();
    }

    void Update()
    {
        if (!gameModePicked)
        {
            HandleModeSelection();
        }
        else
        {
            HandleStageSelection();
        }

        if (Input.GetMouseButtonDown(1)) // Right click to reset
        {
            ResetAll();
        }

        // When Return is pressed while the proceedButton is selected, invoke its click event
        if (Input.GetKeyDown(KeyCode.Return) && proceedButton != null &&
            proceedButton.gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
        {
            proceedButton.onClick.Invoke();
        }
    }

    void HandleModeSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedModeIndex = Mathf.Max(0, selectedModeIndex - 1);
            HighlightCurrentSelection();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedModeIndex = Mathf.Min(1, selectedModeIndex + 1);
            HighlightCurrentSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            gameModePicked = true;
            selectedStageIndex = 0;
            HighlightStageSelection();
        }
    }

    void HandleStageSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (selectedStageIndex > 0)
                selectedStageIndex--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (selectedStageIndex < stageButtons.Length - 1)
                selectedStageIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            stageButtons[selectedStageIndex].onClick.Invoke();
            stagePicked = true;
            proceedActive = true;

            // Select the Proceed button so Enter will work on it
            if (proceedButton != null)
                proceedButton.Select();
        }


        HighlightStageSelection();
    }

    void HighlightCurrentSelection()
    {
        if (selectedModeIndex == 0)
            defaultButton.Select();
        else
            customButton.Select();
    }

    void HighlightStageSelection()
    {
        if (stageButtons != null && stageButtons.Length > selectedStageIndex)
        {
            stageButtons[selectedStageIndex].Select();
        }
    }

    void ResetAll()
    {
        gameModePicked = false;
        selectedModeIndex = 0;
        selectedStageIndex = 0;
        HighlightCurrentSelection();
    }

    // This method should be linked to proceedButton OnClick in the inspector
    public void OnProceed()
    {
        // Disable the GameObject this script is attached to
        gameObject.SetActive(false);

        // Enable the character selection UI
        if (characterSelectionMenu != null)
            characterSelectionMenu.SetActive(true);
    }

}
