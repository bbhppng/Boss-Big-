using UnityEngine;
using UnityEngine.UI;

public class RequestHeal : MonoBehaviour
{
    [SerializeField] private GameObject _huzz;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private float healCooldown = 10f;
    private Heal _healScript;
    private Controlls _controller;
    private bool _healRequsted;
    private float lastHealTime = -10f;
    

    private void Awake()
    {
        _controller = GetComponent<Controlls>();
        _healScript = _huzz.GetComponent<Heal>();
        
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 1f;
            cooldownFillImage.type = Image.Type.Filled;
            cooldownFillImage.fillMethod = Image.FillMethod.Radial360;
            cooldownFillImage.fillOrigin = (int)Image.Origin360.Top;
        }
    }
    
    private void Update()
    {
        if (cooldownFillImage != null)
        {
            float timeSinceLastHeal = Time.time - lastHealTime;
            cooldownFillImage.fillAmount = Mathf.Clamp01(timeSinceLastHeal / healCooldown);
        }
        
        _healRequsted = _controller.input.RetrieveHealInput();
        if (_healRequsted)
        {
            if (Time.time >= lastHealTime + healCooldown) {
                _healScript.HealPlayer(gameObject);
                lastHealTime = Time.time;
            }
        }
    }
    
}
