using System;
using UnityEngine;

public class BossTargetManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Boss _boss;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _healer;
    
    [Header("Target Switching Settings")]
    [SerializeField] private float _targetSwitchCooldown = 5f;
    [SerializeField] private float _minDistanceForProximityBonus = 5f;
    [SerializeField] private float _significantDistanceDifference = 3f;
    [SerializeField] private int _scoreThresholdForSwitch = 15; // NEW: Prevents tiny score differences from switching
    [SerializeField] private float _cooldownReductionOnPriority = 2f; // NEW: Reduce cooldown when high-priority event occurs
    
    [Header("Scoring Weights")]
    [SerializeField] private int _healingActiveWeight = 50;
    [SerializeField] private int _lowHealthDesperationWeight = 30;
    [SerializeField] private int _proximityWeight = 25;
    [SerializeField] private int _clearLineOfSightWeight = 15;
    [SerializeField] private int _distanceAdvantageWeight = 20;
    [SerializeField] private int _currentTargetBiasWeight = 20; // INCREASED: More sticky targeting
    
    [Header("Smoothing")]
    [SerializeField] private float _targetSwitchSmoothTime = 0.3f; // NEW: Smooth transition time
    [SerializeField] private bool _useAdaptiveCooldown = true; // NEW: Cooldown adjusts based on context
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;
    
    // State
    private Transform _currentTarget;
    private bool _playerIsBeingHealed;
    private float _lastTargetSwitchTime;
    private int _lastPlayerScore;
    private int _lastHealerScore;
    private float _currentCooldown; // NEW: Dynamic cooldown
    
    // Events
    public event Action<Transform> OnTargetChanged;
    
    public Transform CurrentTarget => _currentTarget;
    
    private void Awake()
    {
        if (_boss == null)
            _boss = GetComponent<Boss>();
            
        if (_player == null)
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            
        if (_healer == null)
            _healer = GameObject.FindGameObjectWithTag("Healer")?.transform;
        
        _currentTarget = _player; // Default to player
        _lastTargetSwitchTime = -_targetSwitchCooldown; // Allow immediate first switch
        _currentCooldown = _targetSwitchCooldown;
    }

    private void Start()
    {
        SubscribeToHealthEvents();
    }

    private void SubscribeToHealthEvents()
    {
        if (_player != null)
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnHealingStarted += OnPlayerHealingStarted;
                playerHealth.OnHealingEnded += OnPlayerHealingEnded;
            }
        }
    }

    public void UpdateTarget()
    {
        if (_healer == null)
        {
            SetTarget(_player);
            return;
        }

        // NEW: Use dynamic cooldown instead of fixed
        float activeCooldown = _useAdaptiveCooldown ? _currentCooldown : _targetSwitchCooldown;
        
        // Respect cooldown to prevent erratic switching
        if (Time.time - _lastTargetSwitchTime < activeCooldown)
            return;

        Transform newTarget = EvaluateBestTarget();
        
        if (newTarget != _currentTarget)
        {
            // NEW: Only switch if score difference is significant
            int scoreDifference = Mathf.Abs(_lastPlayerScore - _lastHealerScore);
            
            if (scoreDifference >= _scoreThresholdForSwitch)
            {
                SetTarget(newTarget);
            }
            else
            {
                LogDebug($"Score difference too small ({scoreDifference}), maintaining current target");
            }
        }
    }

    private Transform EvaluateBestTarget()
    {
        int playerScore = 0;
        int healerScore = 0;
        
        float playerDist = Vector2.Distance(_boss.transform.position, _player.position);
        float healerDist = Vector2.Distance(_boss.transform.position, _healer.position);

        // === CONDITION 1: Player is being healed (HIGHEST PRIORITY) ===
        if (_playerIsBeingHealed)
        {
            healerScore += _healingActiveWeight;
            LogDebug($"[HEALING ACTIVE] Healer +{_healingActiveWeight}");
            
            // NEW: Reduce cooldown for high-priority switches
            if (_useAdaptiveCooldown && _currentTarget != _healer)
            {
                _currentCooldown = Mathf.Max(1f, _targetSwitchCooldown - _cooldownReductionOnPriority);
            }
        }

        // === CONDITION 2: Boss health is low (Desperation) ===
        float healthPercent = (float)_boss._health.GetCurrentHealth() / _boss._health.GetMaxHealth();
        if (healthPercent < 0.4f)
        {
            // NEW: Scale weight based on how low health is
            int desperationBonus = Mathf.RoundToInt(_lowHealthDesperationWeight * (1f - healthPercent / 0.4f));
            healerScore += desperationBonus;
            LogDebug($"[LOW HEALTH] Healer +{desperationBonus} (HP: {healthPercent:P0})");
        }

        // === CONDITION 3: Proximity bonus (very close = easy target) ===
        // NEW: Gradual falloff instead of binary threshold
        if (healerDist < _minDistanceForProximityBonus)
        {
            int proximityBonus = Mathf.RoundToInt(_proximityWeight * (1f - healerDist / _minDistanceForProximityBonus));
            healerScore += proximityBonus;
            LogDebug($"[PROXIMITY] Healer +{proximityBonus} (dist: {healerDist:F1})");
        }
        
        if (playerDist < _minDistanceForProximityBonus)
        {
            int proximityBonus = Mathf.RoundToInt(_proximityWeight * (1f - playerDist / _minDistanceForProximityBonus));
            playerScore += proximityBonus;
            LogDebug($"[PROXIMITY] Player +{proximityBonus} (dist: {playerDist:F1})");
        }

        // === CONDITION 4: Distance advantage ===
        float distanceDiff = Mathf.Abs(healerDist - playerDist);
        if (distanceDiff > _significantDistanceDifference)
        {
            // NEW: Scale bonus based on how much closer
            int distanceBonus = Mathf.RoundToInt(_distanceAdvantageWeight * Mathf.Min(distanceDiff / 10f, 1f));
            
            if (healerDist < playerDist)
            {
                healerScore += distanceBonus;
                LogDebug($"[DISTANCE] Healer +{distanceBonus} (closer by {distanceDiff:F1})");
            }
            else
            {
                playerScore += distanceBonus;
                LogDebug($"[DISTANCE] Player +{distanceBonus} (closer by {distanceDiff:F1})");
            }
        }

        // === CONDITION 5: Clear line of sight ===
        if (HasClearLineOfSight(_healer, healerDist))
        {
            healerScore += _clearLineOfSightWeight;
            LogDebug($"[LINE OF SIGHT] Healer +{_clearLineOfSightWeight}");
        }
        
        if (HasClearLineOfSight(_player, playerDist))
        {
            playerScore += _clearLineOfSightWeight;
            LogDebug($"[LINE OF SIGHT] Player +{_clearLineOfSightWeight}");
        }

        // === CONDITION 6: Current target bias (prevent ping-ponging) ===
        // NEW: Bias increases the longer we've been targeting them
        float targetDuration = Time.time - _lastTargetSwitchTime;
        int biasBonus = Mathf.RoundToInt(_currentTargetBiasWeight * Mathf.Min(targetDuration / _targetSwitchCooldown, 2f));
        
        if (_currentTarget == _player)
        {
            playerScore += biasBonus;
            LogDebug($"[BIAS] Player +{biasBonus} (current target for {targetDuration:F1}s)");
        }
        else if (_currentTarget == _healer)
        {
            healerScore += biasBonus;
            LogDebug($"[BIAS] Healer +{biasBonus} (current target for {targetDuration:F1}s)");
        }

        // === FINAL EVALUATION ===
        _lastPlayerScore = playerScore;
        _lastHealerScore = healerScore;
        
        LogDebug($"=== FINAL SCORES === Player: {playerScore} | Healer: {healerScore} | Diff: {Mathf.Abs(playerScore - healerScore)}");
        
        return healerScore > playerScore ? _healer : _player;
    }

    private bool HasClearLineOfSight(Transform target, float distance)
    {
        Vector2 direction = (target.position - _boss.transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(_boss.transform.position, direction, distance, _boss._obstacleLayers);
        return hit.collider == null;
    }

    private void SetTarget(Transform newTarget)
    {
        _currentTarget = newTarget;
        _lastTargetSwitchTime = Time.time;
        
        // NEW: Reset cooldown to default after switch
        _currentCooldown = _targetSwitchCooldown;
        
        LogDebug($">>> TARGET SWITCHED TO: {_currentTarget.name} <<<");
        OnTargetChanged?.Invoke(_currentTarget);
    }

    // === HEALING EVENT HANDLERS ===
    private void OnPlayerHealingStarted()
    {
        _playerIsBeingHealed = true;
        LogDebug("!!! PLAYER HEALING STARTED - HEALER NOW HIGH PRIORITY !!!");
        
        // NEW: Force immediate re-evaluation on critical event
        if (_useAdaptiveCooldown)
        {
            _lastTargetSwitchTime = Time.time - _targetSwitchCooldown; // Allow immediate switch
        }
    }

    private void OnPlayerHealingEnded()
    {
        _playerIsBeingHealed = false;
        LogDebug("Player healing ended");
    }

    // === PUBLIC UTILITY METHODS ===
    public Vector2 GetDirectionToCurrentTarget()
    {
        return ((Vector2)_currentTarget.position - (Vector2)_boss.transform.position).normalized;
    }

    public float GetDistanceToCurrentTarget()
    {
        return Vector2.Distance(_boss.transform.position, _currentTarget.position);
    }

    public Vector2 GetVectorToCurrentTarget()
    {
        return (Vector2)(_currentTarget.position - _boss.transform.position);
    }

    public bool IsTargetingHealer()
    {
        return _currentTarget == _healer;
    }

    public bool IsTargetingPlayer()
    {
        return _currentTarget == _player;
    }

    // Force an immediate target switch (useful for special conditions)
    public void ForceTargetSwitch(Transform target)
    {
        if (target == _player || target == _healer)
        {
            SetTarget(target);
        }
    }

    // Reset cooldown to allow immediate re-evaluation
    public void ResetTargetCooldown()
    {
        _lastTargetSwitchTime = -_targetSwitchCooldown;
    }

    private void LogDebug(string message)
    {
        if (_showDebugLogs)
            Debug.Log($"[BossTargetManager] {message}");
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthEvents();
    }

    private void UnsubscribeFromHealthEvents()
    {
        if (_player != null)
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnHealingStarted -= OnPlayerHealingStarted;
                playerHealth.OnHealingEnded -= OnPlayerHealingEnded;
            }
        }
    }

    // === EDITOR HELPERS ===
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _currentTarget == null) return;

        // Draw line to current target
        Gizmos.color = _currentTarget == _healer ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, _currentTarget.position);
        
        // Draw sphere at target
        Gizmos.DrawWireSphere(_currentTarget.position, 0.5f);
        
        // NEW: Draw proximity radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _minDistanceForProximityBonus);
    }
}