using UnityEngine;
using System.Collections;

public class BossRetreat : BossState
{
    [Header("Retreat Settings")]
    private float _safeDistance = 8f; // Desired distance from target
    private bool _isTeleporting;
    private Vector2 _teleportTarget;
    private float _teleportDelay = 0.3f;

    public BossRetreat(Boss boss) : base(boss) { }

    public override void Enter()
    {
        Debug.Log("Entering Retreat State - Boss is teleporting to safe platform!");
        _isTeleporting = false;
        
        // Stop any movement
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        
        // Find a safe platform and teleport there
        Vector2 safePlatform = FindSafePlatform();
        
        if (safePlatform != Vector2.zero)
        {
            _teleportTarget = safePlatform;
            boss.StartCoroutine(TeleportToSafePlatform());
        }
        else
        {
            // Fallback: No safe platform found, go to idle
            Debug.LogWarning("No safe platform found for retreat!");
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.5f));
        }
    }

    public override void Update()
    {
        // Teleport sequence is handled by coroutine
    }

    public override void FixedUpdate()
    {
        // Keep boss stationary during teleport
        if (_isTeleporting)
        {
            boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        boss._animator.SetBool("isTeleporting", false);
        
        Debug.Log("Retreat ended, ready to fire from safe distance");
    }

    private IEnumerator TeleportToSafePlatform()
    {
        _isTeleporting = true;
        
        // Start teleport animation
        boss._animator.SetBool("isTeleporting", true);
        // Optional: Instantiate(boss.teleportEffect, boss.transform.position, Quaternion.identity);
        
        yield return new WaitForSeconds(_teleportDelay);
        
        // Teleport to safe platform
        boss.transform.position = _teleportTarget;
        
        // Face the player
        Transform currentTarget = boss.GetCurrentTarget();
        Vector2 directionToTarget = currentTarget.position - boss.transform.position;
        if ((directionToTarget.x > 0 && !boss._isFacingRight) || (directionToTarget.x < 0 && boss._isFacingRight))
        {
            boss.Flip();
        }
        
        yield return new WaitForSeconds(_teleportDelay);
        
        _isTeleporting = false;
        boss._animator.SetBool("isTeleporting", false);
        
        // Transition to Fire state from safe distance
        Debug.Log("Teleport complete, initiating Fire state");
        boss._stateMachine.ChangeState(new BossFire(boss));
    }

    private Vector2 FindSafePlatform()
    {
        Transform currentTarget = boss.GetCurrentTarget();
        Vector2 targetPos = currentTarget.position;
        
        // Get all platforms
        var allPlatforms = boss._platformFinder._platforms;
        
        PlatformNode bestPlatform = null;
        float bestScore = float.MinValue;
        
        foreach (var platform in allPlatforms)
        {
            if (platform == null) continue;
            
            Vector2 platPos = platform.GetLandingPoint();
            float distToTarget = Vector2.Distance(platPos, targetPos);
            float distToBoss = Vector2.Distance(platPos, boss.transform.position);
            
            // Criteria for a good retreat platform:
            // 1. Within safe distance range (6-10 units from target)
            // 2. Different from current position (at least 3 units away)
            // 3. Prefer platforms that are at safe distance
            
            if (distToTarget >= _safeDistance * 0.75f && distToTarget <= _safeDistance * 1.5f && distToBoss > 3f)
            {
                // Score based on how close to ideal safe distance
                float distanceScore = 100f - Mathf.Abs(distToTarget - _safeDistance);
                
                // Bonus for being further from boss (actually retreating)
                float retreatScore = distToBoss * 10f;
                
                float totalScore = distanceScore + retreatScore;
                
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestPlatform = platform;
                }
            }
        }
        
        // Fallback: If no ideal platform, find any platform that's further than safe distance
        if (bestPlatform == null)
        {
            foreach (var platform in allPlatforms)
            {
                if (platform == null) continue;
                
                Vector2 platPos = platform.GetLandingPoint();
                float distToTarget = Vector2.Distance(platPos, targetPos);
                float distToBoss = Vector2.Distance(platPos, boss.transform.position);
                
                if (distToTarget > _safeDistance * 0.6f && distToBoss > 2f)
                {
                    float score = distToTarget + (distToBoss * 5f);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPlatform = platform;
                    }
                }
            }
        }
        
        if (bestPlatform != null)
        {
            Debug.Log($"Found safe platform at distance {Vector2.Distance(bestPlatform.GetLandingPoint(), targetPos):F1} from target");
            return bestPlatform.GetLandingPoint();
        }
        
        return Vector2.zero;
    }

    public override void CheckPlayerDistance(float distance, Vector2 direction) { }
    public override void CheckPlayerDistanceY(float yDistance) { }
}