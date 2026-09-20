using UnityEngine;

public abstract class WorldEntity : MonoBehaviour, ISelectable
{
    [Header("Visuals")]
    public GameObject selectionRing;
    
    
    [Header("Identity")]
    public int OwnerPlayerID;


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