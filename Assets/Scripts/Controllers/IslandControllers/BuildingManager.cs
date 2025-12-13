using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro.EditorUtilities;

public class BuildingManager : IManager
{
    [Header("References")]
    public MapManager mapManager;
    private Camera _cam;
    public LayerMask groundLayer; // Layer for the mouse raycast (Terrain/Ground)

    [Header("Build settings")]
    public Material validMaterial;   // Transparent Green
    public Material invalidMaterial; // Transparent Red

    [Header("Placing settings")]
    public Button CreateBtn;
    public GameObject buildingPrefab;


    private GameObject _currentGhost;
    private GameObject _prefabToBuild;
    private bool _isBuilding = false;
    private float _currentYRotation = 0f;
    private Renderer[] _ghostRenderers;

    public override void Initialize(IslandController controller)
    {
        base.Initialize(controller);
        _cam = Camera.main;
        
    }

    void Start()
    {
        if (CreateBtn != null)
            CreateBtn.onClick.AddListener(StartPlacingBuilding);

    }
    void Update()
    {
        if (!_isBuilding || _currentGhost == null) return;

        // 1. Cancel Build on Right Click or Escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuilding();
            return;
        }
    

        // 2. Raycast to find mouse position on map
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            // 3. Snap to Grid
            int x = Mathf.RoundToInt(hit.point.x);
            int z = Mathf.RoundToInt(hit.point.z);

            // Update Ghost Position (Height 1f to match your resources)
            Vector3 targetPosition = (_currentYRotation == 0 || _currentYRotation == 180) ? new Vector3(x + 0.5f, 1f, z) : new Vector3(x, 1f, z + 0.5f);
            _currentGhost.transform.position = targetPosition;
            _currentGhost.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
            
            Structure ghostScript = _currentGhost.GetComponent<Structure>();            
            bool isValid = IsAreaValid(x, z, ghostScript);
            UpdateGhostColor(isValid);

            // 4. Debugging: Draw the footprint in Scene View
            DrawDebugFootprint(x, z, ghostScript);

            // 5. Build on Left Click
            if (Input.GetMouseButtonDown(0))
            {
                // Prevent clicking through UI
                if (EventSystem.current.IsPointerOverGameObject()) return;

                if (isValid)
                {
                    PlaceBuilding(x, z);
                }
                else
                {
                    Debug.Log("Cannot build here!");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateBuilding();
            }
        }
    }

    public void StartPlacingBuilding()
    {
        if (_isBuilding) CancelBuilding();

        _prefabToBuild = buildingPrefab;
        _isBuilding = true;

        // Create the ghost visual
        _currentGhost = Instantiate(buildingPrefab);
        
        // Disable collider so the raycast doesn't hit the ghost itself
        Collider[] colliders = _currentGhost.GetComponentsInChildren<Collider>();
        foreach (var col in colliders) col.enabled = false;

        // Get renderers to change color later
        _ghostRenderers = _currentGhost.GetComponentsInChildren<Renderer>();
    }

    private void CancelBuilding()
    {
        _isBuilding = false;
        if (_currentGhost != null) Destroy(_currentGhost);
        _prefabToBuild = null;
    }

    private void UpdateGhostColor(bool isValid)
    {
        Material targetMat = isValid ? validMaterial : invalidMaterial;
        
        foreach (var r in _ghostRenderers)
        {
            r.material = targetMat;
        }
    }

    private void PlaceBuilding(int x, int z)
    {

        GameObject newBuildingObj = Instantiate(_prefabToBuild, 
            (_currentYRotation == 0 || _currentYRotation == 180) ? new Vector3(x + 0.5f, 1f, z) : new Vector3(x, 1f, z + 0.5f),
            Quaternion.Euler(0, _currentYRotation, 0), 
            mapManager.worldContainer
        );

        Structure structureScript = newBuildingObj.GetComponent<Structure>();
        List<Vector2Int> shape = structureScript.GetRotatedFootprint(_currentYRotation);

        foreach (Vector2Int offset in shape)
        {
            int targetX = x + offset.x;
            int targetZ = z + offset.y;
            if (mapManager.map.isPlacable(targetX, targetZ)) {
                Debug.Log($"Occupying Cell: {targetX}, {targetZ}");
                mapManager.map.GetCell(targetX, targetZ).OccupyingObject = structureScript;
            }        }

        CancelBuilding();
        mapManager.map.debugGrid();
    }

    public void RotateBuilding()
    {
        if (!_isBuilding || _currentGhost == null) return;

        _currentYRotation += 90f;
        
        if (_currentYRotation >= 360f) _currentYRotation = 0f;

        _currentGhost.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
    }

    private bool IsAreaValid(int pivotX, int pivotZ, Structure buildingScript)
    {
        // Get the specific shape based on current rotation
        List<Vector2Int> shape = buildingScript.GetRotatedFootprint(_currentYRotation);

        foreach (Vector2Int offset in shape)
        {
            int targetX = pivotX + offset.x;
            int targetZ = pivotZ + offset.y;

            // A. Check Bounds
            if (targetX < 0 || targetX >= mapManager.mapSize.x || 
                targetZ < 0 || targetZ >= mapManager.mapSize.y) 
                return false;

            // B. Check Cell Availability
            CellData cell = mapManager.map.GetCell(targetX, targetZ);

            if (cell.Type != CellType.Ground) return false;
            if (cell.OccupyingObject != null) return false;
        }
        return true;
    }

    void DrawDebugFootprint(int x, int z, Structure script)
    {
        List<Vector2Int> shape = script.GetRotatedFootprint(_currentYRotation);
        foreach (Vector2Int offset in shape)
        {
            // Draw a Red Box at every tile the logic THINKS is occupied
            Vector3 center = new Vector3(x + offset.x, 1.5f, z + offset.y);
            Debug.DrawRay(center, Vector3.up * 2, Color.red);
            Debug.DrawLine(center + Vector3.left*0.4f, center + Vector3.right*0.4f, Color.red);
        }
    }
}