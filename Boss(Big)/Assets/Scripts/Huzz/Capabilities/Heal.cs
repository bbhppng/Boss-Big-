using UnityEngine;

public class Heal : MonoBehaviour
{
    [SerializeField] private int _healAmount = 20;
    [SerializeField] private float _healDuration = 2f;
    [SerializeField] private bool _useGradualHeal = true;

    public void HealPlayer(GameObject player)
    {
        Health healthScript = player.GetComponent<Health>();
        
        if (healthScript != null)
        {
            if (_useGradualHeal)
            {
                healthScript.HealGradually(_healAmount, _healDuration);
            }
            else
            {
                healthScript.Heal(_healAmount);
            }
        }
    }
}
