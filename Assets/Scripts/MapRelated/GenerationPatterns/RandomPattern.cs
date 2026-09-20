using System.Collections.Generic;
using UnityEngine;


public class RandomPattern : IGenerationPattern
{
    private MapResourceType type;
    private float density; 

    public RandomPattern(MapResourceType type, float density, bool requiresEmptySpace = true)
    {
        this.type = type;
        this.density = density;
    }

    public void Generate(WorldMap map, List<Vector2Int> groundTiles, BiomeConfig biome)
    {
        foreach (var pos in groundTiles)
        {
            CellData cell = map.GetCell(pos.x, pos.y);

            if (Random.value < density)
            {
                GameObject prefab = biome.GetRandomPrefab(type);
                
                if (prefab != null)
                {
                    cell.OccupyingObject = prefab.GetComponent<WorldEntity>(); 
                }
            }
        }
    }
}
