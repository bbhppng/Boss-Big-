using UnityEngine;
using System.Collections.Generic;

public class Attack : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private float _coolDown = 0.2f;
    [SerializeField] private int _damage = 50;

    private Controlls _controller;
    private AniamtionManager _animController;
    private HashSet<Collider2D> _hitTargets = new();
    private bool _isAttacking;
    private float _nextAttackTime;
    
    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        _controller = GetComponent<Controlls>();
        _animController = GetComponent<AniamtionManager>();
        _collider.enabled = false;
    }

    private void Update()
    {
        if (_controller.input.RetrieveAttackInput() && Time.time >= _nextAttackTime && !_isAttacking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        _isAttacking = true; // Set this immediately when starting attack
        _nextAttackTime = Time.time + _coolDown;
        _hitTargets.Clear();
        
        if (_animController != null)
        {
            _animController.TriggerAttack();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (_hitTargets.Contains(other)) return;
        _hitTargets.Add(other);
        Debug.Log("Hit " + other.name);
        other.GetComponent<Health>()?.TakeDamage(_damage);
    }
    
    public void EnableHitbox()
    {
        _collider.enabled = true;
    }
    
    public void DisableHitbox()
    {
        _collider.enabled = false;
    }
    
    public void OnAttackEnd()
    {
        _isAttacking = false;
        _collider.enabled = false;
        Debug.Log("Attack ended"); 
    }
}