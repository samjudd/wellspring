using UnityEngine;

public class Cooldown
{
  private float _cooldownTime;
  private float _timer;

  public Cooldown(float cooldownTime)
  {
    _cooldownTime = cooldownTime;
    _timer = 0.0f;
  }
  // have to actually call this in the class you're using it since this isn't a monobehavior 
  // maybe coroutines 
  // maybe invoke? 
  // will have to figure out the resetting cooldown thing
  public void Update()
  {
    Debug.Log("WSCooldownUtil: " + _timer.ToString());
    if (OnCooldown())
      _timer -= Time.deltaTime; 
  }

  public bool OnCooldown()
  {
    // if timer is above 0 thing is on cooldown
    return _timer >= 1e-6;
  }

  public void Reset()
  {
    _timer = _cooldownTime;
  }
}
