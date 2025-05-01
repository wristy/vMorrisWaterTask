using UnityEngine;

public class DistalCueManager : MonoBehaviour
{
    public GameObject distalCueGroup; // OR assign the group if not using tags

    void Start()
    {
        SetDistalCuesActive(GameSettings.enableDistalCues);
    }

    public void SetDistalCuesActive(bool enabled)
    {
        if (distalCueGroup != null)
        {
            distalCueGroup.SetActive(enabled);
        }
        else
        {
            Debug.LogWarning("No distal cues found to toggle.");
        }
    }
}
