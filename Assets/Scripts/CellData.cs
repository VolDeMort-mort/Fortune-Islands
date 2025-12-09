using UnityEngine;

public enum CellType { Water, Ground }

public enum TileVariation 
{ 
    Center, 
    InnerCorner, 
    OuterCorner, 
    Edge, 
    Tip, 
    Isolated 
}
[System.Serializable]
public class CellData
{
    public Vector2Int Coordinates;
    public CellType Type;
    public TileVariation Variation;
    
    // Layer 1: What is on top of this block?
    public GameObject OccupyingObject; 
    public bool IsWalkable => Type == CellType.Ground && OccupyingObject == null;

    // Bitmask value (0-15) to determine specific mesh rotation/type
    public int Bitmask; 

    public CellData(int x, int y)
    {
        Coordinates = new Vector2Int(x, y);
        Type = CellType.Water;
    }
}