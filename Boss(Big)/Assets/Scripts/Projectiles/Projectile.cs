using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _direction;
    
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
        
        _rb.linearVelocity = _direction * _speed;
        if (_rotateToDirection)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        Destroy(gameObject, _lifeSpan);
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
        
        if (IsInLayerMask(other.gameObject.layer, _targetLayers))
        {
            CameraShake cameraShake = other.GetComponent<CameraShake>();
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(_damage);
                Debug.Log($"Projectile hit {other.name} for {_damage} damage");
                if (cameraShake != null)
                {
                    cameraShake.ShakeCamera(_cameraShakeIntensity);
                }
                DestroyProjectile();
            }
        }
        else if (IsInLayerMask(other.gameObject.layer, _obstacleLayers))
        {
            Debug.Log($"Projectile hit obstacle: {other.name}");
            DestroyProjectile();
        }
    }
    
    private void DestroyProjectile()
    {
        if (_hasHit) return;
        _hasHit = true;
        Destroy(gameObject);
    }
    
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}