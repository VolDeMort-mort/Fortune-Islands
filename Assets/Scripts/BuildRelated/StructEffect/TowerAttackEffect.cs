using UnityEngine;

public class TowerAttackEffect : MonoBehaviour, IBuildingFeature
{
    [Header("Combat Stats")]
    public float range = 10f;
    public int damage = 20;
    public float attackSpeed = 1.5f;
    public GameObject projectilePrefab; // Optional

    private IslandController _island;
    private float _attackCooldown = 0f;

    public void Initialize(IslandController island)
    {
        _island = island;
    }

    void Update()
    {
        // Simple cooldown logic
        if (_attackCooldown > 0)
        {
            _attackCooldown -= Time.deltaTime;
            return;
        }

        // 1. Find Target (Very simple version)
        // In a real game, ask a "UnitManager" for the closest enemy to save performance
        WorldEntity target = FindClosestEnemy();

        if (target != null)
        {
            Attack(target);
            _attackCooldown = attackSpeed;
        }
    }

    void Attack(WorldEntity target)
    {
        Debug.Log($"Tower shooting at {target.name}!");
        // target.TakeDamage(damage); 
    }

    WorldEntity FindClosestEnemy()
    {
        // Placeholder logic: You should implement a proper "EnemyManager" later
        return null; 
    }
}