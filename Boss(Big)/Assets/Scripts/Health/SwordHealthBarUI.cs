using UnityEngine;
using UnityEngine.UI;

public class SwordHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health _playerHealth;
    [SerializeField] private Health _allyHealth;

    [SerializeField] private Image _playerFill; 
    [SerializeField] private Image _allyFill;   

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += OnPlayerHealthChanged;

        if (_allyHealth != null)
            _allyHealth.OnHealthChanged += OnAllyHealthChanged;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= OnPlayerHealthChanged;

        if (_allyHealth != null)
            _allyHealth.OnHealthChanged -= OnAllyHealthChanged;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        if (_playerFill == null) return;
        _playerFill.fillAmount = (float)current / max;
    }

    private void OnAllyHealthChanged(int current, int max)
    {
        if (_allyFill == null) return;
        _allyFill.fillAmount = (float)current / max;
    }
}