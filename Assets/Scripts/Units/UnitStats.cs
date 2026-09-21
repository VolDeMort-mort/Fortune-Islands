using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Game/Unit Stats")]
public class UnitStats : ScriptableObject
{
    [Header("General")]
    public string unitName = "Villager";
    public GameObject unitPrefab; // The mesh/visuals

    [Header("Movement")]
    public float moveSpeed = 3f;
    
    [Header("Combat")]
    public int maxHealth = 100;
    public float attackRange = 1.5f; // Short vs Long Range
    public int attackDamage = 10;
    public float attackSpeed = 1.0f; // Seconds between attacks
}