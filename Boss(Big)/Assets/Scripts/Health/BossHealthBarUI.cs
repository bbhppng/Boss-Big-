using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health _bossHealth;
    [SerializeField] private Image _bossFill; 

    private void OnEnable()
    {
        if (_bossHealth != null)
            _bossHealth.OnHealthChanged += OnBossHealthChanged;
        
    }

    private void OnDisable()
    {
        if (_bossHealth != null)
            _bossHealth.OnHealthChanged -= OnBossHealthChanged;
    }

    private void OnBossHealthChanged(int current, int max)
    {
        if (_bossFill == null) return;
        _bossFill.fillAmount = (float)current / max;
    }
}
