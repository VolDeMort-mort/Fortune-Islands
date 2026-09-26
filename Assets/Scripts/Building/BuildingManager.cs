using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using FortuneIslands.Map;
using FortuneIslands.Economy.Stockpile;
using FortuneIslands.Economy;
using FortuneIslands.Building.Effects;

namespace FortuneIslands.Building
{

    public class BuildingManager : MonoBehaviour
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


        private MapManager _mapManager;
        private ResourceManager _resources;
        private Transform _worldContainer;
        private BuildingContext _ctx;

        public event Action<Structure, Vector2Int> OnStructureBuilt;

        public void Initialize(MapManager map, ResourceManager resources,DiceManager dice,Transform worldContainer)
        {
            _mapManager = map;
            _resources = resources;
            _worldContainer = worldContainer;

            _ctx = new BuildingContext(resources, dice);


            _cam = Camera.main;

            _validator = new PlacementValidator(_mapManager);
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
                    if (_resources.TrySpendResources(script.costs))
                    {
                        CommitBuild(x, z, rotation);
                    }
                }
            }
        }

        public void StartPlacingBuilding(BuildingTypes type)
        {
            if (!_isActive) return; // building is allowed only in the Build phase

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
                _worldContainer
            );

            Structure script = finalObj.GetComponent<Structure>();
            foreach (var tile in script.GetRotatedFootprint(rotation))
            {
                _mapManager.map.GetCell(x + tile.offset.x, z + tile.offset.y).OccupyingObject = script;
            }

            foreach (var comp in finalObj.GetComponents<IBuildingFeature>())
            {
                comp.Initialize(_ctx);
            }

            OnStructureBuilt?.Invoke(script, new Vector2Int(x, z));

            CancelBuilding();

        }
    }
}