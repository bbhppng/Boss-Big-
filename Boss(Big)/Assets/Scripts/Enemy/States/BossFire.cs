using UnityEngine;

public class BossFire : BossState
{
    [Header("Firing Limits")]
    private int _maxProjectilesPerBurst = 5;
    private int _projectilesFired = 0;
    private float _timeBetweenShots = 0.75f; // Delay between shots in burst
    private float _lastShotTime;
    
    [Header("State Tracking")]
    private bool _animationComplete;
    private bool _shouldRetreat; // Flag for tactical retreat
    private float _healthAtEnter; // Track damage taken during firing
    
    public BossFire(Boss boss) : base(boss) { }

    public override void Enter()
    {
        Debug.Log("Entering Fire State");
        _projectilesFired = 0;
        _animationComplete = false;
        _shouldRetreat = false;
        _lastShotTime = Time.time - _timeBetweenShots; // Allow immediate first shot
        
        // Track health to detect damage during firing
        _healthAtEnter = boss._health.GetCurrentHealth();
        
        boss._animator.SetTrigger("Fire");
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y); // Stop movement while firing
    }

    public override void Update()
    {
        if (!HasLineOfSightToTarget())
        {
            Debug.Log("LOS lost during firing, aborting Fire state");
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.4f));
            return;
        }
        // Check for retreat conditions
        CheckRetreatConditions();
    
        if (_shouldRetreat)
        {
            Debug.Log("Boss retreating due to damage/parries!");
            boss._stateMachine.ChangeState(new BossRetreat(boss));
            return;
        }
    
        // Check if we've fired max projectiles
        if (_projectilesFired >= _maxProjectilesPerBurst)
        {
            Debug.Log($"Max projectiles fired ({_maxProjectilesPerBurst}), ending Fire state");
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.5f)); // Longer cooldown after burst
            return;
        }
    
        // Fire projectiles in burst
        if (Time.time - _lastShotTime >= _timeBetweenShots)
        {
            OnFireEvent();
        
            // Retrigger animation to keep firing pose active
            if (_projectilesFired < _maxProjectilesPerBurst)
            {
                boss._animator.SetTrigger("Fire");
            }
        }
    }
    
    public override void Exit()
    {
        // Set cooldown when exiting fire state
        boss.SetStateCooldown(typeof(BossFire));
    }
    
    private void CheckRetreatConditions()
    {
        // CONDITION 1: Took significant damage while firing
        float currentHealth = boss._health.GetCurrentHealth();
        float damageTaken = _healthAtEnter - currentHealth;
        float damageThreshold = boss._health.GetMaxHealth() * 0.15f; // 15% of max health
        
        if (damageTaken >= damageThreshold)
        {
            Debug.Log($"Boss took {damageTaken} damage while firing! Retreating...");
            _shouldRetreat = true;
            return;
        }
        
        // CONDITION 2: Player parried 2+ projectiles in a row
        if (boss.GetConsecutiveParries() >= 2)
        {
            Debug.Log("Player parried 2 projectiles in a row! Boss retreating...");
            _shouldRetreat = true;
            boss.ResetConsecutiveParries(); // Reset counter
            return;
        }
    }
    
    public void OnFireEvent()
    {
        // Check if enough time has passed since last shot
        if (Time.time - _lastShotTime < _timeBetweenShots)
        {
            Debug.Log($"Too soon to fire again. Time since last: {Time.time - _lastShotTime:F2}s");
            return;
        }
            
        // Check if we can still fire
        if (_projectilesFired >= _maxProjectilesPerBurst)
        {
            Debug.Log("Already fired max projectiles");
            return;
        }
        
        FireProjectile();
        _projectilesFired++;
        _lastShotTime = Time.time;
        
        Debug.Log($"Fired projectile {_projectilesFired}/{_maxProjectilesPerBurst}");
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
        Transform currentTarget = boss.GetCurrentTarget();
    
        Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)firePoint.position).normalized;
        float distanceToTarget = Vector2.Distance(firePoint.position, currentTarget.position);
    
        // Check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToTarget, distanceToTarget, boss._obstacleLayers);

        if (hit.collider != null)
        {
            Debug.Log($"Obstacle blocking shot: {hit.collider.gameObject.name}");
            return;
        }
        
        // Spawn projectile
        GameObject projectileObj = Object.Instantiate(boss._projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectileScript = projectileObj.GetComponent<Projectile>();
        
        if (projectileScript != null)
        {
            projectileScript.SetDirection(directionToTarget);
            // Store reference to boss in projectile for tracking
            BossProjectileTracker tracker = projectileObj.AddComponent<BossProjectileTracker>();
            tracker.SetBoss(boss);
        }
        
        Debug.Log($"Boss fired projectile #{_projectilesFired} at {currentTarget.name}");
    }
    
    private bool HasLineOfSightToTarget()
    {
        Transform target = boss.GetCurrentTarget();
        Transform firePoint = boss._firePoint != null ? boss._firePoint : boss.transform;

        Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, distance, boss._obstacleLayers);

        return hit.collider == null;
    }
}

// Helper component to track which boss fired the projectile
public class BossProjectileTracker : MonoBehaviour
{
    private Boss _boss;
    
    public void SetBoss(Boss boss)
    {
        _boss = boss;
    }
    
    public Boss GetBoss()
    {
        return _boss;
    }
}