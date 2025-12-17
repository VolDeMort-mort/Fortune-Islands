using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class WorldMap
{

    public Vector2Int mapSize {get; set; }
    private CellData[,] Grid { get; set; }

    // Map
    public WorldMap(Vector2Int size)
    {
        mapSize = size;
        
        InitializeGrid();
    }
    public void InitializeGrid()
    {
        Grid = new CellData[mapSize.x, mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Grid[x, y] = new CellData(x, y);
            }
        }
    }

    public CellData GetCell(int x, int y)
    {
        return Grid[x, y];
    }

    public void SetCell(int x, int y, CellData data)
    {
        Grid[x, y] = data;
    }

    public CellData[,] GetGrid()
    {
        return Grid;
    }

    public bool IsGround(int nx, int ny) 
    {
        if (nx < 0 || nx >= mapSize.x || ny < 0 || ny >= mapSize.y) return false; 
        return Grid[nx, ny].Type == CellType.Ground;
    }

    public bool isPlacable(int x, int z)
    {
        if (x < 0 || x >= mapSize.x || z < 0 || z >= mapSize.y) 
            return false;

        CellData cell = Grid[x, z];

        // if (cell.Type != CellType.Ground) return false;
        if (cell.OccupyingObject != null) return false;

        return true;
    }
    public List<Vector2Int> GetGroundTiles()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (Grid[x, y].Type == CellType.Ground) list.Add(new Vector2Int(x, y));
            }
        }
        return list;
    }

    public void debugGrid()
    {
        string str = "";
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (GetCell(x, y).OccupyingObject != null)
                    str += $"{x} {y} {GetCell(x, y).OccupyingObject.GetType()}\n";
            }
        }

        Debug.Log($"Curr placed obj: {str}");
    }
}