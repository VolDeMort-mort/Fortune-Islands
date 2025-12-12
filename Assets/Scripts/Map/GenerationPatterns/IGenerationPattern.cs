using System.Collections.Generic;
using UnityEngine;

public interface IGenerationPattern
{
    void Generate(WorldMap map, List<Vector2Int> groundTiles, BiomeConfig biome);
}
