using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Target Management")]
    public BossTargetManager _targetManager;
    
    [SerializeField] public Transform visualRoot;
    public GameObject _projectilePrefab;
    public Transform _firePoint;
    public BossStateMachine _stateMachine;
    public Rigidbody2D _rb;
    public PolygonCollider2D _attackBox;
    public CollisionDataRetriever _collisionDataRetriever;
    public Health _health;
    public FindTheBestPlatform _platformFinder;
    private int _maxHealth;
    private int _currentHealth;
    public Animator _animator;
    public LayerMask _forceLayer;
    public int _originalLayer;
    public bool _isFacingRight;
    public LayerMask _obstacleLayers;
    private Dictionary<System.Type, float> _stateCooldowns = new Dictionary<System.Type, float>();
    
    [Header("Combat Tracking")]
    [SerializeField] private int _consecutiveParries = 0; // Track consecutive parries
    [SerializeField] private float _lastParryTime = -999f; // Time of last parry
    [SerializeField] private float _parryResetTime = 3f; // Reset counter after 3 seconds
    [SerializeField] private bool _showParryDebug = true; // Debug parry tracking
    
    private void Awake()
    {
        _collisionDataRetriever = GetComponent<CollisionDataRetriever>();
        _targetManager = GetComponent<BossTargetManager>();
        _platformFinder = GetComponent<FindTheBestPlatform>();
        _rb = GetComponent<Rigidbody2D>();
        _originalLayer = gameObject.layer;
        _stateMachine = new BossStateMachine();
        _stateMachine.Initialize(new BossIdle(this));
        _isFacingRight = visualRoot.localRotation.eulerAngles.y == 0;
    }
    
    void Start()
    {
        _health = GetComponent<Health>();
        
        if (_health != null)
        {
            _health.OnHealthChanged += OnBossHealthChanged;
            _health.OnDeath += OnBossDeath;
        }
        
        // Subscribe to player parry events
        SubscribeToParryEvents();
    }
    
    void Update()
    {
        _stateMachine.Update();
        
        // Reset parry counter if too much time has passed
        if (Time.time - _lastParryTime > _parryResetTime && _consecutiveParries > 0)
        {
            if (_showParryDebug)
                Debug.Log($"[Boss] Parry counter reset due to timeout ({_parryResetTime}s passed)");
            _consecutiveParries = 0;
        }
    }

    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
        _targetManager.UpdateTarget();
        Vector2 toTarget = _targetManager.GetVectorToCurrentTarget();
        float yDistance = toTarget.y;
        float distance = _targetManager.GetDistanceToCurrentTarget();
        _stateMachine.CurrentState.CheckPlayerDistance(distance, toTarget);
        _stateMachine.CurrentState.CheckPlayerDistanceY(yDistance);
    }
    
    public Transform GetCurrentTarget()
    {
        return _targetManager.CurrentTarget;
    }
    
    private void OnBossHealthChanged(int current, int max)
    {
        _currentHealth = current;
        _maxHealth = max;
        Debug.Log($"Boss Health: {_currentHealth}/{_maxHealth}");
    }
    
    private void OnBossDeath()
    {
        Debug.Log("Boss has died!");
    }
    
    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= OnBossHealthChanged;
            _health.OnDeath -= OnBossDeath;
        }
        
        UnsubscribeFromParryEvents();
    }
    
    public void Flip()
    {
        _isFacingRight = !_isFacingRight;
        visualRoot.localRotation = _isFacingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180f, 0);
    }
    
    public bool IsStateOnCooldown(System.Type stateType, float cooldownDuration)
    {
        if (_stateCooldowns.TryGetValue(stateType, out float lastUsedTime))
        {
            return Time.time - lastUsedTime < cooldownDuration;
        }
        return false;
    }
    
    public void SetStateCooldown(System.Type stateType)
    {
        _stateCooldowns[stateType] = Time.time;
    }
    
    public float GetTimeSinceState(System.Type stateType)
    {
        if (_stateCooldowns.TryGetValue(stateType, out float lastUsedTime))
        {
            return Time.time - lastUsedTime;
        }
        return float.MaxValue;
    }
    
    // === PARRY TRACKING SYSTEM ===
    private void SubscribeToParryEvents()
    {
        // Find player's Block component and subscribe to parry events
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Block playerBlock = player.GetComponent<Block>();
            if (playerBlock != null)
            {
                playerBlock.OnParrySuccess += OnPlayerParriedProjectile;
                Debug.Log("[Boss] Subscribed to player parry events");
            }
            else
            {
                Debug.LogWarning("[Boss] Player found but no Block component!");
            }
        }
        else
        {
            Debug.LogWarning("[Boss] No player found with 'Player' tag!");
        }
    }
    
    private void UnsubscribeFromParryEvents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Block playerBlock = player.GetComponent<Block>();
            if (playerBlock != null)
            {
                playerBlock.OnParrySuccess -= OnPlayerParriedProjectile;
            }
        }
    }
    
    private void OnPlayerParriedProjectile()
    {
        // Only count parries during Fire state
        if (_stateMachine.CurrentState is BossFire)
        {
            _consecutiveParries++;
            _lastParryTime = Time.time;
            
            if (_showParryDebug)
            {
                Debug.Log($"[Boss] Player parried! Consecutive parries: {_consecutiveParries}/2");
                
                if (_consecutiveParries >= 2)
                {
                    Debug.Log("[Boss] ⚠️ RETREAT THRESHOLD REACHED! ⚠️");
                }
            }
        }
        else
        {
            if (_showParryDebug)
                Debug.Log($"[Boss] Parry detected but not in Fire state (current: {_stateMachine.CurrentState.GetType().Name})");
        }
    }
    
    public int GetConsecutiveParries()
    {
        return _consecutiveParries;
    }
    
    public void ResetConsecutiveParries()
    {
        if (_showParryDebug && _consecutiveParries > 0)
            Debug.Log($"[Boss] Consecutive parries reset (was {_consecutiveParries})");
        _consecutiveParries = 0;
    }
    
    // === DEBUG GUI ===
    private void OnGUI()
    {
        if (!_showParryDebug) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = _consecutiveParries >= 2 ? Color.red : Color.yellow;
        
        GUI.Label(new Rect(Screen.width - 300, 10, 300, 30), 
            $"Boss Parries Tracked: {_consecutiveParries}/2", style);
        
        if (_consecutiveParries > 0)
        {
            float timeRemaining = _parryResetTime - (Time.time - _lastParryTime);
            GUI.Label(new Rect(Screen.width - 300, 40, 300, 30), 
                $"Reset in: {timeRemaining:F1}s", style);
        }
        
        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(Screen.width - 300, 70, 300, 30), 
            $"Current State: {_stateMachine.CurrentState.GetType().Name}", style);
    }
}