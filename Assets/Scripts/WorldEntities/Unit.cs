using UnityEngine;

using System;
using Unity.VisualScripting;

public abstract class Unit : WorldEntity
{
    [Header("Stats")]
    public int currentHealth = 100;
    public int maxHealth = 100;

    // Event for UI bars or death logic
    public event Action OnDeath;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Visual feedback (Flash red, float text) goes here
        Debug.Log($"{name} took {damage} dmg. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}