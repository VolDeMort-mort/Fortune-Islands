using UnityEngine;


public abstract class Structure : MonoBehaviour, ISelectable
{
    [Header("Base Stats")]
    public string structureName;
    public int maxHealth = 100;
    protected int _currentHealth;

    public virtual void Start()
    {
        _currentHealth = maxHealth;
    }

    public virtual void OnSelect()
    {
        Debug.Log($"Selected {structureName}. HP: {_currentHealth}");
    }

    public virtual void OnDeselect()
    {
        // Hide UI, etc.
    }
    
    // Optional: Shared helper
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0) DestroyStructure();
    }

    protected virtual void DestroyStructure()
    {
        // Destroy(gameObject);
    }
}