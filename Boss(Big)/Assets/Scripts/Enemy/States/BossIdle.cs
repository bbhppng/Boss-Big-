using UnityEngine;

public class BossIdle : BossState
{
    private float _idleDuration;
    private float _idleTimer;

    public BossIdle(Boss boss, float duration = 0.2f) : base(boss)
    {
        _idleDuration = duration;
    }

    public override void Enter()
    {
        Debug.Log("Entering Idle State");
        _idleTimer = 0f;
        boss._rb.linearVelocity = new Vector2(0, boss._rb.linearVelocity.y);
        boss._animator.SetBool("isRunning", false);
    }

    public override void Update()
    {
        _idleTimer += Time.deltaTime;

        if (_idleTimer >= _idleDuration)
        {
            Vector2 toTarget = (Vector2)(boss._player.position - boss.transform.position);
            float distance = toTarget.magnitude;

            if (distance <= 4f && !boss.IsStateOnCooldown(typeof(BossFire), 2f))
            {
                boss._stateMachine.ChangeState(new BossFire(boss));
            }
            else if (!boss.IsStateOnCooldown(typeof(BossTeleport), 3f))
            {
                boss._stateMachine.ChangeState(new BossTeleport(boss));
            }
            else
            {
                _idleTimer = _idleDuration - 0.5f;
            }
        }
    }
}