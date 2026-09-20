using UnityEngine;

public class Surface : WorldEntity
{
    private Renderer _renderer;
    private Material[] _originalMaterials;
    private bool _isHighlighted = false;

    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _originalMaterials = _renderer.materials;
        }
    }

    public void ToggleHighlight(bool isActive, Material highlightMat)
    {
        if (_renderer == null) return;

        if (isActive)
        {
            // Apply Highlight Color
            _isHighlighted = true;
            Material[] newMats = new Material[_renderer.materials.Length];
            for (int i = 0; i < newMats.Length; i++) newMats[i] = highlightMat;
            
            _renderer.materials = newMats;
        }
        else if (_isHighlighted) 
        {
            // Reset to Normal
            _isHighlighted = false;
            _renderer.materials = _originalMaterials;
        }
    }
}