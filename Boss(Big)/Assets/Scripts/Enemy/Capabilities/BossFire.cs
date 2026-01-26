using UnityEngine;

public class BossFire : BossState
{
    public BossFire(Boss boss) : base(boss) { }

    public override void Enter()
    {
        Debug.Log("Entering Fire State");
    }
    
    
}
