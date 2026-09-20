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
        _resourceGenService.GenerateResourceData(map, currentBiome, mapConfig);
        _renderer.RenderMap(map, worldContainer, currentBiome, _tileService, island.PlayerID);
    }
}