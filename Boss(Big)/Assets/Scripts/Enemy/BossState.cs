using UnityEngine;

public abstract class BossState
{
    protected Boss boss;

    public BossState(Boss boss)
    {
        this.boss = boss;
    }

    public virtual void Enter() {}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual void Exit() {}
    public virtual void CheckPlayerDistance(float distance, Vector2 direction) {}
    public virtual void CheckPlayerDistanceY(float distance) {}
}
