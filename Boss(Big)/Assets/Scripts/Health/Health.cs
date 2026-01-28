using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _health = 100;
    private Block _blockScript;

    public bool Dead { get; private set; }

    public event Action<int, int> OnHealthChanged; 
    public event Action OnDeath;

    private bool _canTakeDamage = true;

    private void Awake()
    {
        _health = Mathf.Clamp(_health, 0, _maxHealth);
        _blockScript = GetComponent<Block>();
    }

    public void TakeDamage(int damage)
    {
        if (Dead || !_canTakeDamage) return;
        
        
        if (_blockScript != null && _blockScript.IsBlocking)
        {
            int reducedDamage = Mathf.CeilToInt(damage * 0.5f); 
            damage = reducedDamage; 
            Debug.Log("Blocked! Took reduced damage: " + reducedDamage);
        }
        _health -= damage;
        _health = Mathf.Clamp(_health, 0, _maxHealth);
        Debug.Log(this.name + "is damaged");
        OnHealthChanged?.Invoke(_health, _maxHealth);

        if (_health == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (Dead) return;

        _health += amount;
        _health = Mathf.Clamp(_health, 0, _maxHealth);

        OnHealthChanged?.Invoke(_health, _maxHealth);
    }

    private void Die()
    {
        if (Dead) return;

        Dead = true;
        Debug.Log(this.gameObject + "is dead");
        OnDeath?.Invoke();
    }
    
    public void SetInvulnerable(bool value)
    {
        _canTakeDamage = !value;
    }
}