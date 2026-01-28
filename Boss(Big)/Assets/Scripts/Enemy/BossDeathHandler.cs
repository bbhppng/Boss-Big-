using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDeathHandler : MonoBehaviour
{
    [Header("Victory Screen UI")]
    [SerializeField] private GameObject _victoryScreenPanel;
    [SerializeField] private Image _victoryScreenImage;
    
    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _delayBeforeSceneChange = 2f;
    
    [Header("Scene")]
    [SerializeField] private string _nextSceneName = "Menu";
    
    [Header("Optional Effects")]
    [SerializeField] private bool _disablePlayerControlsOnDeath = true;
    [SerializeField] private GameObject _player;
    
    private Health _healthScript;
    private bool _isDying = false;
    
    private void Awake()
    {
        _healthScript = GetComponent<Health>();
        
        if (_healthScript == null)
        {
            Debug.LogError("BossDeathHandler: No Health component found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Make sure victory screen starts invisible
        if (_victoryScreenPanel != null)
        {
            CanvasGroup canvasGroup = _victoryScreenPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _victoryScreenPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            _victoryScreenPanel.SetActive(false);
        }
        
        if (_victoryScreenImage != null)
        {
            Color color = _victoryScreenImage.color;
            color.a = 0f;
            _victoryScreenImage.color = color;
        }
    }
    
    private void OnEnable()
    {
        if (_healthScript != null)
        {
            _healthScript.OnDeath += HandleBossDeath;
        }
    }
    
    private void OnDisable()
    {
        if (_healthScript != null)
        {
            _healthScript.OnDeath -= HandleBossDeath;
        }
    }
    
    private void HandleBossDeath()
    {
        if (_isDying) return;
        
        _isDying = true;
        StartCoroutine(BossDeathSequence());
    }
    
    private IEnumerator BossDeathSequence()
    {
        // Stop boss movement and attacks
        StopBossActions();
        
        // Optionally disable player controls
        if (_disablePlayerControlsOnDeath && _player != null)
        {
            DisablePlayerControls();
        }
        
        // Activate victory screen
        if (_victoryScreenPanel != null)
        {
            _victoryScreenPanel.SetActive(true);
        }
        
        // Fade in the victory screen
        yield return StartCoroutine(FadeInVictoryScreen());
        
        // Wait before changing scene
        yield return new WaitForSeconds(_delayBeforeSceneChange);
        
        // Change to next scene
        SceneManager.LoadScene(_nextSceneName);
    }
    
    private void StopBossActions()
    {
        // Disable AI/movement components
        var navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }
        
        // Disable Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Disable all MonoBehaviour scripts except this one and Health
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            if (script != null && script != this && !(script is Health) && script.enabled)
            {
                script.enabled = false;
            }
        }
        
        // Stop animations
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }
    
    private void DisablePlayerControls()
    {
        // Disable player movement scripts
        MonoBehaviour[] playerScripts = _player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            if (script != null && script.enabled)
            {
                string scriptName = script.GetType().Name;
                if (scriptName.Contains("Movement") || 
                    scriptName.Contains("Controller") || 
                    scriptName.Contains("Input") ||
                    scriptName.Contains("Attack"))
                {
                    script.enabled = false;
                }
            }
        }
        
        // Stop player rigidbody
        Rigidbody playerRb = _player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }
        
        // Disable character controller
        CharacterController playerCC = _player.GetComponent<CharacterController>();
        if (playerCC != null)
        {
            playerCC.enabled = false;
        }
    }
    
    private IEnumerator FadeInVictoryScreen()
    {
        float elapsed = 0f;
        
        CanvasGroup panelCanvasGroup = null;
        if (_victoryScreenPanel != null)
        {
            panelCanvasGroup = _victoryScreenPanel.GetComponent<CanvasGroup>();
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
            if (_victoryScreenImage != null)
            {
                Color color = _victoryScreenImage.color;
                color.a = alpha;
                _victoryScreenImage.color = color;
            }
            
            yield return null;
        }
        
        // Ensure fully visible
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
        }
        
        if (_victoryScreenImage != null)
        {
            Color color = _victoryScreenImage.color;
            color.a = 1f;
            _victoryScreenImage.color = color;
        }
    }
}