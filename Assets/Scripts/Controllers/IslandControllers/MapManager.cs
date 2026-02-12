using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : IManager
{
    [Header("UI")]

    [Header("Configuration")]
    public Vector2Int mapSize;
    public Transform worldContainer;
    public MapConfig mapConfig;
    public BiomeConfig[] availableBiomes; 


    [Header("Unit Settings")]
    public GameObject villagerPrefab;


    public WorldMap map;
    private TileService _tileService;
    private BiomeConfig currentBiome;
    private NoiseService _noiseService;
    private MapResourceGenService _resourceGenService;
    private RenderService _renderer;

    public override void Initialize(IslandController controller)
    {
        island = controller;
        // worldContainer = controller.worldContainer;

        map = new WorldMap(mapSize);
        _tileService = new TileService();
        _noiseService = new NoiseService();
        _resourceGenService = new MapResourceGenService();
        _renderer = new RenderService();


        GenerateWorld();   
    }

    public void GenerateWorld()
    {
        if (availableBiomes.Length > 0)
        {
            currentBiome = availableBiomes[UnityEngine.Random.Range(0, availableBiomes.Length)];
            Debug.Log($"Selected Biome: {currentBiome.biomeName}");
        }
        else
        {
            Debug.LogError("No Biomes assigned in Inspector!");
            return;
        }

        _noiseService.GenerateTerrain(map, mapConfig);
        _tileService.RotateMapTiles(map);
        // _resourceGenService.GenerateResourceData(map, currentBiome, mapConfig);
        _renderer.RenderMap(map, worldContainer, currentBiome, _tileService, island.PlayerID);
    }

    // void ClearWorld()
    // {
    //     for (int i = worldContainer.childCount - 1; i >= 0; i--)
    //     {
    //         Destroy(worldContainer.GetChild(i).gameObject);
    //     }
    // }
    // void ClearGrid()
    // {
    //     for (int x = 0; x < map.mapSize.x; x++)
    //     {
    //         for (int y = 0; y < map.mapSize.y; y++)
    //         {
    //             map.SetCell(x, y, null);
    //         }
    //     }
    // }



    // void SpawnVillager()
    // {
    //     if (map == null)
    //     {
    //         Debug.LogError("Generate the map first!");
    //         return;
    //     }

    //     // Try 100 times to find a random empty spot
    //     for (int i = 0; i < 100; i++)
    //     {
    //         int rx = UnityEngine.Random.Range(0, map.mapSize.x);
    //         int ry = UnityEngine.Random.Range(0, map.mapSize.y);
    //         CellData cell = map.GetCell(rx, ry);

    //         // Check if valid spawn point (Ground + No Tree/Rock + No other Villager)
    //         if (cell.Type == CellType.Ground && cell.OccupyingObject == null)
    //         {
    //             Vector3 spawnPos = new Vector3(rx, 2f, ry);
    //             GameObject unitObj = Instantiate(villagerPrefab, worldContainer);
                
    //             unitObj.transform.localPosition = spawnPos; 
                
    //             // Initialize 
    //             VillagerController controller = unitObj.GetComponent<VillagerController>();
    //             if (controller != null)
    //             {
    //                 controller.Initialize(this, new Vector2Int(rx, ry));
    //             }
                
    //             Debug.Log($"Spawned Villager at {rx}, {ry}");
    //             return;
    //         }
    //     }
    //     Debug.LogWarning("Could not find empty spot for Villager.");
    // }
}