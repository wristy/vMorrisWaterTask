using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TrialEditorController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text trialLabel; // e.g. “Trial 1 of 5”
    public TMP_InputField radiusInput;
    public TMP_InputField cuesInput;
    public TMP_InputField timeLimitInput;
    public TMP_Dropdown trialTypeDropdown;

    // One dropdown for how we pick the starting location
    public TMP_Dropdown startingLocationDropdown;

    // One dropdown for how we place the proximal cues
    public TMP_Dropdown cuePlacementDropdown;

    // One dropdown for how we place the treasure chest
    public TMP_Dropdown chestPlacementDropdown;
    public Toggle distalCueToggle;
    public List<Vector3> customCuePositions = new List<Vector3>();
    public RectTransform cueMarkerContainer;


    public RectTransform startingLocationCircle;
    // Optional: A marker to display the selected point in the UI.
    public RectTransform selectedMarker; // green dot for start
    public RectTransform cueMarkerPrefab; // blue dot for cues
    public RectTransform chestMarkerPrefab; // e.g. a gold dot for chest
    public RectTransform markerContainer; // parent container for all dots

    public Button prevButton;
    public Button nextButton;
    public Button doneButton;
    public Button copyToAllButton;
    private TrialDefinition currentTrialDefinition;


    public string nextSceneName = "TrialScene"; // Or wherever you go after editing

    private int currentTrialIndex = 0;

    void Start()
    {
        // Display the first trial
        RefreshUIFromData();
        UpdateButtonInteractables();

        // Hook up button actions
        prevButton.onClick.AddListener(OnClickPrev);
        nextButton.onClick.AddListener(OnClickNext);
        doneButton.onClick.AddListener(OnClickDone);
        copyToAllButton.onClick.AddListener(OnCopyToAll);

        // Also wire up the toggles or dropdowns if needed
        distalCueToggle.isOn = GameSettings.enableDistalCues;

        // If you want an “onValueChanged” callback for each dropdown
        startingLocationDropdown.onValueChanged.AddListener((_) => OnStartingLocationOptionChanged());
        cuePlacementDropdown.onValueChanged.AddListener((_) => OnCuePlacementOptionChanged());
        chestPlacementDropdown.onValueChanged.AddListener((_) => OnChestPlacementOptionChanged());
    }

    public void OnClickPrev()
    {
        // Save current UI into currentTrialIndex, then go back
        SaveUIIntoData();
        currentTrialIndex--;
        RefreshUIFromData();
        UpdateButtonInteractables();
    }

    public void OnClickNext()
    {
        // Save UI into data, then go forward
        SaveUIIntoData();
        currentTrialIndex++;
        RefreshUIFromData();
        UpdateButtonInteractables();
    }


    public void OnClickDone()
    {
        // Save UI into data one last time
        SaveUIIntoData();


        currentTrialDefinition = GameSettings.allTrials[0];

        // Copy the current trial’s data into the GameSettings
        GameSettings.circleRadius = currentTrialDefinition.circleRadius;
        GameSettings.numberOfProximalCues = currentTrialDefinition.numberOfProximalCues;
        GameSettings.trialType = currentTrialDefinition.trialType;
        GameSettings.timeLimit = currentTrialDefinition.timeLimit;
        GameSettings.enableDistalCues = distalCueToggle.isOn;


        // Move on to the final or next scene
        SceneManager.LoadScene(nextSceneName);
    }

    void OnCopyToAll()
    {
        // Save current UI settings into data.
        SaveUIIntoData();

        // Get the current trial settings.
        TrialDefinition currentSettings = GameSettings.allTrials[currentTrialIndex];

        // Iterate through all trials and copy the current settings.
        for (int i = 0; i < GameSettings.allTrials.Length; i++)
        {
            if (i == currentTrialIndex)
                continue;

            GameSettings.allTrials[i].circleRadius = currentSettings.circleRadius;
            GameSettings.allTrials[i].numberOfProximalCues = currentSettings.numberOfProximalCues;
            GameSettings.allTrials[i].trialType = currentSettings.trialType;
            GameSettings.allTrials[i].timeLimit = currentSettings.timeLimit;
            GameSettings.allTrials[i].startingLocationOption = currentSettings.startingLocationOption;
            GameSettings.allTrials[i].customStartingPosition = currentSettings.customStartingPosition;
            GameSettings.allTrials[i].customCuePositions = new List<Vector3>(currentSettings.customCuePositions);
        }

        // Optionally, refresh the UI so all trials show the updated settings.
        RefreshUIFromData();
        Debug.Log("Copied current trial settings to all trials.");
    }



    void RefreshUIFromData()
    {
        if (GameSettings.allTrials == null || GameSettings.allTrials.Length == 0) return;

        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];
        trialLabel.text = $"Trial {currentTrialIndex + 1} of {GameSettings.allTrials.Length}";

        radiusInput.text = td.circleRadius.ToString();
        cuesInput.text = td.numberOfProximalCues.ToString();
        timeLimitInput.text = td.timeLimit.ToString();

        trialTypeDropdown.value = (int)td.trialType;
        startingLocationDropdown.value = (int)td.startingLocationOption;

        // Update circle UI: if Circle option is selected, show the circle and reset the pointer;
        // otherwise, hide them.
        if (td.startingLocationOption == StartingLocationOption.Circle)
        {
            startingLocationCircle.gameObject.SetActive(true);
            if (selectedMarker != null)
            {
                selectedMarker.anchoredPosition = Vector2.zero; // Center the pointer
                selectedMarker.gameObject.SetActive(false);
            }
            // td.customStartingPosition = Vector3.zero;
        }
        else
        {
            startingLocationCircle.gameObject.SetActive(false);
            if (selectedMarker != null)
            {
                selectedMarker.gameObject.SetActive(false);
            }
        }
        RegenerateCueMarkers();
    }

    void SaveUIIntoData()
    {
        if (GameSettings.allTrials == null || GameSettings.allTrials.Length == 0) return;

        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];

        if (radiusInput == null)
            Debug.LogError("radiusInput is null!");
        if (cuesInput == null)
            Debug.LogError("cuesInput is null!");
        if (timeLimitInput == null)
            Debug.LogError("timeLimitInput is null!");
        if (trialTypeDropdown == null)
            Debug.LogError("trialTypeDropdown is null!");
        if (startingLocationDropdown == null)
            Debug.LogError("startingLocationDropdown is null!");

        // Parse user input safely
        if (float.TryParse(radiusInput.text, out float rad))
            td.circleRadius = rad;

        if (int.TryParse(cuesInput.text, out int cues))
            td.numberOfProximalCues = cues;

        if (int.TryParse(timeLimitInput.text, out int time))
            td.timeLimit = time;

        td.trialType = (GameSettings.TrialType)trialTypeDropdown.value;

        GameSettings.allTrials[currentTrialIndex].startingLocationOption = (StartingLocationOption)startingLocationDropdown.value;
    }

    // ------------------------------------------------------------------
    // Dropdown callbacks
    // ------------------------------------------------------------------
    // void OnStartingLocationOptionChanged()
    // {
    //     currentTrialDefinition.startingLocationOption = (StartingLocationOption)startingLocationDropdown.value;
    //     RefreshUIFromData();
    // }

    void OnCuePlacementOptionChanged()
    {
        currentTrialDefinition.cuePlacementOption = (CuePlacementOption)cuePlacementDropdown.value;
        RefreshUIFromData();
    }

    void OnChestPlacementOptionChanged()
    {
        currentTrialDefinition.chestPlacementOption = (ChestPlacementOption)chestPlacementDropdown.value;
        RefreshUIFromData();
    }

    // ------------------------------------------------------------------
    // Handling clicks on the circle
    // ------------------------------------------------------------------


    void UpdateButtonInteractables()
    {
        // If we’re on the first trial, “Prev” is disabled
        prevButton.interactable = currentTrialIndex > 0;
        // If we’re on the last trial, “Next” is disabled
        nextButton.interactable = currentTrialIndex < (GameSettings.allTrials.Length - 1);
    }

    // --- Handling the circle UI for selecting starting location ---

    // This method is called when the user clicks on the circle UI element.
    // (Make sure the startingLocationCircle UI element has an EventTrigger or
    // is set to use IPointerClickHandler.)
    public void OnClickCircle(PointerEventData eventData)
    {
        if (GameSettings.allTrials == null || GameSettings.allTrials.Length == 0)
            return;

        // Only process if the current option is Circle.
        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];
        if (td.startingLocationOption != StartingLocationOption.Circle)
            return;

        // Convert the screen point to a local point within the circle's RectTransform.
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(startingLocationCircle, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Assume the circle's pivot is in the center.
            float circleUISize = startingLocationCircle.rect.width * 0.5f; // radius in UI units
            // Clamp to the UI circle boundary.
            if (localPoint.magnitude > circleUISize)
                localPoint = localPoint.normalized * circleUISize;

            // Map UI local coordinates to experimental coordinates.
            // The experimental circle has a radius defined by td.circleRadius.
            float scaleFactor = td.circleRadius / circleUISize;
            Vector3 customPos = new Vector3(localPoint.x * scaleFactor, 0, localPoint.y * scaleFactor);

            // Save this custom starting position.
            td.customStartingPosition = customPos;

            // Update a marker in the UI to reflect the chosen point.
            UpdateMarkerPosition(customPos);
        }
    }

    // Updates the marker position on the circle UI to show the selected point.
    void UpdateMarkerPosition(Vector3 customPos)
    {
        if (selectedMarker == null) return;

        // Reverse the mapping: experimental coordinates to UI coordinates.
        float circleUISize = startingLocationCircle.rect.width * 0.5f;
        float scaleFactor = circleUISize / GameSettings.allTrials[currentTrialIndex].circleRadius;
        Vector2 uiPos = new Vector2(customPos.x, customPos.z) * scaleFactor;

        selectedMarker.anchoredPosition = uiPos;
        selectedMarker.gameObject.SetActive(true);
    }

    void OnStartingLocationOptionChanged()
    {
        // Update the current trial definition from the dropdown.
        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];
        td.startingLocationOption = (StartingLocationOption)startingLocationDropdown.value;

        // If the option is Circle, show the circle UI and reset the pointer to the center.
        if (td.startingLocationOption == StartingLocationOption.Circle)
        {
            startingLocationCircle.gameObject.SetActive(true);
            if (selectedMarker != null)
            {
                selectedMarker.anchoredPosition = Vector2.zero; // Reset marker to center
                selectedMarker.gameObject.SetActive(false);     // Hide until user clicks
            }
            // td.customStartingPosition = Vector3.zero;
        }
        else
        {
            // Hide the circle UI and marker if any other option is selected.
            startingLocationCircle.gameObject.SetActive(false);
            if (selectedMarker != null)
            {
                selectedMarker.gameObject.SetActive(false);
            }
        }
    }
    public void OnCueCircleClick(BaseEventData data)
    {
        PointerEventData ped = (PointerEventData)data;
        OnClickCuePlacement(ped);
    }
    public void OnClickCuePlacement(PointerEventData eventData)
    {
        Debug.Log("Clicked circle!");

        if (GameSettings.allTrials == null || GameSettings.allTrials.Length == 0)
            return;

        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];

        // Convert the screen point to a local point within the circle's RectTransform
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            startingLocationCircle, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float circleUISize = startingLocationCircle.rect.width * 0.25f;

            // Enforce placement *only* on the perimeter (normalize and scale to radius)
            Vector2 perimeterPoint = localPoint.normalized * circleUISize;

            float scaleFactor = td.circleRadius / circleUISize;
            Vector3 cueWorldPos = new Vector3(perimeterPoint.x * scaleFactor * 1.2f, 0, perimeterPoint.y * scaleFactor * 1.2f);

            td.customCuePositions.Add(cueWorldPos);

            // Instantiate a marker dot on the UI at that position
            RectTransform newMarker = Instantiate(cueMarkerPrefab, cueMarkerContainer);
            newMarker.anchoredPosition = perimeterPoint;
            newMarker.gameObject.SetActive(true);
        }
    }

    public void ClearCueMarkers()
    {
        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];
        td.customCuePositions.Clear();

        // Loop backwards to avoid issues with modifying collection while iterating
        for (int i = cueMarkerContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cueMarkerContainer.GetChild(i).gameObject);
        }

        Debug.Log("Cleared all cue positions.");
    }

    void RegenerateCueMarkers()
    {
        // Clear any existing markers
        for (int i = cueMarkerContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cueMarkerContainer.GetChild(i).gameObject);
        }

        // Repopulate based on customCuePositions for this trial
        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];
        float circleUISize = startingLocationCircle.rect.width * 0.209f;
        float scaleFactor = circleUISize / td.circleRadius;

        foreach (Vector3 cuePos in td.customCuePositions)
        {
            Vector2 uiPos = new Vector2(cuePos.x, cuePos.z) * scaleFactor;

            RectTransform newMarker = Instantiate(cueMarkerPrefab, cueMarkerContainer);
            newMarker.anchoredPosition = uiPos;
            newMarker.gameObject.SetActive(true);
        }
    }


}
