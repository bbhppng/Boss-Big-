using UnityEngine;

public class FallDeath : MonoBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float _deathHeight = -10f;
    [SerializeField] private bool _useRelativeHeight = false;
    [SerializeField] private float _relativeDeathDistance = 20f;
    
    [Header("Optional")]
    [SerializeField] private bool _showWarning = true;
    [SerializeField] private float _warningHeight = -5f;
    
    private Health _healthScript;
    private float _startHeight;
    private bool _hasWarned = false;
    
    public event System.Action OnFallWarning;
    
    private void Awake()
    {
        _healthScript = GetComponent<Health>();
        
        if (_healthScript == null)
        {
            Debug.LogError("FallDeath: No Health component found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        _startHeight = transform.position.y;
    }
    
    private void Update()
    {
        if (_healthScript.Dead) return;
        
        float currentHeight = transform.position.y;
        float deathThreshold = _useRelativeHeight ? 
            _startHeight - _relativeDeathDistance : 
            _deathHeight;
        
        // Check for warning
        if (_showWarning && !_hasWarned)
        {
            float warningThreshold = _useRelativeHeight ? 
                _startHeight - (_relativeDeathDistance * 0.5f) : 
                _warningHeight;
            
            if (currentHeight <= warningThreshold)
            {
                _hasWarned = true;
                OnFallWarning?.Invoke();
                Debug.Log("Warning: Falling too far!");
            }
        }
        
        // Check for death
        if (currentHeight <= deathThreshold)
        {
            KillPlayer();
        }
    }
    
    private void KillPlayer()
    {
        Debug.Log("Player fell to death at height: " + transform.position.y);
        _healthScript.TakeDamage(_healthScript.GetMaxHealth());
    }
    
    // Optional: Reset warning when player gets back to safe height
    public void ResetWarning()
    {
        _hasWarned = false;
    }
    
    // Optional: Update start height (useful for checkpoints)
    public void UpdateStartHeight()
    {
        _startHeight = transform.position.y;
        _hasWarned = false;
    }
    
    // Optional: Visualize death height in editor
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && _useRelativeHeight)
        {
            // Can't draw relative height in edit mode
            return;
        }
        
        float deathY = _useRelativeHeight ? 
            _startHeight - _relativeDeathDistance : 
            _deathHeight;
        
        Gizmos.color = Color.red;
        Vector3 position = transform.position;
        position.y = deathY;
        
        // Draw a plane at death height
        Gizmos.DrawWireCube(position, new Vector3(10f, 0.1f, 10f));
        
        // Draw warning height
        if (_showWarning)
        {
            float warningY = _useRelativeHeight ? 
                _startHeight - (_relativeDeathDistance * 0.5f) : 
                _warningHeight;
            
            Gizmos.color = Color.yellow;
            position.y = warningY;
            Gizmos.DrawWireCube(position, new Vector3(10f, 0.1f, 10f));
        }
    }
}