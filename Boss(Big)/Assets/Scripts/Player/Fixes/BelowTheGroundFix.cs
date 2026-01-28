using UnityEngine;

public class BelowTheGroundFix : MonoBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private float _skinWidth = 0.05f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() 
    {
        if (_rb.linearVelocity.y >= 0) return; 
        
        float distanceThisFrame = Mathf.Abs(_rb.linearVelocity.y) * Time.fixedDeltaTime;
        
        RaycastHit2D hit = Physics2D.Raycast(
            _rb.position,
            Vector2.down,
            distanceThisFrame + _skinWidth,
            _groundMask
        );

        if (hit && hit.distance < distanceThisFrame + _skinWidth)
        {
            Vector2 newPosition = new Vector2(
                _rb.position.x, 
                hit.point.y + _skinWidth
            );
            _rb.MovePosition(newPosition);
            
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
        }
    }
    
}