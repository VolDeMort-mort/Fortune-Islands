using UnityEngine;
using System.Collections.Generic;

public class UnitManager : IManager
{
    [Header("Settings")]
    public GameObject villagerPrefab; // Fallback if stats are missing
    public LayerMask unitLayer;
    public LayerMask groundLayer;
    public float dragThreshold = 10f; 

    [Header("Unit Database")]
    public UnitStats villagerData; 
    public UnitStats archerData;
    public UnitStats warriorData;

    // State
    private List<UnitController> _allUnits = new List<UnitController>();
    private List<UnitController> _selectedUnits = new List<UnitController>();
    
    private bool _isWarPhase = false;
    private bool _isDragging = false;
    private Vector3 _dragStartPos;

    // --- GUI Drawing ---
    private void OnGUI()
    {
        if (_isDragging && _isWarPhase)
        {
            Rect rect = Utils.GetScreenRect(_dragStartPos, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    public void SetWarPhase(bool active)
    {
        _isWarPhase = active;
        if (!active) DeselectAll();
    }

    // --- SPAWNING ---
    public void SpawnUnit(UnitStats type, Vector2Int gridPos)
    {
        // 1. Create Object
        GameObject newUnit = Instantiate(type.unitPrefab, island.worldContainer);
        
        // 2. Add Controller
        UnitController ctrl = newUnit.GetComponent<UnitController>();
        if (ctrl == null) ctrl = newUnit.AddComponent<UnitController>();
        
        // 3. Initialize
        ctrl.Initialize(island.mapManager, gridPos, type);
        _allUnits.Add(ctrl);
    }
    
    // Fallback for random spawn (Legacy support)
    public void SpawnUnitRandomly()
    {
        WorldMap map = island.mapManager.map;
        for (int i = 0; i < 50; i++)
        {
            int rx = Random.Range(0, map.mapSize.x);
            int ry = Random.Range(0, map.mapSize.y);
            if (map.GetCell(rx, ry).Type == CellType.Ground && map.GetCell(rx, ry).OccupyingObject == null)
            {
                SpawnUnit(villagerData, new Vector2Int(rx, ry));
                return;
            }
        }
    }

    // --- UPDATE LOOP ---
    private void Update()
    {
        if (!_isWarPhase) return;

        HandleSelectionInput();
        HandleMovementInput();
    }

    // --- SELECTION ---
    private void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector3.Distance(_dragStartPos, Input.mousePosition) > dragThreshold)
                _isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
            {
                SelectUnitsInBox(_dragStartPos, Input.mousePosition);
                _isDragging = false;
            }
            else
            {
                HandleSingleClickSelection();
            }
        }
    }

    private void HandleSingleClickSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayer))
        {
            UnitController hitUnit = hit.collider.GetComponentInParent<UnitController>();
            if (hitUnit != null)
            {
                if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
                AddToSelection(hitUnit);
            }
        }
        else
        {
             if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
        }
    }

    private void SelectUnitsInBox(Vector3 start, Vector3 end)
    {
        if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();

        Rect selectionRect = Utils.GetScreenRect(start, end);

        foreach (var unit in _allUnits)
        {
            if (unit == null) continue;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            screenPos.y = Screen.height - screenPos.y; 

            if (selectionRect.Contains(screenPos, true))
            {
                AddToSelection(unit);
            }
        }
    }

    private void AddToSelection(UnitController unit)
    {
        if (!_selectedUnits.Contains(unit))
        {
            _selectedUnits.Add(unit);
            unit.OnSelect();
        }
    }

    private void DeselectAll()
    {
        foreach (var unit in _selectedUnits) unit.OnDeselect();
        _selectedUnits.Clear();
    }

    // --- MOVEMENT (COMMAND PATTERN IMPLEMENTATION) ---
    private void HandleMovementInput()
    {
        // Middle Mouse Button (Button 2)
        if (Input.GetMouseButtonDown(2) && _selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                // 1. Calculate Target Center
                Vector3 localPoint = island.worldContainer.InverseTransformPoint(hit.point);
                int targetX = Mathf.RoundToInt(localPoint.x);
                int targetZ = Mathf.RoundToInt(localPoint.z);
                Vector2Int centerPos = new Vector2Int(targetX, targetZ);

                // 2. Get Group Destinations (Anti-stacking logic)
                List<Vector2Int> destinations = GetValidDestinations(centerPos, _selectedUnits.Count);

                // 3. Create and Issue Commands
                for (int i = 0; i < _selectedUnits.Count; i++)
                {
                    if (i < destinations.Count)
                    {
                        // COMMAND PATTERN: Create the packet
                        IUnitCommand moveOrder = new MoveCommand(destinations[i]);
                        
                        // COMMAND PATTERN: Send it
                        _selectedUnits[i].ExecuteCommand(moveOrder);
                    }
                }
            }
        }
    }

    // Anti-Stacking Spiral Search
    private List<Vector2Int> GetValidDestinations(Vector2Int center, int count)
    {
        List<Vector2Int> results = new List<Vector2Int>();
        int radius = 0;
        
        // Loop until we find enough spots or give up
        while (results.Count < count && radius < 10)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (results.Count >= count) return results;

                    Vector2Int p = new Vector2Int(center.x + x, center.y + y);
                    
                    if (island.mapManager.map.isPlacable(p.x, p.y))
                    {
                        if (!results.Contains(p)) results.Add(p);
                    }
                }
            }
            radius++;
        }
        return results;
    }
}