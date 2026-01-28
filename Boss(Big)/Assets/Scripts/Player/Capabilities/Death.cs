using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Death : MonoBehaviour
{
    [Header("Death Screen UI")]
    [SerializeField] private GameObject _deathScreenPanel;
    [SerializeField] private Image _deathScreenImage;
    
    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _delayBeforeSceneChange = 1f;
    
    [Header("Scene")]
    [SerializeField] private string _menuSceneName = "Menu";
    
    private Health _healthScript;
    private CharacterController _characterController;
    private MonoBehaviour[] _movementScripts;
    
    private void Awake()
    {
        _healthScript = GetComponent<Health>();
        _characterController = GetComponent<CharacterController>();
        
        // Get all movement-related scripts (adjust based on your project)
        _movementScripts = GetComponents<MonoBehaviour>();
        
        // Make sure death screen starts invisible
        if (_deathScreenPanel != null)
        {
            CanvasGroup canvasGroup = _deathScreenPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _deathScreenPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            _deathScreenPanel.SetActive(false);
        }
        
        if (_deathScreenImage != null)
        {
            Color color = _deathScreenImage.color;
            color.a = 0f;
            _deathScreenImage.color = color;
        }
    }
    
    private void OnEnable()
    {
        if (_healthScript != null)
        {
            _healthScript.OnDeath += HandleDeath;
        }
    }
    
    private void OnDisable()
    {
        if (_healthScript != null)
        {
            _healthScript.OnDeath -= HandleDeath;
        }
    }
    
    private void HandleDeath()
    {
        StartCoroutine(DeathSequence());
    }
    
    private IEnumerator DeathSequence()
    {
        // Stop all movement
        StopPlayerMovement();
        
        // Activate death screen
        if (_deathScreenPanel != null)
        {
            _deathScreenPanel.SetActive(true);
        }
        
        // Fade in the death screen
        yield return StartCoroutine(FadeInDeathScreen());
        
        // Wait a bit before changing scene
        yield return new WaitForSeconds(_delayBeforeSceneChange);
        
        // Change to menu scene
        SceneManager.LoadScene(_menuSceneName);
    }
    
    private void StopPlayerMovement()
    {
        // Disable character controller
        if (_characterController != null)
        {
            _characterController.enabled = false;
        }
        
        // Disable Rigidbody if present
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Disable common movement scripts (adjust script names based on your project)
        foreach (var script in _movementScripts)
        {
            if (script != null && script != this && script.enabled)
            {
                // Check for common movement script names
                string scriptName = script.GetType().Name;
                if (scriptName.Contains("Movement") || 
                    scriptName.Contains("Controller") || 
                    scriptName.Contains("Player") ||
                    scriptName.Contains("Input"))
                {
                    script.enabled = false;
                }
            }
        }
        
        // Disable player input if you have an input script
        var inputScript = GetComponent<MonoBehaviour>();
        if (inputScript != null)
        {
            inputScript.enabled = false;
        }
    }
    
    private IEnumerator FadeInDeathScreen()
    {
        float elapsed = 0f;
        
        CanvasGroup panelCanvasGroup = null;
        if (_deathScreenPanel != null)
        {
            panelCanvasGroup = _deathScreenPanel.GetComponent<CanvasGroup>();
        }
        
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / _fadeDuration);
            
            // Fade panel
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = alpha;
            }
            
            // Fade image
            if (_deathScreenImage != null)
            {
                Color color = _deathScreenImage.color;
                color.a = alpha;
                _deathScreenImage.color = color;
            }
            
            yield return null;
        }
        
        // Ensure fully visible
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
        }
        
        if (_deathScreenImage != null)
        {
            Color color = _deathScreenImage.color;
            color.a = 1f;
            _deathScreenImage.color = color;
        }
    }
}