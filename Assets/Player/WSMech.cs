using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSMech : MonoBehaviour
{
  // Amplitude of the shake. A larger value shakes the camera harder.
  [SerializeField]
  public float _shakeAmount = 0.1f;
  [SerializeField]
  public float _shakeFrequency = 10.0f;
  [SerializeField]
  public float _jumpShakeTime = 0.6f;
  [SerializeField]
  public float _stepShakeTime = 0.2f;
  private Vector3 _originalPos;
  private Task _jumpShake;
  private Task _stepShake;

  void OnEnable()
  {
    _originalPos = transform.localPosition;
  }

  protected IEnumerator ShakeCoroutine(float shakeDuration, Vector3 scale)
  {
    // * 2 - 1 so it's in range [-1,1] instead of [0,1]
    float noise = Mathf.PerlinNoise(0, Time.time * _shakeFrequency) * 2.0f - 1.0f;
    Vector3 shake = new Vector3(noise, noise, noise);
    shake.Scale(scale);
    transform.localPosition = _originalPos + shake;
    yield return new WaitForSeconds(shakeDuration);
  }

  public void JumpShake()
  {
    if (!_jumpShake.Running && !_stepShake.Running)
    {
      Debug.Log("WSMech: Jump shake coroutine started.");
      _jumpShake = new Task(ShakeCoroutine(_jumpShakeTime, new Vector3(0f, _shakeAmount, 0f)));
    }
  }

  public void StepShake()
  {
    if (!_jumpShake.Running && !_stepShake.Running)
    {
      Debug.Log("WSMech: Step shake coroutine started.");
      _stepShake = new Task(ShakeCoroutine(_stepShakeTime, new Vector3(0f, _shakeAmount, 0f)));
    }
  }
}
