using UnityEngine;

public class Heal : MonoBehaviour
{
    [SerializeField] private int _healAmount = 20;

    public void HealPlayer(GameObject player)
    {
        player.GetComponent<Health>().Heal(_healAmount);
    }
}
