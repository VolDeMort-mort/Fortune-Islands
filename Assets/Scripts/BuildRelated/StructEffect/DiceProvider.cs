using UnityEngine;
using System.Collections.Generic;

public class DiceProvider : MonoBehaviour, IBuildingFeature
{
    [Header("Dice Configuration")]
    // Drag your ScriptableObject here (e.g. "House Die")
    public Dice diceToProvide; 

    private bool _isActive = false;

    public void Initialize(IslandController island)
    {
        _isActive = true;
        // Register self if needed, or DiceManager can find me
    }

    public bool IsActive => _isActive;
}