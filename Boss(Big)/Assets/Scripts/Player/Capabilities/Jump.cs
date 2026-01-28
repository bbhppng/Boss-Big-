using UnityEngine;

[RequireComponent(typeof(Controlls), typeof(CollisionDataRetriever), typeof(Rigidbody2D))]
public class Jump : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float _jumpHeight = 3f;
    [SerializeField, Range(0, 5)] private int _maxAirJumps = 0;
    [SerializeField, Range(0f, 5f)] private float _downwardMovementMultiplier = 3f;
    [SerializeField, Range(0f, 5f)] private float _upwardMovementMultiplier = 1.7f;
    [SerializeField, Range(0f, 0.3f)] private float _coyoteTime = 0.2f;
    [SerializeField, Range(0f, 0.3f)] private float _jumpBufferTime = 0.2f;
    
    private Controlls _controller;
    private Rigidbody2D _rb;
    private CollisionDataRetriever _collisionDataRetriever;
    private Vector2 _velocity;

    private int _jumpPhase;
    private float _defaultGravityScale, _jumpSpeed, _coyoteCounter, _jumpBufferCounter;

    private bool _jumpRequested, _onGround, _isJumping, _isJumpReset;
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collisionDataRetriever = GetComponent<CollisionDataRetriever>();
        _controller = GetComponent<Controlls>();

        _isJumpReset = true;
        _defaultGravityScale = 1f;
    }
    
    void Update()
    {
        _jumpRequested = _controller.input.RetrieveJumpInput();
    }

    private void FixedUpdate()
    {
        _onGround = _collisionDataRetriever.OnGround;
        _velocity = _rb.linearVelocity;

        if (_onGround && _rb.linearVelocity.y <= 0.1f)
        {
            _jumpPhase = 0;
            _coyoteCounter = _coyoteTime;
            _isJumping = false;
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
        }

        if (_jumpRequested && _isJumpReset)
        {
            _isJumpReset = false;
            _jumpRequested = false;
            _jumpBufferCounter = _jumpBufferTime;
        }
        
        else if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
        else if (!_jumpRequested)
        {
            _isJumpReset = true;
        }

        if (_jumpBufferCounter > 0)
        {
            JumpAction();
        }

        if (_controller.input.RetrieveJumpInput() && _rb.linearVelocity.y > 0)
        {
            _rb.gravityScale = _upwardMovementMultiplier;
        }
        
        else if (!_controller.input.RetrieveJumpInput() || _rb.linearVelocity.y < 0)
        {
            _rb.gravityScale = _downwardMovementMultiplier;
        }
        
        else if (_rb.linearVelocity.y == 0)
        {
            _rb.gravityScale = _defaultGravityScale;
        }

        _rb.linearVelocity = _velocity;
    }

    private void JumpAction()
    {
        if (_coyoteCounter > 0f || _jumpPhase < _maxAirJumps && _isJumping)
        {
            if (_isJumping)
            {
                _jumpPhase += 1;
            }

            _jumpBufferCounter = 0;
            _coyoteCounter = 0;
            _jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _jumpHeight * _upwardMovementMultiplier);
            _isJumping = true;
            if (_velocity.y > 0f)
            {
                _jumpSpeed = Mathf.Max(_jumpSpeed - _velocity.y, 0f);
            }

            _velocity.y += _jumpSpeed;
        }
    }
}