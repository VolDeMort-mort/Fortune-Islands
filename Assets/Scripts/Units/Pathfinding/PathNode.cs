public class PathNode
{
    public int x;
    public int y;

    public int gCost; // Cost from start
    public int hCost; // Heuristic (distance to end)
    public int fCost => gCost + hCost; // Total cost

    public PathNode parent; // To retrace the path backwards

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}