using UnityEngine;

public class BossMove : BossState
{
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _maxAcceleration = 35f;
    [SerializeField] private float _maxAirAcceleration = 20f;
    [SerializeField] private float _stopDistance = 3f;
    
    private Vector2 _velocity;
    private Vector2 _desiredVelocity;
    private float _maxSpeedChange;
    private float _acceleration;
    private bool _onGround;

    public BossMove(Boss boss) : base(boss)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering Move State");
    }

    public override void Update()
    {
        
    }

    public override void FixedUpdate()
    {
        _onGround = boss._collisionDataRetriever.OnGround;
        _velocity = boss._rb.linearVelocity;
        
        _acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
        _maxSpeedChange = _acceleration * Time.deltaTime;
        
        _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, _maxSpeedChange);
        boss._rb.linearVelocity = _velocity;
        
        SetDirection();
        boss._animator.SetBool("isRunning", Mathf.Abs(_velocity.x) > 0.1f);
    }

    public override void Exit()
    {
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
    }

    public override void CheckPlayerDistance(float distance, Vector2 direction)
    {
        if (distance <= _stopDistance)
        {
            _desiredVelocity = Vector2.zero;
        }
        else
        {
            float directionX = Mathf.Sign(direction.x);
            _desiredVelocity = new Vector2(directionX * _maxSpeed, 0f);
        }
    }
    
    public override void CheckPlayerDistanceY(float yDistance)
    {
        if (yDistance > 2f && boss._collisionDataRetriever.OnGround)
        {
            boss._stateMachine.ChangeState(new BossLeap(boss));
        }
    }
    
    private void SetDirection()
    {
        float velocityX = boss._rb.linearVelocity.x;
        
        if (boss._isFacingRight && velocityX < -0.1f)
        {
            boss.Flip();
        }
        else if (!boss._isFacingRight && velocityX > 0.1f)
        {
            boss.Flip();
        }
    }
}
