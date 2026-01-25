using System;
using System.Collections.Generic;
using UnityEngine;

public class FindTheBestPlatform : MonoBehaviour
{
    [SerializeField] private List<PlatformNode> _platforms = new List<PlatformNode>();
    [SerializeField] private Transform _player;
    [SerializeField] private float _maxJumpDistance = 3f;
    
    public void RegisterPlatform(PlatformNode node)
    {
        if (!_platforms.Contains(node))
        {
            _platforms.Add(node);
        }
    }

    public PlatformNode FindPlayerPlatform()
    {
        _platforms.RemoveAll(p => p == null);
        if (_platforms == null || _platforms.Count == 0) {
            Debug.LogError("Platforms array is null or empty!");
            return null;
        }

        if (_player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj == null) {
                Debug.LogError("Could not find GameObject with tag 'Player'!");
                return null;
            }
            _player = pObj.transform;
        }
        PlatformNode best = null;
        float bestDistance = Mathf.Infinity;

        foreach (PlatformNode p in _platforms)
        {
            if (p == null) {
                Debug.LogError("Found NULL platform in _platforms array!");
                continue;
            }
    
            Vector2 landing = p.GetLandingPoint();
            Debug.Log($"Platform landing point: {landing}");
    
            float distance = Vector2.Distance(landing, _player.position);
            Debug.Log($"Distance calculated: {distance}");
    
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
