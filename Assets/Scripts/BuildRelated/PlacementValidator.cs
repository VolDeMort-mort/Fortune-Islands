using UnityEngine;
using System.Collections.Generic;

public class PlacementValidator
{
    private MapManager _mapManager;

    public PlacementValidator(MapManager mapManager)
    {
        _mapManager = mapManager;
    }

    public bool Validate(int pivotX, int pivotZ, Structure building, float rotation)
    {
        List<FootprintTile> shape = building.GetRotatedFootprint(rotation);
        
        foreach (FootprintTile tile in shape)
        {
            int tx = pivotX + tile.offset.x;
            int tz = pivotZ + tile.offset.y;

            // 1. Bounds Check
            if (!_mapManager.map.isPlacable(tx, tz)) 
                return false;

            // 2. Data Check
            CellData cell = _mapManager.map.GetCell(tx, tz);
            
            if (cell.Type != tile.requiredSurface) return false;
        }

        return true;
    }
}