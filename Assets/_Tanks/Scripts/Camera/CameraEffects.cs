using System.Collections;
using Tanks.Complete;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private Material _cameraShaders;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeAmplitude = 0.01f;

    private bool _isShaking = false;

    private const string SHAKE_AMPLITUDE = "_ShakeAmplitude";

    private void Awake()
    {
        TankHealth.OnTakeDamage += TankHealth_OnTakeDamage;
    }

    private void OnDestroy()
    {
        TankHealth.OnTakeDamage -= TankHealth_OnTakeDamage;
    }

    private void TankHealth_OnTakeDamage()
    {
        if (!_isShaking) StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        _isShaking = true;
        _cameraShaders.SetFloat(SHAKE_AMPLITUDE, _shakeAmplitude);
        yield return new WaitForSeconds(_shakeDuration);
        _cameraShaders.SetFloat(SHAKE_AMPLITUDE, 0);
        _isShaking = false;
    }
}