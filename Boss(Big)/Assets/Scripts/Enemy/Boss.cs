using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] public Transform visualRoot;
    public BossStateMachine _stateMachine;
    public Transform _player;
    public Rigidbody2D _rb;
    public CollisionDataRetriever _collisionDataRetriever;
    private Health _health;
    public FindTheBestPlatform _platformFinder;
    private int _maxHealth;
    private int _currentHealth;
    public Animator _animator;
    public LayerMask _forceLayer;
    public int _originalLayer;
    public bool _isFacingRight;
    
    private void Awake()
    {
        _collisionDataRetriever = GetComponent<CollisionDataRetriever>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _platformFinder = GetComponent<FindTheBestPlatform>();
        _rb = GetComponent<Rigidbody2D>();
        _originalLayer = gameObject.layer;
        _stateMachine = new BossStateMachine();
        _stateMachine.Initialize(new BossMove(this));
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
    }
    
    void Update()
    {
        _stateMachine.Update();
    }

    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
        Vector2 toTarget = (Vector2)(_player.position - transform.position);
        float yDistance = toTarget.y;
        float distance = toTarget.magnitude;
        _stateMachine.CurrentState.CheckPlayerDistance(distance, toTarget);
        _stateMachine.CurrentState.CheckPlayerDistanceY(yDistance);
        // Debug.Log($"Distance to player: {distance}");
        // Debug.Log($"YDistance to player: {yDistance}");
        
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
    }
    
    public void Flip()
    {
        _isFacingRight = !_isFacingRight;
        visualRoot.localRotation = _isFacingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180f, 0);
    }
}
