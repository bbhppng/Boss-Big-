using UnityEngine;

public class BossFireAnimationBridge : MonoBehaviour
{
    private Boss _boss;

    private void Awake()
    {
        _boss = GetComponentInParent<Boss>();
    }
    public void OnFireEvent()
    {
        if (_boss != null && _boss._stateMachine.CurrentState is BossFire fireState)
        {
            fireState.OnFireEvent();
        }
    }
    public void OnAnimationComplete()
    {
        if (_boss != null && _boss._stateMachine.CurrentState is BossFire fireState)
        {
            fireState.OnAnimationComplete();
        }
    }
}