using UnityEngine;

public class BossStateMachine : MonoBehaviour
{
    public BossState CurrentState { get; private set; }
    private float _lastStateChangeTime;
    private float _minTimeBetweenChanges = 3f;
    
    public void Initialize(BossState initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
        _lastStateChangeTime = Time.time;
    }

    public void ChangeState(BossState newState)
    {
        if (Time.time - _lastStateChangeTime < _minTimeBetweenChanges)
        {
            return;
        }
        
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        
        _lastStateChangeTime = Time.time;
    }

    public void Update()
    {
        CurrentState?.Update();
        CurrentState?.FixedUpdate();
    }
}
