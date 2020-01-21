using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSMech : MonoBehaviour
{
  // Amplitude of the shake. A larger value shakes the camera harder.
  [SerializeField]
  public float _shakeAmount = 0.1f;
  [SerializeField]
  public float _jumpShakeTime = 0.6f;
  [SerializeField]
  public float _stepShakeTime = 0.2f;
  private Vector3 _originalPos;
  private bool _isShaking = false;

  void OnEnable()
  {
    _originalPos = transform.localPosition;
  }

  protected IEnumerator ShakeCoroutine(float shakeDuration, Vector3 scale)
  {
    for(float t = shakeDuration; t > 0.0f; t -= Time.deltaTime)
    {
      Vector3 shake = Random.insideUnitSphere * _shakeAmount;
      shake.Scale(scale);
      transform.localPosition = _originalPos + shake;
      yield return null;
    }
    _isShaking = false;
    Debug.Log("WSMech: Shake coroutine ended.");
    yield break;
  }

  public void JumpShake()
  {
    if (!_isShaking)
    {
      _isShaking = true;
      Debug.Log("WSMech: Shake coroutine started.");
      StartCoroutine(ShakeCoroutine(_jumpShakeTime, new Vector3(0f, 1f, 0f)));
    }
  }

  public void StepShake()
  {
    if (!_isShaking)
    {
      _isShaking = true;
      Debug.Log("WSMech: Shake coroutine started.");
      StartCoroutine(ShakeCoroutine(_stepShakeTime, new Vector3(0f, 1f, 0f)));
    }
  }
}
