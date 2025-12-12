using UnityEngine;

public class SelectableUnit : MonoBehaviour, ISelectable
{
    [Header("Visuals")]
    public GameObject selectionRing;

    private void Start()
    {
        if (selectionRing != null) selectionRing.SetActive(false);
    }

    public void OnSelect()
    {
        Debug.Log($"Selected: {gameObject.name}");
        if (selectionRing != null) selectionRing.SetActive(true);
    }

    public void OnDeselect()
    {
        Debug.Log($"Deselected: {gameObject.name}");
        if (selectionRing != null) selectionRing.SetActive(false);
    }
}