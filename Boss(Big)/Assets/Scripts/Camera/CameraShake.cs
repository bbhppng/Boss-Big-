using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin _perlinNoise;
    [SerializeField] private float _defaultShakeIntensity;

    private void Awake()
    {
        _defaultShakeIntensity = _perlinNoise.AmplitudeGain;
    }

    public void ShakeCamera(float intensity)
    {
        _perlinNoise.AmplitudeGain = intensity;
    }

    public void ResetCamera()
    {
        _perlinNoise.AmplitudeGain = _defaultShakeIntensity;
    }
}
