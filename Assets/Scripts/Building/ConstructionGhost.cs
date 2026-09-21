using UnityEngine;

public class ConstructionGhost
{
    private GameObject _ghostObj;
    private Renderer[] _renderers;
    private Material _validMat;
    private Material _invalidMat;
    private float _currentYRotation;

    public float Rotation => _currentYRotation;
    public GameObject GameObject => _ghostObj;

    public ConstructionGhost(GameObject prefab, Material valid, Material invalid)
    {
        _validMat = valid;
        _invalidMat = invalid;

        _ghostObj = Object.Instantiate(prefab);
        
        // Disable colliders so we don't click ourselves
        foreach (var col in _ghostObj.GetComponentsInChildren<Collider>()) 
            Object.Destroy(col);

        _renderers = _ghostObj.GetComponentsInChildren<Renderer>();
    }

    public void Move(Vector3 position)
    {
        _ghostObj.transform.position = position;
    }

    public void Rotate90()
    {
        _currentYRotation = (_currentYRotation + 90f) % 360f;
        _ghostObj.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
    }

    public void SetRotation(float rotation)
    {
        _currentYRotation = rotation;
        _ghostObj.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);

    }

    public void SetState(bool isValid)
    {
        Material target = isValid ? _validMat : _invalidMat;
        foreach (var r in _renderers) r.material = target;
    }

    public void Destroy()
    {
        if (_ghostObj != null) Object.Destroy(_ghostObj);
    }
}