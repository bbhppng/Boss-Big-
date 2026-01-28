using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _direction;
    private GameObject _lastOwner;
    private bool _isReflected = false;

    [Header("Movement")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeSpan = 3f;
    [SerializeField] private bool _rotateToDirection = true;

    [Header("Combat")]
    [SerializeField] private int _damage = 5;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _obstacleLayers;

    [Header("Effects")]
    [SerializeField] private float _cameraShakeIntensity = 5f;
    [SerializeField] private float _cameraShakeDuration = 1f;

    private bool _hasHit = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (_direction == Vector2.zero)
        {
            Debug.LogWarning("Projectile direction not set!");
            _direction = Vector2.right;
        }

        ApplyVelocity();
        Destroy(gameObject, _lifeSpan);
    }

    private void ApplyVelocity()
    {
        _rb.linearVelocity = _direction * _speed;

        if (_rotateToDirection)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        if (other.gameObject == _lastOwner) return;

        Block block = other.GetComponent<Block>();
        Health health = other.GetComponent<Health>();
        
        if (block != null)
        {
            if (block.TryParry())
            {
                Vector2 reverseDirection = -_rb.linearVelocity.normalized;
                Reverse(reverseDirection, other.gameObject);
                return;
            }
            
            if (block.IsBlocking)
            {
                DestroyProjectile();
                return;
            }
        }
        
        if (health != null)
        {
            if (!_isReflected && IsInLayerMask(other.gameObject.layer, _targetLayers))
            {
                health.TakeDamage(_damage);

                CameraShake cameraShake = other.GetComponent<CameraShake>();
                if (cameraShake != null)
                {
                    cameraShake.ShakeCamera(_cameraShakeIntensity, _cameraShakeDuration);
                }

                Debug.Log($"Projectile hit {other.name} for {_damage} damage");
                DestroyProjectile();
                return;
            }

            
            if (_isReflected && other.CompareTag("Enemy"))
            {
                health.TakeDamage(_damage * 2);
                Debug.Log($"Projectile hit {other.name} for {_damage} damage");
                DestroyProjectile();
                return;
            }
        }
        
        // if (IsInLayerMask(other.gameObject.layer, _obstacleLayers))
        // {
        //     Debug.Log($"Projectile hit obstacle: {other.name}");
        //     DestroyProjectile();
        // }
    }

    private void DestroyProjectile()
    {
        if (_hasHit) return;
        _hasHit = true;
        Destroy(gameObject);
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public void Reverse(Vector2 newDirection, GameObject newOwner)
    {
        _hasHit = false;
        _lastOwner = newOwner;
        _isReflected = true;

        _direction = newDirection.normalized;
        gameObject.GetComponent<SpriteRenderer>().color = Color.royalBlue;

        ApplyVelocity();

        Debug.Log($"Projectile reversed to {newDirection}");
    }
}
