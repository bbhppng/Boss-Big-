using UnityEngine;

public class BossLeap : BossState
{
    private float _minDistance = 1.5f;
    private float _heightOffset = 2f;
    private float _maxLeapDuration = 5f;
    private float _leapTimer;
    private bool _isLeaping;
    private PlatformNode _targetPlatform;
    public BossLeap(Boss boss) : base(boss) { }

    public override void Enter()
    {
        Debug.Log("Entering Leap State");
        PlatformNode playerPlatform = boss._platformFinder.FindTargetPlatform(boss.GetCurrentTarget());
        PlatformNode bossPlatform = boss._platformFinder.FindClosestPlatform(boss.transform.position);
    
        if (playerPlatform == null || bossPlatform == null)
        {
            Debug.LogWarning("Platform not found, switching to Move state");
            // boss._stateMachine.ChangeState(new BossMove(boss));
            return;
        }
    
        _targetPlatform = boss._platformFinder.FindNextPlatform(bossPlatform, playerPlatform);
        Vector2 targetPosition;
    
        if (_targetPlatform != null)
        {
            targetPosition = _targetPlatform.GetLandingPoint();
        }
        else
        {
            targetPosition = playerPlatform.GetLandingPoint();
        }
        float distance = Vector2.Distance(boss.transform.position, targetPosition);
        if (distance < _minDistance)
        {
            Debug.Log("Target too close, switching to Move state");
            // boss._stateMachine.ChangeState(new BossMove(boss));
            return;
        }
    
        Vector2 direction = (targetPosition - (Vector2)boss.transform.position);
        Leap(direction, targetPosition);
    }

    public override void Update()
    {
        if (_isLeaping)
        {
            _leapTimer += Time.deltaTime;
            
            if (_leapTimer > _maxLeapDuration)
            {
                Debug.LogWarning("Leap timeout - forcing landing");
                _isLeaping = false;
                boss._animator.SetBool("isJumping", false);
                boss._animator.SetBool("isFalling", false);
                boss.gameObject.layer = boss._originalLayer;
                // boss._stateMachine.ChangeState(new BossMove(boss));
                return;
            }
            
            if (boss._rb.linearVelocity.y <= 0.1f && boss._animator.GetBool("isJumping"))
            {
                boss._animator.SetBool("isJumping", false);
                boss._animator.SetBool("isFalling", true);
                boss.gameObject.layer = boss._originalLayer;
            }
            
            if (boss._collisionDataRetriever.OnGround && boss._rb.linearVelocity.y <= 0.5f)
            {
                _isLeaping = false;
                boss._animator.SetBool("isFalling", false);
                boss._stateMachine.ChangeState(new BossIdle(boss, 0.15f));
            }
        }
    }

    private void Leap(Vector2 direction, Vector2 targetPosition)
    {
        Vector2 startPos = boss.transform.position;
        Vector2 endPos = targetPosition;
        
        Vector2 velocity = CalculateLeapVelocity(startPos, endPos, _heightOffset);
        
        if (velocity != Vector2.zero)
        {
            boss._rb.linearVelocity = velocity;
            boss.gameObject.layer = (int)Mathf.Log(boss._forceLayer.value, 2);
            _isLeaping = true;
            _leapTimer = 0f;
            boss._animator.SetBool("isJumping", true);
            
            // Face the direction of movement
            if (velocity.x != 0)
            {
                if(boss._isFacingRight && velocity.x < 0) boss.Flip();
                else if (!boss._isFacingRight && velocity.x > 0) boss.Flip();
            }
        }
    }
    
    private Vector2 CalculateLeapVelocity(Vector2 start, Vector2 target, float arcHeight)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * boss._rb.gravityScale);
        float displacementY = target.y - start.y;
        float displacementX = target.x - start.x;

        // Ensure arcHeight is above the target
        float requiredHeight = Mathf.Max(arcHeight, displacementY + 0.5f);
    
        // Calculate time to reach apex
        float timeToApex = Mathf.Sqrt(2f * requiredHeight / gravity);

        // Calculate descent time
        float descentHeight = requiredHeight - displacementY;
        if (descentHeight < 0) descentHeight = 0.1f; 
    
        float timeToDescend = Mathf.Sqrt(2f * descentHeight / gravity);
        float totalTime = timeToApex + timeToDescend;

        // Avoid division by zero
        if (totalTime <= 0.01f) totalTime = 0.5f;

        Vector2 velocity = new Vector2(
            displacementX / totalTime,
            Mathf.Sqrt(2f * gravity * requiredHeight)
        );

        return velocity;
    }
}
