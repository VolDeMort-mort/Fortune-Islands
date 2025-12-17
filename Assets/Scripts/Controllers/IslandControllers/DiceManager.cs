using UnityEngine;
using System;
using System.Collections.Generic;

public class DiceManager : IManager
{
    // Event: Sends the list of faces we rolled (for UI to display icons)
    public event Action<List<DiceFace>> OnDiceRolled;

    public void PerformRoll()
    {
        List<DiceFace> results = new List<DiceFace>();

        // 1. Find all dice providers in the world container
        // (Optimization: Cache this list if you have 100+ buildings)
        var providers = island.worldContainer.GetComponentsInChildren<DiceProvider>();

        Debug.Log($"Found dices{providers.Length}");
        // 2. Roll every single die from every building
        foreach (var provider in providers)
        {
            if (!provider.IsActive) continue;

            // The Dice Definition handles the random logic
            DiceFace result = provider.diceToProvide.Roll();
            results.Add(result);

            // 3. Apply Resource immediately (or wait for animation)
            if (result.amount > 0)
            {
                Debug.Log($"Add resources: {result.type} {result.amount}");
                island.resourceManager.AddResource(result.type, result.amount);
            }
        }

        Debug.Log($"Rolled {results.Count} dice.");
        
        // 4. Update UI
        OnDiceRolled?.Invoke(results);
    }
}