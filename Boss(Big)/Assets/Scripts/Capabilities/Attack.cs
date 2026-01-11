using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attack : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private float _activeTime = 0.12f;
    [SerializeField] private float _coolDown = 0.2f;
    [SerializeField] private int _damage = 50;

    private Controlls _controller;
    private HashSet<Collider2D> _hitTargets = new();
    private bool _isAttacking;

    private void Awake()
    {
        _controller = GetComponent<Controlls>();
        _collider.enabled = false;
    }

    private void Update()
    {
        if (_controller.input.RetrieveAttackInput() && !_isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _hitTargets.Clear();
        _collider.enabled = true;
        yield return new WaitForSeconds(_activeTime);
        _collider.enabled = false;
        yield return new WaitForSeconds(_coolDown);
        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (_hitTargets.Contains(other)) return;
        _hitTargets.Add(other);
        Debug.Log("Hit " + other.name);
        other.GetComponent<Health>()?.TakeDamage(_damage);
    }
}