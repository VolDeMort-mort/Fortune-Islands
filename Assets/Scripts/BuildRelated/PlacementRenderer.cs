using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlacementRenderer
{
    // Settings
    private Material _validMat;
    private Material _invalidMat;
    private LayerMask _groundLayer;

    // State
    private ConstructionGhost _ghost;
    private List<Surface> _highlightedSurfaces = new List<Surface>();

    public PlacementRenderer(Material valid, Material invalid, LayerMask groundLayer)
    {
        _validMat = valid;
        _invalidMat = invalid;
        _groundLayer = groundLayer;
    }

    // --- Ghost Logic ---
    public void SpawnGhost(GameObject prefab)
    {
        ClearGhost();
        _ghost = new ConstructionGhost(prefab, _validMat, _invalidMat);
    }

    public void UpdateGhost(Vector3 position, float rotation, bool isValid)
    {
        if (_ghost == null) return;
        _ghost.Move(position);
        _ghost.Rotate90();
        _ghost.SetState(isValid);
    }

    public void ClearGhost()
    {
        if (_ghost != null) _ghost.Destroy();
        _ghost = null;
    }
    
    public float GetRotation() => _ghost != null ? _ghost.Rotation : 0f;
    public void RotateGhost() => _ghost?.Rotate90();
    public GameObject GetGhostObject() => _ghost?.GameObject;

    // --- Highlighting Logic (Moved from Manager) ---
    public void UpdateHighlights(int pivotX, int pivotZ, Structure script, float rotation, bool isValid)
    {
        // 1. Reset old highlights
        ClearHighlights();

        // 2. Calculate new footprint
        List<FootprintTile> shape = script.GetRotatedFootprint(rotation);
        Material matToUse = isValid ? _validMat : _invalidMat;

        // 3. Raycast to find and color surfaces
        foreach (var tile in shape)
        {
            int targetX = pivotX + tile.offset.x;
            int targetZ = pivotZ + tile.offset.y;

            Vector3 tileCenter = new Vector3(targetX, 5f, targetZ);
            
            if (Physics.Raycast(tileCenter, Vector3.down, out RaycastHit hit, 20f, _groundLayer))
            {
                Surface surface = hit.collider.GetComponent<Surface>();
                if (surface != null)
                {
                    surface.ToggleHighlight(true, matToUse);
                    _highlightedSurfaces.Add(surface);
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var surface in _highlightedSurfaces)
        {
            if (surface != null) surface.ToggleHighlight(false, null);
        }
        _highlightedSurfaces.Clear();
    }
}