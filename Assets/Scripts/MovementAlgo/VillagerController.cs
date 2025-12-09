using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerController : MonoBehaviour
{
    private MapGenerator mapRef;
    private Vector2Int currentGridPos;
    private bool isMoving = false;
    
    public float moveSpeed = 1.5f;

    public void Initialize(MapGenerator map, Vector2Int startPos)
    {
        this.mapRef = map;
        this.currentGridPos = startPos;
        transform.position = new Vector3(startPos.x, 1f, startPos.y);
        
        // Occupy start pos
        mapRef.Grid[startPos.x, startPos.y].OccupyingObject = this.gameObject;

        StartCoroutine(LifeCycle());
    }

    IEnumerator LifeCycle()
    {
        yield return new WaitForSeconds(1f); // Boot up time

        while (true)
        {
            // 1. Pick a random valid tile on the map as a destination
            Vector2Int destination = GetRandomDestination();

            // 2. Calculate A* Path
            List<Vector2Int> path = Pathfinding.FindPath(mapRef.Grid, currentGridPos, destination);

            // 3. If path is valid, follow it
            if (path != null && path.Count > 0)
            {
                yield return StartCoroutine(FollowPath(path));
            }
            else
            {
                // No path found (maybe trapped on an island), wait and try again
                yield return new WaitForSeconds(2f);
            }
            
            // Wait at destination before moving again
            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

    Vector2Int GetRandomDestination()
    {
        // Try to find a walkable ground tile
        for(int i=0; i<50; i++)
        {
            int x = Random.Range(0, mapRef.mapSize.x);
            int y = Random.Range(0, mapRef.mapSize.y);
            
            // Only go there if it is Ground and Empty
            if (mapRef.Grid[x,y].Type == CellType.Ground && mapRef.Grid[x,y].OccupyingObject == null)
            {
                return new Vector2Int(x, y);
            }
        }
        return currentGridPos; // Fail safe
    }

    IEnumerator FollowPath(List<Vector2Int> path)
    {
        isMoving = true;

        foreach (Vector2Int step in path)
        {
            // Re-check: Is the next step STILL empty? (Maybe another villager walked there while we were moving)
            if (mapRef.Grid[step.x, step.y].OccupyingObject != null)
            {
                // Path blocked! Stop here and recalculate later
                break; 
            }

            // LOGIC: Swap Grid Data
            mapRef.Grid[currentGridPos.x, currentGridPos.y].OccupyingObject = null;
            mapRef.Grid[step.x, step.y].OccupyingObject = this.gameObject;
            currentGridPos = step;

            // VISUAL: Smooth movement
            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(step.x, 1f, step.y);
            float t = 0;

            transform.LookAt(endPos);

            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            transform.position = endPos;
        }

        isMoving = false;
    }
}