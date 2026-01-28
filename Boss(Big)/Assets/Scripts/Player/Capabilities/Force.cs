using UnityEngine;
using UnityEngine.UI;

public class Force : MonoBehaviour
{
    [Header("Assign these")]
    [SerializeField] private Transform _huzz;
    [SerializeField] private Rigidbody2D _huzzRb;
    [SerializeField] private Transform _heroSnapAnchor;
    [SerializeField] private Transform _huzzSnapAnchor;
    //anchor is the point where either the hero or the healer snap themselves to 

    [Header("Pull tuning")]
    [SerializeField] private float _freeDistance = 1f;   
    [SerializeField] private float _maxDistance = 9f;     
    [SerializeField] private float _maxPullForce = 40f;   
    [SerializeField] private float _endDistance = 0.5f;    
    [SerializeField] private float _snapDistance = 1f;    
    [SerializeField] private float _coolDown = 5f; 
    
    [Header("Snap tuning")]
    [SerializeField] float _snapSpeed = 25f;
    
    [Header("Camera")] 
    [SerializeField] private float _cameraShakeIntensity = 0.1f;
    
    [Header("Cooldown UI")]
    [SerializeField] private Image _coolDownFill;

    private Rigidbody2D _rb;
    private Controlls _controller;
    private CameraShake _cameraShake;

    private bool _heroPullRequested;
    private bool _huzzPullRequested;
    
    private bool _isPulling;
    private bool _isSnapping;
    
    private Rigidbody2D _pulledRb;      
    private Transform _pullToward;  
    private Transform _snapTarget;       
    private bool _cancelWhenHeroReleased;  
    
    private int _forceLayer;
    private int _pulledOriginalLayer;

    private float _timer;
    private float _nextReadyTime;

    private void Awake()
    {
        
        _controller = GetComponent<Controlls>();
        _rb = GetComponent<Rigidbody2D>();
        _forceLayer = LayerMask.NameToLayer("Force");
        _cameraShake = GetComponent<CameraShake>();
        _nextReadyTime = Time.time;
        if (_coolDownFill != null)
            _coolDownFill.fillAmount = 1f;
    }

    private void Update()
    {
        
        _heroPullRequested = _controller.input.RetrieveHeroPullInput();
        _huzzPullRequested = _controller.input.RetrieveHuzzPullInput();
        
        UpdateCooldownUI();

        if (_isPulling || _isSnapping)
            return;

        if (Time.time < _nextReadyTime)
            return;

        if (_heroPullRequested)
            BeginHeroPull();
        else if (_huzzPullRequested)
            BeginHuzzPull();
    }

    private void FixedUpdate()
    {
        if (_isPulling)
        {
            Vector2 toTarget = (Vector2)_pullToward.position - _pulledRb.position;
            float distance = toTarget.magnitude;

            bool reached = distance <= _endDistance;
            bool overshot = Vector2.Dot(_pulledRb.linearVelocity, toTarget) < 0f;

            bool canceled =
                _cancelWhenHeroReleased ? !_heroPullRequested : !_huzzPullRequested;

            if (reached || overshot || canceled)
            {
                StartSnap();
            }
            else
            {
                ApplyPullForce(_pulledRb, toTarget, distance);
            }
        }

        if (_isSnapping)
        {
            Vector2 target = _snapTarget != null ? (Vector2)_snapTarget.position : (Vector2)transform.position;
            Vector2 pos = _pulledRb.position;

            Vector2 newPos = Vector2.MoveTowards(pos, target, _snapSpeed * Time.fixedDeltaTime);
            _pulledRb.MovePosition(newPos);

            if (Vector2.Distance(newPos, target) < 0.05f)
            {
                _isSnapping = false;
                FinishPull();
            }
        }
    }
    
    private void UpdateCooldownUI()
    {
        if (_coolDownFill == null)
            return;

        float remaining = Mathf.Max(0f, _nextReadyTime - Time.time);
        float fill = 1f - (remaining / _coolDown);
        _coolDownFill.fillAmount = fill;
    }

    private void ApplyPullForce(Rigidbody2D rb, Vector2 toTarget, float distance)
    {
        if (distance <= _freeDistance)
            return;
        float t = Mathf.Clamp01((distance - _freeDistance) / (_maxDistance - _freeDistance));
        t *= t;
        
        Vector2 force = toTarget.normalized * (_maxPullForce * t);
        rb.AddForce(force, ForceMode2D.Force);
    }

    private void BeginHeroPull()
    {
        if (_huzz == null || _heroSnapAnchor == null)
            return;

        BeginPull(
            pulled: _rb,
            pullToward: _huzz,
            snapTarget: _heroSnapAnchor,
            cancelWhenHeroReleased: true
        );
    }

    private void BeginHuzzPull()
    {
        if (_huzz == null || _huzzRb == null)
            return;

        Transform snap = _huzzSnapAnchor != null ? _huzzSnapAnchor : transform;

        BeginPull(
            pulled: _huzzRb,
            pullToward: transform,
            snapTarget: snap,
            cancelWhenHeroReleased: false
        );
    }

    private void BeginPull(Rigidbody2D pulled, Transform pullToward, 
        Transform snapTarget, bool cancelWhenHeroReleased)
    {
        Vector2 toTarget = (Vector2)pullToward.position - pulled.position;
        float distance = toTarget.magnitude;
        
        if(distance <= _freeDistance)
            return;
        _pulledRb = pulled;
        _pullToward = pullToward;
        _snapTarget = snapTarget;
        _cancelWhenHeroReleased = cancelWhenHeroReleased;

        _pulledRb.linearVelocity = Vector2.zero;
        
        _pulledOriginalLayer = _pulledRb.gameObject.layer;
        _pulledRb.gameObject.layer = _forceLayer;
        _cameraShake.ShakeCamera( _cameraShakeIntensity);
        
        _isPulling = true;
    }

    private void StartSnap()
    {
        _isPulling = false;
        _isSnapping = true;
        _pulledRb.linearVelocity = Vector2.zero;
    }
    
    private void FinishPull()
    {
        _cameraShake.ResetCamera();
        _pulledRb.gameObject.layer = _pulledOriginalLayer;
        _nextReadyTime = Time.time + _coolDown;

        _pulledRb = null;
        _pullToward = null;
        _snapTarget = null;
    }
}
