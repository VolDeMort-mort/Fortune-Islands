using UnityEngine;
using System.Collections.Generic;

public class MapResourceGenService: MonoBehaviour
{


    public void GenerateResourceData(WorldMap map, BiomeConfig biome, MapConfig mapConfig)
    {
        ClearResources(map);
        List<Vector2Int> groundTiles = map.GetGroundTiles();
        if (groundTiles.Count == 0) return;

        List<IGenerationPattern> generationSteps = new List<IGenerationPattern>();

        generationSteps.Add(new ClusterPattern(MapResourceType.Tree, mapConfig.treeClusterCount, mapConfig.treeClusterRadius, mapConfig.treeClusterDensity));
        
        generationSteps.Add(new ClusterPattern(MapResourceType.Rock, mapConfig.rockClusterCount, mapConfig.rockClusterRadius, mapConfig.rockClusterDensity));
        
        generationSteps.Add(new ClusterPattern(MapResourceType.Gold, mapConfig.goldClusterCount, mapConfig.goldClusterRadius, mapConfig.goldClusterDensity));

        generationSteps.Add(new RandomPattern(MapResourceType.Grass, mapConfig.grassDensity, true));


        foreach (var pattern in generationSteps)
        {
            pattern.Generate(map, groundTiles, biome);
        }
    }

    private void ClearResources(WorldMap map)
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (map.GetCell(x, y).OccupyingObject != null){
                    Destroy(map.GetCell(x, y).OccupyingObject);
                    map.GetCell(x, y).OccupyingObject = null;
                }
            }
        }
    }
}