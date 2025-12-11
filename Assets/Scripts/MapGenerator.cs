using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [Header("UI")]
    public Button BtnGenerate;

    [Header("Configuration")]
    public Transform worldContainer;
    public Vector2Int mapSize = new Vector2Int(100, 100);
    public BiomeConfig[] availableBiomes; 

    [Header("Noise Settings")]
    public float noiseScale = 25f;
    [Range(1, 10)] public int noiseOctaves = 4;
    [Range(0f, 1f)] public float noiseThreshold = 0.4f;
    public float falloffStrength = 3f;
    public float islandSizeMultiplier = 1f;

    [Header("Resource Settings")]
    public int treeClusterCount = 5;
    public float treeClusterRadius = 5f;
    [Range(0f, 1f)]public float treeClusterDensity = 0.5f;
    public int rockClusterCount = 3;
    public float rockClusterRadius = 5f;
    [Range(0f, 1f)]public float rockClusterDensity = 0.5f;

    public int goldClusterCount = 1;
    public float goldClusterRadius = 2f;
    [Range(0f, 1f)]public float goldClusterDensity = 0.5f;


    [Range(0f, 1f)]public float grassDensity = 0.2f;

    [Header("Unit Settings")]
    public Button BtnSpawnUnit;    // Drag new button here
    public GameObject villagerPrefab; // Drag villager prefab here

    // The Grid Variable
    public CellData[,] Grid { get; private set; }
    
    private BiomeConfig currentBiome;

    void Start()
    {
        // if (BtnGenerate != null)
            // BtnGenerate.onClick.AddListener(GenerateWorld);
        GenerateWorld();
        SpawnVillager();
        if (BtnSpawnUnit != null) 
            BtnSpawnUnit.onClick.AddListener(SpawnVillager);
    }

    public void GenerateWorld()
    {
        ClearWorld();
        InitializeGrid();
        
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

        // 1. Generate Layer 0 (Terrain Data)
        GenerateTerrainData();
        
        // 2. Generate Layer 1 (Resources Data)
        ClearResources();
        GenerateResourceData();

        // 3. Instantiate Visuals based on Data
        RenderMap();     
    }

    // Map
    void InitializeGrid()
    {
        Grid = new CellData[mapSize.x, mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Grid[x, y] = new CellData(x, y);
            }
        }
    }

    // Map class
    void GenerateTerrainData()
    {
        float seed = UnityEngine.Random.Range(0f, 10000f);
        Vector2 offset = new Vector2(seed, seed);

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // 1. Calculate Noise + Falloff
                float noiseVal = GetNoiseValue(x, y, offset);
                float falloff = GetFalloffValue(x, y);
                float finalValue = noiseVal - falloff;

                // 2. Determine Type
                if (finalValue > noiseThreshold)
                {
                    Grid[x, y].Type = CellType.Ground;
                }
                else
                {
                    Grid[x, y].Type = CellType.Water;
                }
            }
        }

        // 3. Post-Processing: Calculate Bitmask for Edges/Corners
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (Grid[x, y].Type == CellType.Ground)
                {
                    CalculateTileVariation(x, y);
                }
            }
        }
    }
    
    // Map class
    void CalculateTileVariation(int x, int y)
    {
        // Top(1), Right(2), Bottom(4), Left(8)
        int mask = 0;
        
        if (IsGround(x, y + 1)) mask += 1; // Top
        if (IsGround(x + 1, y)) mask += 2; // Right
        if (IsGround(x, y - 1)) mask += 4; // Bottom
        if (IsGround(x - 1, y)) mask += 8; // Left

        Grid[x, y].Bitmask = mask;

        switch (mask)
        {
            case 0:
                Grid[x, y].Variation = TileVariation.Isolated;
                break;
            
            // Tips (1 connection)
            case 1: case 2: case 4: case 8:
                Grid[x, y].Variation = TileVariation.Tip;
                break;

            // Outer Corners (L-Shapes)
            case 3: case 6: case 9: case 12:
                Grid[x, y].Variation = TileVariation.OuterCorner;
                break;

            // Edges (3-Sides or Tube)
            case 7: case 11: case 13: case 14:
            case 5: case 10:
                Grid[x, y].Variation = TileVariation.Edge; 
                break;

            // Center or Inner Corner (4 connections)
            case 15:
                if (!IsGround(x + 1, y + 1) || !IsGround(x + 1, y - 1) || 
                    !IsGround(x - 1, y - 1) || !IsGround(x - 1, y + 1))
                {
                    Grid[x, y].Variation = TileVariation.InnerCorner;
                }
                else
                {
                    Grid[x, y].Variation = TileVariation.Center;
                }
                break;
        }
    }

    // Map class
    bool IsGround(int nx, int ny) 
    {
        if (nx < 0 || nx >= mapSize.x || ny < 0 || ny >= mapSize.y) return false; 
        return Grid[nx, ny].Type == CellType.Ground;
    }

    // Map class
    void GenerateResourceData()
    {
        List<Vector2Int> groundTiles = GetGroundTiles();
        if (groundTiles.Count == 0) return;

        List<IGenerationPattern> generationSteps = new List<IGenerationPattern>();

        generationSteps.Add(new ClusterPattern(ResourceType.Tree,treeClusterCount, treeClusterRadius, treeClusterDensity));
        
        generationSteps.Add(new ClusterPattern(ResourceType.Rock, rockClusterCount, rockClusterRadius, rockClusterDensity));
        
        generationSteps.Add(new ClusterPattern(ResourceType.Gold, goldClusterCount, goldClusterRadius, goldClusterDensity));

        generationSteps.Add(new RandomPattern(ResourceType.Grass, grassDensity, true));


        foreach (var pattern in generationSteps)
        {
            pattern.Generate(Grid, groundTiles, currentBiome);
        }
    }

    // Map class
    void RenderMap()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                CellData cell = Grid[x, y];
                Vector3 pos = new Vector3(0, 0, 0);

                // --- Layer 0: Surface ---
                GameObject surfacePrefab = null;
                Quaternion rotation = Quaternion.identity;

                if (cell.Type == CellType.Ground)
                {
                    pos = new Vector3(x, 0.15f, y);
                    rotation = GetRotationForCell(x, y, cell.Bitmask, cell.Variation);
                    switch (cell.Variation)
                    {
                        case TileVariation.Center:      surfacePrefab = currentBiome.groundCenter; break;
                        case TileVariation.InnerCorner: surfacePrefab = currentBiome.groundInnerCorner; break;
                        case TileVariation.OuterCorner: surfacePrefab = currentBiome.groundOuterCorner; break;
                        case TileVariation.Edge:        surfacePrefab = currentBiome.groundEdge; break;
                        case TileVariation.Tip:         surfacePrefab = currentBiome.groundTip; break;
                        case TileVariation.Isolated:    surfacePrefab = currentBiome.groundIsolated; break;
                        default:                        surfacePrefab = currentBiome.groundCenter; break;
                    }

                    // --- Layer 1: Resources ---
                    if (cell.OccupyingObject != null)
                    {
                        Instantiate(cell.OccupyingObject, new Vector3(x, 1f, y), Quaternion.identity, worldContainer);
                        // cell.OccupyingObject = resourceObj; 
                    }
                }
                else if(cell.Type == CellType.Water)
                {
                    pos = new Vector3(x, 0, y);
                    surfacePrefab = currentBiome.deepWater;
                }

                Instantiate(surfacePrefab, pos, rotation, worldContainer);
            }
        }
    }

    // Map class
    Quaternion GetRotationForCell(int x, int y, int mask, TileVariation variation)
    {
        float angle = 0f;

        switch (variation)
        {
            case TileVariation.Tip:
                if (mask == 1) angle = 0f;   
                if (mask == 2) angle = 90f;  
                if (mask == 4) angle = 180f; 
                if (mask == 8) angle = 270f; 
                break;

            case TileVariation.OuterCorner:
                if (mask == 3)  angle = 0f;   
                if (mask == 6)  angle = 90f;  
                if (mask == 12) angle = 180f; 
                if (mask == 9)  angle = 270f; 
                break;

            case TileVariation.Edge:
                if (mask == 14) angle = 180f; // Missing Bottom
                if (mask == 13) angle = 270f; // Missing Right
                if (mask == 11) angle = 0f;   // Missing Top
                if (mask == 7)  angle = 90f;  // Missing Left
                if (mask == 5)  angle = 0f;   // Tube Vertical
                if (mask == 10) angle = 90f;  // Tube Horizontal
                break;

            case TileVariation.InnerCorner:
                bool tr = IsGround(x + 1, y + 1);
                bool br = IsGround(x + 1, y - 1);
                bool bl = IsGround(x - 1, y - 1);
                bool tl = IsGround(x - 1, y + 1);

                if (!tr) angle = 0f;    
                else if (!br) angle = 90f;   
                else if (!bl) angle = 180f;  
                else if (!tl) angle = 270f;  
                break;
        }

        return Quaternion.Euler(0, angle - 90f, 0);
    }

    // --- Helpers ---

    float GetNoiseValue(int x, int y, Vector2 offset)
    {
        float noiseVal = 0;
        float scale = noiseScale;
        float opacity = 1;
        float norm = 0;

        for (int i = 0; i < noiseOctaves; i++)
        {
            float xCoord = (x / scale) + offset.x;
            float yCoord = (y / scale) + offset.y;
            // Requires "Unity.Mathematics" package installed via Package Manager
            noiseVal += noise.snoise(new float2(xCoord, yCoord)) * opacity;
            norm += opacity;
            scale /= 2f;
            opacity *= 0.5f;
        }
        return Mathf.InverseLerp(-1, 1, noiseVal / norm);
    }

    float GetFalloffValue(int x, int y)
    {
        float xv = x / (float)mapSize.x * 2 - 1;
        float yv = y / (float)mapSize.y * 2 - 1;
        float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
        
        float a = falloffStrength;
        float b = islandSizeMultiplier; 
        return Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
    }


    // Map
    List<Vector2Int> GetGroundTiles()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (Grid[x, y].Type == CellType.Ground) list.Add(new Vector2Int(x, y));
            }
        }
        return list;
    }

    // Map
    void ClearWorld()
    {
        for (int i = worldContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(worldContainer.GetChild(i).gameObject);
        }
    }
    void ClearGrid()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Grid[x, y] = null;
            }
        }
    }

    void ClearResources()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (Grid[x, y].OccupyingObject != null){
                    Destroy(Grid[x, y].OccupyingObject);
                    Grid[x, y].OccupyingObject = null;
                }
            }
        }
    }

    // General
    void SpawnVillager()
    {
        if (Grid == null)
        {
            Debug.LogError("Generate the map first!");
            return;
        }

        // Try 100 times to find a random empty spot
        for (int i = 0; i < 100; i++)
        {
            int rx = UnityEngine.Random.Range(0, mapSize.x);
            int ry = UnityEngine.Random.Range(0, mapSize.y);
            CellData cell = Grid[rx, ry];

            // Check if valid spawn point (Ground + No Tree/Rock + No other Villager)
            if (cell.Type == CellType.Ground && cell.OccupyingObject == null)
            {
                // Instantiate
                Vector3 spawnPos = new Vector3(rx, 2f, ry);
                GameObject unitObj = Instantiate(villagerPrefab, spawnPos, Quaternion.identity, worldContainer);
                
                // Initialize the AI
                VillagerController controller = unitObj.GetComponent<VillagerController>();
                if (controller != null)
                {
                    controller.Initialize(this, new Vector2Int(rx, ry));
                }
                
                Debug.Log($"Spawned Villager at {rx}, {ry}");
                return; // Success, exit function
            }
        }
        Debug.LogWarning("Could not find empty spot for Villager.");
    }
}