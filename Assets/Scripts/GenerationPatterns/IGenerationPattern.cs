using System.Collections.Generic;
using UnityEngine;

public interface IGenerationPattern
{
    void Generate(CellData[,] grid, List<Vector2Int> groundTiles, BiomeConfig biome);
}
