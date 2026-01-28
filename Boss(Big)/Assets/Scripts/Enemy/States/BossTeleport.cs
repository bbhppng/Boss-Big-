using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTeleport : BossState
{
    private float _teleportCooldown = 3f;
    private float _lastTeleportTime;
    private bool _isTeleporting;
    private float _teleportDelay = 0.3f; 
    private Vector2 _teleportTarget;

    public BossTeleport(Boss boss) : base(boss)
    {
        _lastTeleportTime = -_teleportCooldown;
    }

    public override void Enter()
    {
        Debug.Log("Entering Teleport State");
        boss._animator.SetBool("isRunning", false);
        
        if (!boss.IsStateOnCooldown(typeof(BossTeleport), _teleportCooldown))
        {
            boss.SetStateCooldown(typeof(BossTeleport));
            AttemptTeleport();
        }
        else
        {
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.2f));
        }
    }

    public override void Update() { }

    public override void FixedUpdate()
    {
        if (_isTeleporting)
        {
            boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        boss._animator.SetBool("isTeleporting", false);
    }

    private void AttemptTeleport()
    {
        Vector2 teleportPosition = FindSmartTeleportPosition();
        
        if (teleportPosition != Vector2.zero)
        {
            _teleportTarget = teleportPosition;
            _isTeleporting = true;
            _lastTeleportTime = Time.time;
            boss.StartCoroutine(TeleportSequence());
        }
        else
        {
            boss._stateMachine.ChangeState(new BossIdle(boss, 0.2f));
        }
    }

    private IEnumerator TeleportSequence()
    {
        // Start Disappearing
       // boss._animator.SetBool("isTeleporting", true);
        // Optional: Instantiate(boss.teleportEffect, boss.transform.position, Quaternion.identity);
        
        yield return new WaitForSeconds(_teleportDelay);
        
        // Move to target
        boss.transform.position = _teleportTarget;
        
        // Face player immediately
        Vector2 directionToTarget = boss.GetCurrentTarget().position - boss.transform.position;
        if ((directionToTarget.x > 0 && !boss._isFacingRight) || (directionToTarget.x < 0 && boss._isFacingRight))
        {
            boss.Flip();
        }
        
        yield return new WaitForSeconds(_teleportDelay);
        
        _isTeleporting = false;
        boss._animator.SetBool("isTeleporting", false);
        
        // Transition to Idle to decide next move (Fire or Leap)
        boss._stateMachine.ChangeState(new BossIdle(boss, 0.1f));
    }

    private Vector2 FindSmartTeleportPosition()
    {
        Transform currentTarget = boss.GetCurrentTarget();
        // 1. Get the platform the player is currently on
        PlatformNode targetPlatform = boss._platformFinder.FindClosestPlatform(currentTarget.position);
        if (targetPlatform == null) return Vector2.zero;

        // 2. Get all platforms and find ones within a "good" range
        // We don't want to be exactly on top of the player, but close enough to attack
        Vector2 targetLandingPos = targetPlatform.GetLandingPoint();
        
        // Use your platform list to find a tactical spot
        // We can look for platforms that are near the player but not the player's platform
        PlatformNode bestTacticalPlatform = null;
        float bestDist = float.MaxValue;

        // We'll try to find a platform that is 4-7 units away from the player
        // This keeps the boss mobile and annoying to hit
        var allPlatforms = boss._platformFinder._platforms;
        foreach (var platform in allPlatforms)
        {
            if (platform == null) continue;
            
            Vector2 platPos = platform.GetLandingPoint();
            float distToTarget = Vector2.Distance(platPos, targetLandingPos);
            float distToBoss = Vector2.Distance(platPos, boss.transform.position);

            // Criteria: 
            // - Not too far from player (within 8 units)
            // - Not too close to current boss position (so we actually move)
            // - Not the exact same spot as the player (optional, depends on boss type)
            if (distToTarget < 8f && distToBoss > 3f)
            {
                if (distToTarget < bestDist)
                {
                    bestDist = distToTarget;
                    bestTacticalPlatform = platform;
                }
            }
        }

        // Fallback: If no tactical platform found, just go to the player's platform
        if (bestTacticalPlatform != null)
        {
            return bestTacticalPlatform.GetLandingPoint();
        }
        
        return targetPlatform.GetLandingPoint();
    }

    public override void CheckPlayerDistance(float distance, Vector2 direction) { }
    public override void CheckPlayerDistanceY(float yDistance) { }
}