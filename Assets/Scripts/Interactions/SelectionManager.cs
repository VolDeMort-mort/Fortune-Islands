using UnityEngine;
using UnityEngine.EventSystems; // Required to stop clicking through UI buttons

public class SelectionManager : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask selectableLayer; // Set this to "Default" or "Units"
    
    // We remember what is currently selected so we can deselect it later
    private ISelectable _currentSelection;
    bool isSelected = false;

    void Update()
    {
        // 1. Check if we clicked the Left Mouse Button (0)
        if (Input.GetMouseButtonDown(0))
        {
            // Optional: Stop if clicking on UI (Buttons, Menus)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
                return;
            
            if (isSelected) DeselectCurrent();

            HandleSelection();
        }
    }

    void HandleSelection()
    {
        // Create a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Shoot the ray!
        if (Physics.Raycast(ray, out hit, 1000f, selectableLayer))
        {
            // Did we hit something with the ISelectable interface?
            ISelectable clickedObject = hit.collider.GetComponent<ISelectable>();

            if (clickedObject != null)
            {
                // If we hit a DIFFERENT object, deselect the old one
                if (_currentSelection != clickedObject)
                {
                    DeselectCurrent();
                    
                    // Select the new one
                    _currentSelection = clickedObject;
                    _currentSelection.OnSelect();
                }
            }
            else 
            {
                // We hit ground/wall that isn't selectable -> Deselect current
                DeselectCurrent();
            }
        }
        else
        {
            // We clicked empty sky -> Deselect current
            DeselectCurrent();
        }
    }

    void DeselectCurrent()
    {
        if (_currentSelection != null)
        {
            _currentSelection.OnDeselect();
            _currentSelection = null;
        }
    }
}