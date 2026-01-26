using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class BossAttack : BossState
{
    // private float _coolDown = 2f;
    // private int _damage = 20;
    //
    // private HashSet<PolygonCollider2D> _hitTargets = new();
    // private bool _isAttacking;
    public BossAttack(Boss boss) : base(boss) { }

    // public override void Enter()
    // {
    //     Debug.Log("Entering Attack State");
    //     Attack();
    // }
    //
    // private void Attack()
    // {
    //     boss._animator.SetBool("isAttacking", true);
    // }
    //
    // private void OnTriggerEnter2D(PolygonCollider2D other)
    // {
    //     if (!other.CompareTag("Player")) return;
    //     if (_hitTargets.Contains(other)) return;
    //     _hitTargets.Add(other);
    //     Debug.Log("Hit " + other.name);
    //     other.GetComponent<Health>()?.TakeDamage(_damage);
    // }
}
