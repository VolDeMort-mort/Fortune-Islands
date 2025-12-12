using UnityEngine;
using UnityEngine.EventSystems; 
public class SelectionManager : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask selectableLayer;

    private ISelectable _currentSelection;
    bool isSelected = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Optional: Stop if clicking on UI
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

        if (Physics.Raycast(ray, out hit, 1000f, selectableLayer))
        {
            ISelectable clickedObject = hit.collider.GetComponent<ISelectable>();

            // Check hit
            if (clickedObject != null)
            {
                if (_currentSelection != clickedObject)
                {
                    DeselectCurrent();
                    
                    _currentSelection = clickedObject;
                    _currentSelection.OnSelect();
                }
            }
            else 
            {
                DeselectCurrent();
            }
        }
        else
        {
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