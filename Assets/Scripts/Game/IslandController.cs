using UnityEngine;

using FortuneIslands.Building;
using FortuneIslands.Economy.Stockpile;
using FortuneIslands.Economy;
using FortuneIslands.Units;
using FortuneIslands.Map;

namespace FortuneIslands.Game
{
    public class IslandController : MonoBehaviour
    {
        [Header("Identity")]
        public int PlayerID;
        public bool isLocalPlayer;
        public Transform worldContainer;

        [Header("Island managers")]
        public MapManager mapManager;
        public BuildingManager buildManager;
        public ResourceManager resourceManager;
        public DiceManager diceManager;
        public UnitManager unitManager;

        public void Initialize(int id)
        {
            PlayerID = id;
            resourceManager.Initialize();
            mapManager.Initialize(worldContainer, id);
            diceManager.Initialize(resourceManager);
            unitManager.Initialize(mapManager, worldContainer);
            buildManager.Initialize(mapManager, resourceManager, diceManager, worldContainer);

            buildManager.OnStructureBuilt += HandleStructureBuilt;


            unitManager.SpawnUnitRandomly();
        }

        public void OnDestroy()
        {
            if (buildManager != null) buildManager.OnStructureBuilt -= HandleStructureBuilt;
        }

        private void HandleStructureBuilt(Structure structure, Vector2Int pivot)
        {
            Vector2Int spawnPos = new Vector2Int(pivot.x + 1, pivot.y);

            if (mapManager.map.isPlacable(spawnPos.x, spawnPos.y))
                unitManager.SpawnUnit(unitManager.warriorData, spawnPos);
            else
                unitManager.SpawnUnitRandomly();
        }

    }
}