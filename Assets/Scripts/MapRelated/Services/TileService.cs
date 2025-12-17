using UnityEngine;

public class TileService
{    

    public void RotateMapTiles(WorldMap map)
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (map.GetCell(x, y).Type == CellType.Ground)
                {
                    CalculateTileVariation(map, x, y);
                }
            }
        }
    }

    public void CalculateTileVariation(WorldMap map, int x, int y)
    {
        // Top(1), Right(2), Bottom(4), Left(8)
        int mask = 0;
        
        if (map.IsGround(x, y + 1)) mask += 1; // Top
        if (map.IsGround(x + 1, y)) mask += 2; // Right
        if (map.IsGround(x, y - 1)) mask += 4; // Bottom
        if (map.IsGround(x - 1, y)) mask += 8; // Left

        map.GetCell(x, y).Bitmask = mask;

        CellData cell = map.GetCell(x, y);
        switch (mask)
        {
            case 0:
                cell.Variation = TileVariation.Isolated;
                break;
            
            // Tips (1 connection)
            case 1: case 2: case 4: case 8:
                cell.Variation = TileVariation.Tip;
                break;

            // Outer Corners (L-Shapes)
            case 3: case 6: case 9: case 12:
                cell.Variation = TileVariation.OuterCorner;
                break;

            // Edges (3-Sides or Tube)
            case 7: case 11: case 13: case 14:
            case 5: case 10:
                cell.Variation = TileVariation.Edge; 
                break;

            // Center or Inner Corner (4 connections)
            case 15:
                if (!map.IsGround(x + 1, y + 1) || !map.IsGround(x + 1, y - 1) || 
                    !map.IsGround(x - 1, y - 1) || !map.IsGround(x - 1, y + 1))
                {
                    cell.Variation = TileVariation.InnerCorner;
                }
                else
                {
                    cell.Variation = TileVariation.Center;
                }
                break;
        }
    } 

    public Quaternion GetRotationForCell(WorldMap map, int x, int y, int mask, TileVariation variation)
    {
        float angle = 0f;

        switch (variation)
        {
            case TileVariation.Tip:
                if (mask == 1) angle = 0f;   
                if (mask == 2) angle = 90f;  
                if (mask == 4) angle = 180f; 
                if (mask == 8) angle = 270f; 
                break;

            case TileVariation.OuterCorner:
                if (mask == 3)  angle = 0f;   
                if (mask == 6)  angle = 90f;  
                if (mask == 12) angle = 180f; 
                if (mask == 9)  angle = 270f; 
                break;

            case TileVariation.Edge:
                if (mask == 14) angle = 180f; // Missing Bottom
                if (mask == 13) angle = 270f; // Missing Right
                if (mask == 11) angle = 0f;   // Missing Top
                if (mask == 7)  angle = 90f;  // Missing Left
                if (mask == 5)  angle = 0f;   // Tube Vertical
                if (mask == 10) angle = 90f;  // Tube Horizontal
                break;

            case TileVariation.InnerCorner:
                bool tr = map.IsGround(x + 1, y + 1);
                bool br = map.IsGround(x + 1, y - 1);
                bool bl = map.IsGround(x - 1, y - 1);
                bool tl = map.IsGround(x - 1, y + 1);

                if (!tr) angle = 0f;    
                else if (!br) angle = 90f;   
                else if (!bl) angle = 180f;  
                else if (!tl) angle = 270f;  
                break;
        }

        return Quaternion.Euler(0, angle - 90f, 0);
    }
 
}