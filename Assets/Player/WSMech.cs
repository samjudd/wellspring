using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSMech : MonoBehaviour
{
  // Amplitude of the shake. A larger value shakes the camera harder.
  [SerializeField]
  public float _shakeAmount = 0.07f;
  [SerializeField]
  public float _shakeFrequency = 20.0f;
  [SerializeField]
  public float _jumpShakeTime = 0.6f;
  [SerializeField]
  public float _stepShakeTime = 0.1f;
  private Vector3 _originalPos;
  private Task _jumpShake;
  private Task _stepShake;
  private float _startTime;
  private bool _isShaking;

  void OnEnable()
  {
    _originalPos = transform.localPosition;
  }

  protected IEnumerator ShakeCoroutine(float shakeDuration, Vector3 scale)
  {
    while ((Time.time - _startTime) < shakeDuration)
    {
      // * 2 - 1 so it's in range [-1,1] instead of [0,1]
      float noise = Mathf.PerlinNoise(0, Time.time * _shakeFrequency) * 2.0f - 1.0f;
      Vector3 shake = new Vector3(noise, noise, noise);
      shake.Scale(scale);
      transform.localPosition = _originalPos + shake;
      yield return null;
    }
    _isShaking = false;
  }

  public void JumpShake()
  {
    if (!_isShaking)
    {
      Debug.Log("WSMech: Jump shake coroutine started.");
      _startTime = Time.time;
      _isShaking = true;
      _jumpShake = new Task(ShakeCoroutine(_jumpShakeTime, new Vector3(0f, _shakeAmount, 0f)));
    }
  }

  public void StepShake()
  {
    if (!_isShaking)
    {
      Debug.Log("WSMech: Step shake coroutine started.");
      _startTime = Time.time;
      _isShaking = true;
      _stepShake = new Task(ShakeCoroutine(_stepShakeTime, new Vector3(0f, _shakeAmount, 0f)));
    }
  }
}
