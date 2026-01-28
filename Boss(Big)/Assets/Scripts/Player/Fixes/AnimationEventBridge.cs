using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    private Attack _parentAttack;

    private void Awake()
    {
        // Finds the Attack script on the parent
        _parentAttack = GetComponentInParent<Attack>();
    }

    // These are the functions you select in the Animation Window
    public void EnableHitbox() => _parentAttack?.EnableHitbox();
    public void DisableHitbox() => _parentAttack?.DisableHitbox();
    public void OnAttackEnd() => _parentAttack?.OnAttackEnd();
}