using UnityEngine;

public class RequestHeal : MonoBehaviour
{
    [SerializeField] private GameObject _huzz;
    [SerializeField] private float healCooldown = 10f;
    private Heal _healScript;
    private Controlls _controller;
    private bool _healRequsted;
    private float lastHealTime = -10f;
    

    private void Awake()
    {
        _controller = GetComponent<Controlls>();
        _healScript = _huzz.GetComponent<Heal>();
    }
    
    private void Update()
    {
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
