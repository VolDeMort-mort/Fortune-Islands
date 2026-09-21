using System.Collections.Generic;
using UnityEngine;

namespace FortuneIslands.Map.Generation.Patterns
{
    public interface IGenerationPattern
    {
        void Generate(WorldMap map, List<Vector2Int> groundTiles, BiomeConfig biome);
    }
}