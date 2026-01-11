using UnityEngine;

[RequireComponent(typeof(Controlls), typeof(CollisionDataRetriever), typeof(Rigidbody2D))]
public class Move : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 4f;
    [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 35f;
    [SerializeField, Range(0f, 100f)] private float _maxAirAcceleration = 20f;
    [SerializeField] private Transform visualRoot;

    private Controlls _controller;
    private Vector2 _velocity, _desiredVelocity;
    private Rigidbody2D _rb;
    private CollisionDataRetriever _collisionDataRetriever;
    //private Animator _animator;

    private float _maxSpeedChange, _acceleration;
    private bool _onGround;

    public bool _isFacingRight;
    public Vector2 _direction;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collisionDataRetriever = GetComponent<CollisionDataRetriever>();
        _controller = GetComponent<Controlls>();
        //_animator = GetComponent<Animator>();
        _isFacingRight = visualRoot.localRotation.eulerAngles.y == 0;
    }

    private void Update()
    {
        _direction.x = _controller.input.RetrieveMoveInput();
        _desiredVelocity = new Vector2(_direction.x, 0f) * Mathf.Max(_maxSpeed - _collisionDataRetriever.Friction, 0f);
    }

    private void FixedUpdate()
    {
        _onGround = _collisionDataRetriever.OnGround;
        _velocity = _rb.linearVelocity;

        _acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
        _maxSpeedChange = _acceleration * Time.deltaTime;

        _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, _maxSpeedChange);
        _rb.linearVelocity = _velocity;

        SetDirection();
        //_animator.SetBool("isRunning", Mathf.Abs(_velocity.x) > 0.1f);
    }

    private void SetDirection()
    {
        if (_isFacingRight && _direction.x < 0)
        {
            Flip();
        }
        else if (!_isFacingRight && _direction.x > 0)
        {
            Flip();
        }
    }

    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        visualRoot.localRotation = _isFacingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180f, 0);
    }
}
