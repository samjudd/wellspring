using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSMech : MonoBehaviour
{
  // Amplitude of the shake. A larger value shakes the camera harder.
  public float _jumpShakeAmount = 0.08f;
  public float _stepShakeAmount = 0.04f;
  public float _jumpShakeFrequency = 12.0f;
  public float _stepShakeFrequency = 7.0f;
  public float _jumpShakeTime = 0.4f;
  public float _stepShakeTime = 0.20f;
  public AudioClip[] _stepSounds;
  public AudioClip _jumpSound;
  private Vector3 _originalPos;
  private Task _jumpShake;
  private Task _stepShake;
  private float _startTime;
  private bool _isShaking;

  void OnEnable()
  {
    _originalPos = transform.localPosition;
  }

  protected IEnumerator ShakeCoroutine(float shakeDuration, Vector3 scale, float frequency)
  {
    while ((Time.time - _startTime) < shakeDuration)
    {
      // * 2 - 1 so it's in range [-1,1] instead of [0,1]
      float xNoise = Mathf.PerlinNoise(0, Time.time * frequency) * 2.0f - 1.0f;
      float yNoise = Mathf.PerlinNoise(5, Time.time * frequency) * 2.0f - 1.0f;
      float zNoise = Mathf.PerlinNoise(10, Time.time * frequency) * 2.0f - 1.0f;
      Vector3 shake = new Vector3(xNoise, yNoise, zNoise);
      float decayScaling = (Time.time - _startTime) / shakeDuration;
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
      _jumpShake = new Task(ShakeCoroutine(_jumpShakeTime, new Vector3(_jumpShakeAmount * 0.5f, _jumpShakeAmount, _jumpShakeAmount * 0.5f), _jumpShakeFrequency));
      GetComponent<AudioSource>().PlayOneShot(_jumpSound, 1.0f);
    }
  }

  public void StepShake()
  {
    if (!_isShaking)
    {
      Debug.Log("WSMech: Step shake coroutine started.");
      _startTime = Time.time;
      _isShaking = true;
      _stepShake = new Task(ShakeCoroutine(_stepShakeTime, new Vector3(_stepShakeAmount * 0.5f, _stepShakeAmount, _stepShakeAmount * 0.5f), _stepShakeFrequency));
      GetComponent<AudioSource>().PlayOneShot(_stepSounds[Random.Range(0,_stepSounds.Length)], 0.7f);
    }
  }
}
