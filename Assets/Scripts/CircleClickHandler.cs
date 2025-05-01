using UnityEngine;
using UnityEngine.EventSystems;

public class CircleStartingLocationSelector : MonoBehaviour, IPointerClickHandler
{
    // Reference to the RectTransform of the circle UI element.
    public RectTransform startingLocationCircle;
    // Optional: Reference to a marker (like your green dot) to show the selected point.
    public RectTransform selectedMarker;

    // The current trial index and the trial definitions should be set up somewhere in your project.
    // For this example, we'll assume they're available in a static GameSettings or passed in some way.
    private int currentTrialIndex = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on the circle!");

        // Only process if the current trial's starting location option is Circle.
        TrialDefinition td = GameSettings.allTrials[currentTrialIndex];

        // Convert the screen point to a local point within the circle's RectTransform.
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            startingLocationCircle, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Assume the circle's pivot is in the center.
            float circleUISize = startingLocationCircle.rect.width * 0.5f; // radius in UI units

            // Clamp the local point so that it stays within the circle's boundary.
            if (localPoint.magnitude > circleUISize)
                localPoint = localPoint.normalized * circleUISize;

            // Map UI local coordinates to experimental coordinates.
            // The experimental circle has a radius defined by td.circleRadius.
            float scaleFactor = td.circleRadius / circleUISize;
            Vector3 customPos = new Vector3(localPoint.x * scaleFactor * 0.7f, 1, localPoint.y * scaleFactor * 0.7f);

            // Save this custom starting position.
            td.customStartingPosition = customPos;

            // Update a marker in the UI to reflect the chosen point.
            UpdateMarkerPosition(customPos);
        }
    }

    // This method updates the marker (the small green dot) position on the UI.
    void UpdateMarkerPosition(Vector3 customPos)
    {
        if (selectedMarker == null || startingLocationCircle == null)
            return;

        // Reverse mapping from experimental coordinates back to UI coordinates.
        float circleUISize = startingLocationCircle.rect.width * 0.5f;
        float scaleFactor = circleUISize / GameSettings.allTrials[currentTrialIndex].circleRadius;
        Vector2 uiPos = new Vector2(customPos.x, customPos.z) * scaleFactor;

        // Set the marker's anchored position relative to the circle.
        selectedMarker.anchoredPosition = uiPos;
        selectedMarker.gameObject.SetActive(true);
    }
}
