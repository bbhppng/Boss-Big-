using System;
using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private float _parryWindow = 0.2f;
    [SerializeField] private float _parryCooldown = 0.5f;
    
    private IEnumerator _parryAttackWindow;
    private Controlls _controller;
    private bool _isBlocking;
    private bool _isParryEnabled;
    private bool _isParryWindowActive;
    private bool _canParry = true;
    private float _lastBlockTime;
    
    public event Action OnParrySuccess;
    public event Action OnBlockStart;
    public event Action OnBlockEnd;
    
    public bool IsBlocking => _isBlocking;
    public bool IsParryWindowActive => _isParryWindowActive;
    
    [Header("Debug Visuals")]
    [SerializeField] private bool _showDebugVisuals = true;
    [SerializeField] private SpriteRenderer _playerSprite; 
    [SerializeField] private GameObject _parryIndicator;
    private Color _originalColor;
    
    private void Awake()
    {
        _controller = GetComponent<Controlls>();
        
        if (_playerSprite != null)
        {
            _originalColor = _playerSprite.color;
        }
        
        if (_parryIndicator != null)
        {
            _parryIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        bool blockInput = _controller.input.RetrieveBlockInput();
        
        if (blockInput && !_isBlocking && _canParry)
        {
            OnBlockPressed();
        }
        
        if (blockInput && !_isBlocking)
        {
            StartBlocking();
        }
        else if (!blockInput && _isBlocking)
        {
            StopBlocking();
        }
    }

    private void OnBlockPressed()
    {
        float timeSinceLastBlock = Time.time - _lastBlockTime;
        
        if (timeSinceLastBlock > _parryCooldown)
        {
            StartParryWindow();
            _lastBlockTime = Time.time;
        }
    }

    private void StartBlocking()
    {
        _isBlocking = true;
        OnBlockStart?.Invoke();
        
        if (_showDebugVisuals && _playerSprite != null)
        {
            _playerSprite.color = new Color(0.5f, 0.5f, 1f, 1f); 
        }
    }

    private void StopBlocking()
    {
        _isBlocking = false;
        ResetParryWindow();
        OnBlockEnd?.Invoke();
        if (_showDebugVisuals && _playerSprite != null)
        {
            _playerSprite.color = _originalColor;
        }
    }

    private void StartParryWindow()
    {
        if (_parryAttackWindow != null)
        {
            StopCoroutine(_parryAttackWindow);
        }
        _parryAttackWindow = ParryWindowCoroutine();
        StartCoroutine(_parryAttackWindow);
    }

    private IEnumerator ParryWindowCoroutine()
    {
        _isParryEnabled = true;
        _isParryWindowActive = true;
        if (_showDebugVisuals)
        {
            if (_playerSprite != null)
            {
                _playerSprite.color = new Color(1f, 0.92f, 0.016f, 1f); 
            }
            if (_parryIndicator != null)
            {
                _parryIndicator.SetActive(true);
            }
        }
        
        yield return new WaitForSeconds(_parryWindow);
        
        _isParryEnabled = false;
        _isParryWindowActive = false;
        
        if (_showDebugVisuals)
        {
            if (_playerSprite != null && _isBlocking)
            {
                _playerSprite.color = new Color(0.5f, 0.5f, 1f, 1f); 
            }
            if (_parryIndicator != null)
            {
                _parryIndicator.SetActive(false);
            }
        }
    }

    private void ResetParryWindow()
    {
        if (_parryAttackWindow != null)
        {
            StopCoroutine(_parryAttackWindow);
            _parryAttackWindow = null;
        }
        _isParryEnabled = false;
        _isParryWindowActive = false;
    }
    
    public bool TryParry()
    {
        if (_isParryEnabled && _isParryWindowActive)
        {
            OnParrySuccessful();
            return true;
        }
        return false;
    }

    private void OnParrySuccessful()
    {
        OnParrySuccess?.Invoke();
        if (_showDebugVisuals)
        {
            StartCoroutine(ParrySuccessFlash());
        }
        ResetParryWindow();
    }
    
    private IEnumerator ParrySuccessFlash()
    {
        if (_playerSprite != null)
        {
            _playerSprite.color = Color.green;
            yield return new WaitForSeconds(0.15f);
            _playerSprite.color = _originalColor;
        }
    }

    public bool CanBlock()
    {
        return _canParry;
    }
    
    
    public void SetCanParry(bool canParry)
    {
        _canParry = canParry;
        if (_showDebugVisuals && _playerSprite != null && !canParry)
        {
            _playerSprite.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
        else if (_showDebugVisuals && _playerSprite != null)
        {
            _playerSprite.color = _originalColor;
        }
    }
    
    
    private void OnGUI()
    {
        if (!_showDebugVisuals) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        
        float cooldownRemaining = Mathf.Max(0, _parryCooldown - (Time.time - _lastBlockTime));
        
        GUI.Label(new Rect(10, 10, 300, 30), $"Parry Window: {(_isParryWindowActive ? "ACTIVE" : "Inactive")}", style);
        GUI.Label(new Rect(10, 40, 300, 30), $"Blocking: {(_isBlocking ? "YES" : "No")}", style);
        GUI.Label(new Rect(10, 70, 300, 30), $"Can Parry: {(_canParry ? "YES" : "No")}", style);
        GUI.Label(new Rect(10, 100, 300, 30), $"Cooldown: {cooldownRemaining:F2}s", style);
    }
}