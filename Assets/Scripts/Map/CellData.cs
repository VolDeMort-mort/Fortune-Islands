using UnityEngine;
using FortuneIslands.Core;

using FortuneIslands.Map.Enums;

namespace FortuneIslands.Map
{
    [System.Serializable]
    public class CellData
    {
        public Vector2Int Coordinates;
        public CellType Type { get; set; }
        public TileVariation Variation;

        public WorldEntity OccupyingObject;

        public IGridOccupant OccupyingUnit;

        public bool IsWalkable => Type == CellType.Ground && OccupyingObject == null && OccupyingUnit == null;
        // Bitmask value (0-15) to determine specific mesh rotation/type
        public int Bitmask;

        public CellData(int x, int y)
        {
            Coordinates = new Vector2Int(x, y);
            Type = CellType.Water;
        }
    }
}