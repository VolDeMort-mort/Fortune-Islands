using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceManager : IManager
{
    // --- NEW: Helper Struct for Inspector ---
    [System.Serializable]
    public struct ResourceEntry
    {
        public ResourceType type;
        public int amount;
    }

    public event Action<ResourceType> OnResourceChanged;

    [Header("Setup")]
    public List<ResourceDefinition> allResources; 

    [Header("Configuration")]
    // --- NEW: Drag your starting resources here ---
    public List<ResourceEntry> startingResources; 

    private Dictionary<ResourceType, int> _inventory = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, ResourceDefinition> _definitions = new Dictionary<ResourceType, ResourceDefinition>();

    // Remove Awake() if you are spawning islands via GameStarter
    // relying on Initialize() is safer for execution order.
    // private void Awake() { InitializeResources(); } 

    public override void Initialize(IslandController controller)
    {
        base.Initialize(controller);
        InitializeResources();
    }

    private void InitializeResources()
    {
        // 1. Initialize everything to 0 first (safety)
        foreach (var def in allResources)
        {
            if (!_definitions.ContainsKey(def.type)) _definitions.Add(def.type, def);
            
            // Reset/Init to 0
            if (!_inventory.ContainsKey(def.type)) 
                _inventory.Add(def.type, 0);
            else 
                _inventory[def.type] = 0;
        }

        // 2. Apply the Starting Resources (Overwriting 0s)
        foreach (var entry in startingResources)
        {
            if (_inventory.ContainsKey(entry.type))
            {
                _inventory[entry.type] = entry.amount;
            }
            else
            {
                // Safety: Add it even if it wasn't in 'allResources'
                _inventory.Add(entry.type, entry.amount);
            }
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (!_inventory.ContainsKey(type)) _inventory[type] = 0;
        _inventory[type] += amount;
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
    
    // Check and Spend logic we added previously
    public bool TrySpendResources(List<ResourceCost> costs)
    {
        if (costs == null || costs.Count == 0) return true;

        foreach (var cost in costs)
        {
            if (GetAmount(cost.type) < cost.amount) return false;
        }

        foreach (var cost in costs)
        {
            AddResource(cost.type, -cost.amount);
        }
        return true;
    }
}