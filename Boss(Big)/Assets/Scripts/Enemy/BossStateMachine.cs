using UnityEngine;

public class BossStateMachine : MonoBehaviour
{
    public BossState CurrentState { get; private set; }
    private float _lastStateChangeTime;
    private float _minTimeBetweenChanges = 0.1f;
    
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
            Debug.LogWarning($"State change blocked: {CurrentState?.GetType().Name} -> {newState.GetType().Name}");
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
    }
    
    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}
