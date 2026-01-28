using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin _perlinNoise;
    [SerializeField] private float _defaultShakeIntensity;
    
    private float _shakeTimer = 0f;
    private bool _useTimer = false;

    private void Awake()
    {
        _defaultShakeIntensity = _perlinNoise.AmplitudeGain;
    }

    private void Update()
    {
        if (_useTimer && _shakeTimer > 0f)
        {
            _shakeTimer -= Time.deltaTime;
            
            if (_shakeTimer <= 0f)
            {
                ResetCamera();
            }
        }
    }
    public void ShakeCamera(float intensity, float duration)
    {
        _perlinNoise.AmplitudeGain = intensity;
        _shakeTimer = duration;
        _useTimer = true;
    }
    public void ShakeCamera(float intensity)
    {
        _perlinNoise.AmplitudeGain = intensity;
        _useTimer = false;
        _shakeTimer = 0f;
    }

    public void ResetCamera()
    {
        _perlinNoise.AmplitudeGain = _defaultShakeIntensity;
        _shakeTimer = 0f;
        _useTimer = false;
    }
}