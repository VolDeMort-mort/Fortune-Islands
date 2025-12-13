using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    // --- The Event ---
    // We send the Type so the UI knows WHICH resource changed
    public event Action<ResourceType> OnResourceChanged;

    [Header("Setup")]
    public List<ResourceDefinition> allResources; 

    // The Dynamic Data
    private Dictionary<ResourceType, int> _inventory = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, ResourceDefinition> _definitions = new Dictionary<ResourceType, ResourceDefinition>();

    private void Awake()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        foreach (var def in allResources)
        {
            if (!_definitions.ContainsKey(def.type)) _definitions.Add(def.type, def);
            if (!_inventory.ContainsKey(def.type)) _inventory.Add(def.type, 0);
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (!_inventory.ContainsKey(type)) _inventory[type] = 0;

        _inventory[type] += amount;

        // Trigger the event on THIS specific instance
        OnResourceChanged?.Invoke(type);
    }

    public int GetAmount(ResourceType type)
    {
        return _inventory.ContainsKey(type) ? _inventory[type] : 0;
    }

    public Sprite GetIcon(ResourceType type)
    {
        return _definitions.ContainsKey(type) ? _definitions[type].icon : null;
    }
}