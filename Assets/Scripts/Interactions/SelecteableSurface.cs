using UnityEngine;

public class SelectableSurface : MonoBehaviour, ISelectable
{
    [Header("Visuals")]
    public GameObject selectionRing; // Drag a "Green Ring" sprite here in Inspector

    private void Start()
    {
        // Ensure selection graphic is hidden at start
        if (selectionRing != null) selectionRing.SetActive(false);
    }

    public void OnSelect()
    {
        Debug.Log($"Selected: {gameObject.name}");
        // Show the ring, change color, or play sound
        if (selectionRing != null) selectionRing.SetActive(true);
    }

    public void OnDeselect()
    {
        Debug.Log($"Deselected: {gameObject.name}");
        // Hide the ring
        if (selectionRing != null) selectionRing.SetActive(false);
    }
}