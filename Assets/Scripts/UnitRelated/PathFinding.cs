using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Pathfinding
{
    // Returns null if no path found, or a list of steps if found
    public static List<Vector2Int> FindPath(CellData[,] grid, Vector2Int startPos, Vector2Int targetPos)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Create a temporary grid of Nodes for calculation
        PathNode[,] nodeGrid = new PathNode[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                nodeGrid[x, y] = new PathNode(x, y);

        PathNode startNode = nodeGrid[startPos.x, startPos.y];
        PathNode targetNode = nodeGrid[targetPos.x, targetPos.y];

        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<PathNode> closedList = new HashSet<PathNode>();

        while (openList.Count > 0)
        {
            // 1. Get node with lowest F cost
            PathNode currentNode = openList.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // 2. Check Neighbors
            foreach (PathNode neighbor in GetNeighbors(currentNode, nodeGrid, width, height))
            {
                // Logic: specific to YOUR game (Check Collision)
                CellData cellInfo = grid[neighbor.x, neighbor.y];
                bool isWalkable = cellInfo.Type == CellType.Ground && cellInfo.OccupyingObject == null;

                // Exception: The Target tile might be "Occupied" by the target itself, so we allow it if it is the target
                if (neighbor == targetNode) isWalkable = true; 

                if (!isWalkable || closedList.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + 1; // 1 is distance between tiles
                if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    static List<PathNode> GetNeighbors(PathNode node, PathNode[,] grid, int width, int height)
    {
        List<PathNode> neighbors = new List<PathNode>();

        // Check Up, Down, Left, Right
        int[] xDir = { 0, 0, -1, 1 };
        int[] yDir = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.x + xDir[i];
            int checkY = node.y + yDir[i];

            if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }
        return neighbors;
    }

    static List<Vector2Int> RetracePath(PathNode start, PathNode end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        PathNode current = end;

        while (current != start)
        {
            path.Add(new Vector2Int(current.x, current.y));
            current = current.parent;
        }
        path.Reverse(); // Grid calculates End->Start, so we reverse it
        return path;
    }

    // Manhattan Distance (Best for 4-direction grid)
    static int GetDistance(PathNode a, PathNode b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}