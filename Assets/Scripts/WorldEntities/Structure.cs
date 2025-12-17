using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct FootprintTile
{
    public Vector2Int offset;
    public CellType requiredSurface; // Ground, Water, etc.
}

[System.Serializable]
public struct ResourceCost
{
    public ResourceType type;
    public int amount;
}

public class Structure : WorldEntity
{

    [Header("Economy")]
    public List<ResourceCost> costs; // Drag/Set costs in Inspector

    [Header("Shape Configuration")]
    // Replaces the old List<Vector2Int>
    public List<FootprintTile> footprint = new List<FootprintTile>();

    // Returns the full tile data (position + type), but with positions rotated
    public List<FootprintTile> GetRotatedFootprint(float rotationDegrees)
    {
        List<FootprintTile> rotatedFootprint = new List<FootprintTile>();
        
        // Normalize angle to 0, 1, 2, 3 steps
        int steps = Mathf.RoundToInt(rotationDegrees / 90f) % 4;
        if (steps < 0) steps += 4;

        foreach (FootprintTile tile in footprint)
        {
            int x = tile.offset.x;
            int y = tile.offset.y;
            
            // Standard Grid Rotation Math
            for (int i = 0; i < steps; i++)
            {
                int temp = x;
                x = y;
                y = -temp;
            }
            
            // Return new struct with Rotated X/Y but SAME required surface
            rotatedFootprint.Add(new FootprintTile 
            { 
                offset = new Vector2Int(x, y), 
                requiredSurface = tile.requiredSurface 
            });
        }

        return rotatedFootprint;
    }
}