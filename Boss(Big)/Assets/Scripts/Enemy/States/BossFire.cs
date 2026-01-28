using UnityEngine;

public class BossFire : BossState
{
    private bool _hasFired;
    private bool _animationComplete;

    public BossFire(Boss boss) : base(boss) { }

    public override void Enter()
    {
        Debug.Log("Entering Fire State");
        _hasFired = false;
        _animationComplete = false;
        boss._animator.SetTrigger("Fire");
        Debug.Log(boss._animator.GetCurrentAnimatorStateInfo(0).IsName("Fire"));
    }

    public override void Update()
    {
        if (_animationComplete)
        {
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.1f));
        }
    }
    public void OnFireEvent()
    {
        if (!_hasFired)
        {
            FireProjectile();
            _hasFired = true;
        }
    }
    public void OnAnimationComplete()
    {
        _animationComplete = true;
    }
    
    private void FireProjectile()
    {
        if (boss._projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned!");
            return;
        }
        
        Transform firePoint = boss._firePoint != null ? boss._firePoint : boss.transform;
        
        Vector2 directionToPlayer = ((Vector2)boss._player.position - (Vector2)firePoint.position).normalized;
        float distanceToPlayer = Vector2.Distance(firePoint.position, boss._player.position);
        
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToPlayer, distanceToPlayer, boss._obstacleLayers);

        if (hit.collider != null)
        {
            Debug.Log($"Hit something other than player, ignoring: {hit.collider.gameObject.name}");
            return;
        }
        
        GameObject projectile = Object.Instantiate(boss._projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        
        if (projectileScript != null)
        {
            projectileScript.SetDirection(directionToPlayer);
        }
        
        Debug.Log($"Boss fired projectile in direction: {directionToPlayer}");
    }
}