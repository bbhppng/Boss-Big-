using UnityEngine;

public class BossIdle : BossState
{
    private float _idleDuration;
    private float _idleTimer;
    
    [Header("Attack Cooldowns")]
    private float _fireCooldown = 4f; // Increased from 2f to reduce spam
    private float _teleportCooldown = 4f; // Increased from 3f
    
    [Header("Attack Ranges")]
    private float _fireRange = 7f; // Increased from 4f - less aggressive
    private float _preferredFireRange = 5f; // Optimal range for firing

    public BossIdle(Boss boss, float duration = 0.2f) : base(boss)
    {
        _idleDuration = duration;
    }

    public override void Enter()
    {
        Debug.Log("Entering Idle State");
        _idleTimer = 0f;
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        boss._animator.SetBool("isRunning", false);
    }

    public override void Update()
    {
        _idleTimer += Time.deltaTime;

        if (_idleTimer >= _idleDuration)
        {
            Vector2 toTarget = boss._targetManager.GetVectorToCurrentTarget();
            float distance = toTarget.magnitude;

            // Decision tree for next action
            bool canFire = !boss.IsStateOnCooldown(typeof(BossFire), _fireCooldown);
            bool canTeleport = !boss.IsStateOnCooldown(typeof(BossTeleport), _teleportCooldown);
            
            // Get time since last fire for smarter decisions
            float timeSinceFire = boss.GetTimeSinceState(typeof(BossFire));
            
            // Priority 1: Fire if in good range and not on cooldown
            if (distance <= _fireRange && canFire && HasLineOfSight())
            {
                if (distance <= _preferredFireRange || timeSinceFire > _fireCooldown * 1.5f)
                {
                    Debug.Log($"Firing (LOS confirmed, distance {distance:F1})");
                    boss._stateMachine.ChangeState(new BossFire(boss));
                    return;
                }
            }
            
            // Priority 2: Teleport if Fire is on cooldown or out of range
            if (canTeleport)
            {
                // Teleport if too far OR if fire is on cooldown and we want to reposition
                if (distance > _fireRange || (!canFire && distance > _preferredFireRange))
                {
                    Debug.Log($"Initiating Teleport (distance: {distance:F1}, canFire: {canFire})");
                    boss._stateMachine.ChangeState(new BossTeleport(boss));
                    return;
                }
            }
            
            // Priority 3: Wait a bit longer if both on cooldown
            _idleTimer = _idleDuration - 0.5f; // Wait another 0.5s
            
            // Debug info
            if (!canFire && !canTeleport)
            {
                float fireCD = _fireCooldown - timeSinceFire;
                float teleportCD = _teleportCooldown - boss.GetTimeSinceState(typeof(BossTeleport));
                Debug.Log($"Both attacks on cooldown. Fire: {fireCD:F1}s, Teleport: {teleportCD:F1}s");
            }
        }
    }
    
    private bool HasLineOfSight()
    {
        Transform target = boss.GetCurrentTarget();
        Transform firePoint = boss._firePoint != null ? boss._firePoint : boss.transform;

        Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, distance, boss._obstacleLayers);

        return hit.collider == null; // true = can see player, false = blocked
    }
}