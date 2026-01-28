using UnityEngine;

public class AniamtionManager : MonoBehaviour
{
    private Animator _animator;
    private Move _move;
    private Jump _jump;
    private Force _force;
    private Block _block;
    private Attack _attack;
    private Rigidbody2D _rb;
    
    // Animation parameter hashes
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int YVelocity = Animator.StringToHash("yVelocity");
    private static readonly int Attack = Animator.StringToHash("attack");
    private static readonly int IsBlocking = Animator.StringToHash("isBlocking");
    private static readonly int Parry = Animator.StringToHash("parry");
    private static readonly int IsPulling = Animator.StringToHash("isPulling");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    
    [Header("Animation Settings")]
    [SerializeField] private float _runThreshold = 0.1f;
    [SerializeField] private bool _lockMovementDuringAttack = true;
    
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _move = GetComponent<Move>();
        _jump = GetComponent<Jump>();
        _force = GetComponent<Force>();
        _block = GetComponent<Block>();
        _attack = GetComponent<Attack>();
        _rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        if (_block != null)
        {
            _block.OnBlockStart += HandleBlockStart;
            _block.OnBlockEnd += HandleBlockEnd;
            _block.OnParrySuccess += HandleParrySuccess;
        }
    }
    
    private void OnDisable()
    {
        if (_block != null)
        {
            _block.OnBlockStart -= HandleBlockStart;
            _block.OnBlockEnd -= HandleBlockEnd;
            _block.OnParrySuccess -= HandleParrySuccess;
        }
    }
    
    private void Update()
    {
        UpdateMovementAnimations();
        // REMOVED UpdateAttackState() - let the Attack script handle this directly
    }
    
    private void UpdateMovementAnimations()
    {
        // Grounded state and Velocity should ALWAYS update so transitions work
        _animator.SetBool(IsGrounded, IsPlayerGrounded());
        _animator.SetFloat(YVelocity, _rb.linearVelocity.y);

        if (_lockMovementDuringAttack && _attack != null && _attack.IsAttacking)
        {
            _animator.SetBool(IsRunning, false);
        }
        else
        {
            bool isRunning = Mathf.Abs(_rb.linearVelocity.x) > _runThreshold;
            _animator.SetBool(IsRunning, isRunning);
        }
    }
    
    private bool IsPlayerGrounded()
    {
        var collisionData = GetComponent<CollisionDataRetriever>();
        return collisionData != null && collisionData.OnGround;
    }
    
    public void TriggerAttack()
    {
        _animator.SetTrigger(Attack);
    }
    
    public void SetPulling(bool isPulling)
    {
        _animator.SetBool(IsPulling, isPulling);
    }
    
    private void HandleBlockStart()
    {
        _animator.SetBool(IsBlocking, true);
    }
    
    private void HandleBlockEnd()
    {
        _animator.SetBool(IsBlocking, false);
    }
    
    private void HandleParrySuccess()
    {
        _animator.SetTrigger(Parry);
    }
    
    public void OnAttackComplete()
    {
        // Called when attack animation finishes
        Debug.Log("Attack animation complete");
    }
}