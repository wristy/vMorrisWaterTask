// Create this new script: MainCircleClickHandler.cs
using UnityEngine;
using UnityEngine.EventSystems; // Required for event system interfaces

public class MainCircleClickHandler : MonoBehaviour, IPointerClickHandler
{
    // Reference to your TrialEditorController.
    // You can assign this in the Inspector or find it dynamically.
    public TrialEditorController trialEditorController;

    void Start()
    {
        Debug.Log("MainCircleClickHandler started", this.gameObject);
        // If not assigned in Inspector, try to find it.
        // This assumes TrialEditorController is on a known GameObject or is unique.
        if (trialEditorController == null)
        {
            trialEditorController = FindObjectOfType<TrialEditorController>();
            if (trialEditorController == null)
            {
                Debug.LogError("MainCircleClickHandler could not find TrialEditorController!", this.gameObject);
            }

        }
        Debug.Log("MainCircleClickHandler found TrialEditorController: " + trialEditorController, this.gameObject);
    }

    // This method will be called when the UI element this script is attached to is clicked.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (trialEditorController != null)
        {
            // Call the method on your TrialEditorController, passing the eventData
            trialEditorController.OnMainCircleClicked(eventData);
        }
        else
        {
            Debug.LogError("TrialEditorController reference is not set in MainCircleClickHandler.", this.gameObject);
        }
    }
}