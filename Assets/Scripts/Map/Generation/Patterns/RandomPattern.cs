using System.Collections.Generic;
using UnityEngine;

using FortuneIslands.Core;
using FortuneIslands.Map.Enums;

namespace FortuneIslands.Map.Generation.Patterns
{

    public class RandomPattern : IGenerationPattern
    {
        private MapResourceType type;
        private float density;
        private bool requiresEmptySpace;

        public RandomPattern(MapResourceType type, float density, bool requiresEmptySpace = true)
        {
            this.type = type;
            this.density = density;
            this.requiresEmptySpace = requiresEmptySpace;
        }

        public void Generate(WorldMap map, List<Vector2Int> groundTiles, BiomeConfig biome)
        {
            foreach (var pos in groundTiles)
            {
                CellData cell = map.GetCell(pos.x, pos.y);

                if (requiresEmptySpace && cell.OccupyingObject != null) continue;

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
}