using System.Collections.Generic;
using UnityEngine;


public class ClusterPattern : IGenerationPattern
{
    private ResourceType type;
    private int clusterCount;
    private float radius;
    private float density;

    public ClusterPattern(ResourceType type, int count, float radius, float density)
    {
        this.type = type;
        this.clusterCount = count;
        this.radius = radius;
        this.density = density;
    }

    public void Generate(CellData[,] grid, List<Vector2Int> groundTiles, BiomeConfig biome)
    {
        if (groundTiles.Count == 0) return;

        // Створюємо центри кластерів
        List<ResourceCluster> clusters = new List<ResourceCluster>();
        for (int i = 0; i < clusterCount; i++)
        {
            Vector2 randomCenter = groundTiles[Random.Range(0, groundTiles.Count)];
            clusters.Add(new ResourceCluster(randomCenter, radius, type, density));
        }

        // Проходимось по карті і застосовуємо вплив кластерів
        foreach (var pos in groundTiles)
        {
            if (grid[pos.x, pos.y].Variation != TileVariation.Center) continue;
            if (grid[pos.x, pos.y].OccupyingObject != null) continue;
   
            CellData cell = grid[pos.x, pos.y];
            
            float bestInfluence = 0f;
            foreach (var cluster in clusters)
            {
                float influence = cluster.GetInfluence(pos.x, pos.y);
                if (influence > bestInfluence) 
                bestInfluence = influence;
            }

            if (Random.value < bestInfluence)
            {
                GameObject prefab = biome.GetRandomPrefab(type);
                if (prefab != null)
                {
                    cell.OccupyingObject = prefab;
                }
            }
        }
    }
}