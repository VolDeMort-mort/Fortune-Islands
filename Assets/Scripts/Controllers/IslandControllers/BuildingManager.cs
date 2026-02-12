using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingManager : IManager
{
    [System.Serializable]
    public class BuildingTypeObj { public BuildingTypes buildingType; public GameObject gameObj; }

    [Header("Configuration")]
    public List<BuildingTypeObj> allBuildings;
    public LayerMask groundLayer;
    
    [Header("Visual Settings")]
    public Material validMaterial;
    public Material invalidMaterial;

    private PlacementValidator _validator; 
    private PlacementRenderer _pRenderer; 
    
    private GameObject _prefabToBuild;
    private bool _isBuilding = false;
    private Camera _cam;

    bool _isActive = false;

    public override void Initialize(IslandController controller)
    {
        base.Initialize(controller);
        _cam = Camera.main;
        
        _validator = new PlacementValidator(island.mapManager);
        _pRenderer = new PlacementRenderer(validMaterial, invalidMaterial, groundLayer);
    }

    public void SetActive(bool isActive)
    {
        this._isActive = isActive;
        
        if (!isActive && _isBuilding)
        {
            CancelBuilding();
        }
    }

    void Update()
    {
        if (!_isBuilding) return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuilding();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R)) _pRenderer.RotateGhost();

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            int x = Mathf.RoundToInt(hit.point.x);
            int z = Mathf.RoundToInt(hit.point.z);
            float rotation = _pRenderer.GetRotation();

            Structure script = _prefabToBuild.GetComponent<Structure>();
            bool isValid = _validator.Validate(x, z, script, rotation);

            _pRenderer.UpdateGhost(new Vector3(x, 1f, z), rotation, isValid);
            _pRenderer.UpdateHighlights(x, z, script, rotation, isValid);

            if (Input.GetMouseButtonDown(0) && isValid && !EventSystem.current.IsPointerOverGameObject())
            {
                if (island.resourceManager.TrySpendResources(script.costs))
                {
                    CommitBuild(x, z, rotation);
                }
            }
        }
    }

    public void StartPlacingBuilding(BuildingTypes type)
    {
        if (_isBuilding) CancelBuilding();

        var target = allBuildings.Find(b => b.buildingType == type);
        if (target == null) return;

        _prefabToBuild = target.gameObj;
        _isBuilding = true;
        
        _pRenderer.SpawnGhost(_prefabToBuild);
    }

    private void CancelBuilding()
    {
        _isBuilding = false;
        _prefabToBuild = null;
        
        _pRenderer.ClearGhost();
        _pRenderer.ClearHighlights();
    }

    private void CommitBuild(int x, int z, float rotation)
    {
        GameObject finalObj = Instantiate(_prefabToBuild, 
            new Vector3(x, 1f, z), 
            Quaternion.Euler(0, rotation, 0), 
            island.worldContainer
        );

        Structure script = finalObj.GetComponent<Structure>();
        foreach (var tile in script.GetRotatedFootprint(rotation))
        {
            island.mapManager.map.GetCell(x + tile.offset.x, z + tile.offset.y).OccupyingObject = script;
        }

        foreach (var comp in finalObj.GetComponents<IBuildingFeature>())
        {
            comp.Initialize(island);
        }


        Vector2Int spawnPos = new Vector2Int(x + 1, z);

        if (island.mapManager.map.isPlacable(spawnPos.x, spawnPos.y))
        {
            island.unitManager.SpawnUnit(island.unitManager.warriorData, spawnPos);
        }
        else
        {
            island.unitManager.SpawnUnitRandomly();
        }

        CancelBuilding();
    }
}