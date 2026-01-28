using System;
using System.Collections.Generic;
using UnityEngine;

public class FindTheBestPlatform : MonoBehaviour
{
    [SerializeField] public List<PlatformNode> _platforms = new List<PlatformNode>();
    [SerializeField] private float _maxJumpDistance = 3f;
    
    public void RegisterPlatform(PlatformNode node)
    {
        if (!_platforms.Contains(node))
        {
            _platforms.Add(node);
        }
    }

    public PlatformNode FindTargetPlatform(Transform target)
    {
        _platforms.RemoveAll(p => p == null);
        if (_platforms == null || _platforms.Count == 0) {
            Debug.LogError("Platforms array is null or empty!");
            return null;
        }

        if (target == null)
        {
            Debug.LogError("Target transform passed to FindTargetPlatform is null!");
            return null;
        }

        PlatformNode best = null;
        float bestDistance = Mathf.Infinity;

        foreach (PlatformNode p in _platforms)
        {
            // Safety check for null entries in the list
            if (p == null) continue;

            Vector2 landing = p.GetLandingPoint();
            float distance = Vector2.Distance(landing, target.position);
    
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = p;
            }
        }
        return best;
    }
    
    public PlatformNode FindClosestPlatform(Vector2 position)
    {
        if (_platforms == null)
        {
            Debug.LogError("Platform list is NULL!");
            return null;
        }

        _platforms.RemoveAll(p => p == null);
        PlatformNode best = null;
        float bestDistance = Mathf.Infinity;

        foreach (PlatformNode p in _platforms)
        {
            Vector2 landing = p.GetLandingPoint();
            float distance = Vector2.Distance(landing, position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = p;
            }
        }
        return best;
    }

    public PlatformNode FindNextPlatform(PlatformNode current, PlatformNode target)
    {
        if (current == target) return target;
    
        PlatformNode best = null;
        float bestScore = Mathf.Infinity;
        Vector2 currentPos = current.GetLandingPoint();
        Vector2 targetPos = target.GetLandingPoint();

        foreach (PlatformNode p in _platforms)
        {
            if (p == current) continue;
        
            Vector2 platformPos = p.GetLandingPoint();
            float jumpDistance = Vector2.Distance(currentPos, platformPos);
            
            if (jumpDistance > _maxJumpDistance) continue;
        
            float verticalDist = Mathf.Abs(platformPos.y - currentPos.y);
            if (verticalDist > _maxJumpDistance * 0.7f) continue; 
            
            float distToTarget = Vector2.Distance(platformPos, targetPos);
            float progressScore = distToTarget + (jumpDistance * 0.3f); 
        
            if (progressScore < bestScore)
            {
                bestScore = progressScore;
                best = p;
            }
        }
    
        return best ?? target; 
    }

}
