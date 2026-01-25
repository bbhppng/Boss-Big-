using UnityEngine;

public class PlatformNode : MonoBehaviour
{
    public Collider2D platformCollider;

    private void Awake()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider2D>();
        }
        var finder = Object.FindAnyObjectByType<FindTheBestPlatform>();
        if (finder != null)
        {
            finder.RegisterPlatform(this);
        }
    }

    public float topY => platformCollider.bounds.max.y;
    public float leftX => platformCollider.bounds.min.x;
    public float rightX => platformCollider.bounds.max.x;
    public float centerX => (leftX + rightX) / 2f;
    
    public Vector2 GetLandingPoint()
    {
        // Add safety margin from edges
        float safetyMargin = 0.5f;
        float safeLeft = leftX + safetyMargin;
        float safeRight = rightX - safetyMargin;
        float safeCenterX = (safeLeft + safeRight) / 2f;
    
        return new Vector2(safeCenterX, topY + 0.1f); 
    }
}