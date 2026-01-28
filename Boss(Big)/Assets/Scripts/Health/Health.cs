using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _health = 100;
    private Block _blockScript;

    public bool Dead { get; private set; }

    public event Action<int, int> OnHealthChanged; 
    public event Action OnDeath;
    
    public event Action OnHealingStarted;
    public event Action OnHealingEnded;

    private bool _canTakeDamage = true;
    
    private Coroutine _healCoroutine;

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
    
    public void HealGradually(int totalAmount, float duration)
    {
        if (Dead) return;
        
        if (_healCoroutine != null)
        {
            StopCoroutine(_healCoroutine);
        }

        _healCoroutine = StartCoroutine(HealOverTime(totalAmount, duration));
    }

    private IEnumerator HealOverTime(int totalAmount, float duration)
    {
        OnHealingStarted?.Invoke();
        
        float elapsed = 0f;
        int startHealth = _health;
        int targetHealth = Mathf.Min(_health + totalAmount, _maxHealth);

        while (elapsed < duration && !Dead)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            int newHealth = Mathf.RoundToInt(Mathf.Lerp(startHealth, targetHealth, progress));

            if (newHealth != _health)
            {
                _health = newHealth;
                OnHealthChanged?.Invoke(_health, _maxHealth);
            }

            yield return null;
        }
        _health = targetHealth;
        OnHealthChanged?.Invoke(_health, _maxHealth);
        OnHealingEnded?.Invoke();
        _healCoroutine = null;
    }
    
    public int GetCurrentHealth()
    {
        return _health;
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
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