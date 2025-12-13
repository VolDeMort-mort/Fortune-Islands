using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;

public class Structure: WorldEntity
{
    [Header("Shape Configuration")]
    public List<Vector2Int> footprint = new List<Vector2Int> { Vector2Int.zero };

    public List<Vector2Int> GetRotatedFootprint(float rotationDegrees)
    {
        List<Vector2Int> rotatedFootprint = new List<Vector2Int>();
        
        // Normalize angle to 0, 1, 2, 3 (for 90 degree steps)
        int steps = Mathf.RoundToInt(rotationDegrees / 90f) % 4;
        if (steps < 0) steps += 4;

        foreach (Vector2Int pos in footprint)
        {
            int x = pos.x;
            int y = pos.y;
            
            // Standard Grid Rotation Math
            for (int i = 0; i < steps; i++)
            {
                int temp = x;
                x = y;
                y = -temp;
            }
            
            rotatedFootprint.Add(new Vector2Int(x, y));
        }

        return rotatedFootprint;
    }

    // public Vector3 PosistionOffset(int rotation, int x, int z)
    // {
    //     return (rotation == 0 || rotation == 180) ? new Vector3(x, 1f, z + 0.5f) : new Vector3(x, 1f, z + 0.5f);
    // }
}