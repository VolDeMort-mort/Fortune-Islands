using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public Camera mainCamera;
    public LayerMask groundLayer; // Layer for the mouse raycast (Terrain/Ground)

    [Header("Settings")]
    public Material validMaterial;   // Transparent Green
    public Material invalidMaterial; // Transparent Red

    private GameObject _currentGhost;
    private GameObject _prefabToBuild;
    private bool _isBuilding = false;
    private float _currentYRotation = 0f;
    private Renderer[] _ghostRenderers;

    public Button CreateBtn;

    public GameObject buildingPrefab;

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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            // 3. Snap to Grid
            int x = Mathf.RoundToInt(hit.point.x);
            int z = Mathf.RoundToInt(hit.point.z);

            // Update Ghost Position (Height 1f to match your resources)
            _currentGhost.transform.position = new Vector3(x + 0.5f, 1f, z);

            // 4. Validate and Color
            bool isValid = mapGenerator.map.isPlacable(x, z);
            UpdateGhostColor(isValid);

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
        // 1. Instantiate Real Object
        GameObject newBuilding = Instantiate(_prefabToBuild, new Vector3(x + 0.5f, 1f, z), Quaternion.Euler(0, _currentYRotation, 0), mapGenerator.worldContainer);

        // 2. Update Data Grid
        CellData cell = mapGenerator.map.GetCell(x, z);
        cell.OccupyingObject = newBuilding;
        
        // newBuilding.AddComponent<SelectableSurface>(); 

        Debug.Log($"Building placed at {x}, {z}");

        // 4. Cleanup
        CancelBuilding();
    }

    public void RotateBuilding()
    {
        if (!_isBuilding || _currentGhost == null) return;

        _currentYRotation += 90f;
        
        if (_currentYRotation >= 360f) _currentYRotation = 0f;

        _currentGhost.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
    }
}