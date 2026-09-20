using UnityEngine;

public class RenderService
{
    // The main entry point
    public void RenderMap(WorldMap map, Transform container, BiomeConfig biome, TileService tileService, int ownerID)
    {
        RenderSurfaces(map, container, biome, tileService, ownerID);
        RenderResources(map, container, ownerID);
    }

    private void RenderSurfaces(WorldMap map, Transform container, BiomeConfig biome, TileService tileService, int ownerID)
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                CellData cell = map.GetCell(x, y);
                Vector3 localPos = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                GameObject surfacePrefab = null;

                // 1. Determine Type & Rotation
                if (cell.Type == CellType.Ground)
                {
                    localPos = new Vector3(x, 0.15f, y);
                    rotation = tileService.GetRotationForCell(map, x, y, cell.Bitmask, cell.Variation);
                    
                    switch (cell.Variation)
                    {
                        case TileVariation.Center:      surfacePrefab = biome.groundCenter; break;
                        case TileVariation.InnerCorner: surfacePrefab = biome.groundInnerCorner; break;
                        case TileVariation.OuterCorner: surfacePrefab = biome.groundOuterCorner; break;
                        case TileVariation.Edge:        surfacePrefab = biome.groundEdge; break;
                        case TileVariation.Tip:         surfacePrefab = biome.groundTip; break;
                        case TileVariation.Isolated:    surfacePrefab = biome.groundIsolated; break;
                        default:                        surfacePrefab = biome.groundCenter; break;
                    }
                }
                else if(cell.Type == CellType.Water)
                {
                    localPos = new Vector3(x, 0, y);
                    surfacePrefab = biome.deepWater;
                }

                // 2. Instantiate & Link WorldEntity
                if (surfacePrefab != null)
                {
                    GameObject newTile = Object.Instantiate(surfacePrefab, container);
                    newTile.transform.localPosition = localPos;
                    newTile.transform.localRotation = rotation;

                    // Connect to your WorldEntity system
                    Surface surfaceScript = newTile.GetComponent<Surface>();
                    if (surfaceScript != null)
                    {
                        surfaceScript.OwnerPlayerID = ownerID;
                    }
                }
            }
        }
    }

    private void RenderResources(WorldMap map, Transform container, int ownerID)
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                CellData cell = map.GetCell(x, y);

                // Check if the generator placed a prefab here
                if (cell.OccupyingObject != null)
                {
                    // cell.OccupyingObject currently holds the PREFAB (from Generation Step)
                    GameObject prefab = cell.OccupyingObject.gameObject;
                    
                    // Instantiate the LIVE version
                    GameObject resObj = Object.Instantiate(prefab, container);
                    resObj.transform.localPosition = new Vector3(x, 1f, y);

                    // Connect to WorldEntity system
                    WorldEntity entityScript = resObj.GetComponent<WorldEntity>();
                    
                    if (entityScript != null)
                    {
                        // Resources usually belong to "Nature" (-1) or the Island Owner
                        // entityScript.OwnerPlayerID = -1; // Or ownerID if you prefer
                        
                        // CRITICAL: Replace the Prefab reference with the Live Instance
                        // Now logic knows about the real object
                        cell.OccupyingObject = entityScript; 
                    }
                }
            }
        }
    }
}