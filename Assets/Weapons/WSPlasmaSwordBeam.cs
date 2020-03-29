using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSPlasmaSwordBeam : MonoBehaviour
{
  public float _baseBeamLength = 2.0f;
  public float _beamResetTime = 1.0f;
  public float _damagePerSecond = 5.0f;
  private LayerMask _layerMask;
  private Transform _raycastPoint;
  private float _beamLength;
  private Cooldown _beamResetCooldown;

  public void Awake()
  {
    _raycastPoint = transform.Find("Raycast Point");
    // want weapon to hit on anything, but will do diff things depending on what it hits
    _layerMask = LayerMask.GetMask("Opponent", "Floor", "Stadium", "Player", "Weapon", "Blocker");
    _beamLength = _baseBeamLength;
    _beamResetCooldown = new Cooldown(_beamResetTime);
  }

  public void Update()
  {
  }

  public void FixedUpdate()
  {
    // get position of base of cylinder
    RaycastHit hit;
    if (Physics.Raycast(_raycastPoint.position, _raycastPoint.TransformDirection(Vector3.up), out hit, _beamLength, _layerMask))
    {
      Debug.Log("Raycast hit on beam " + this.name);
      // don't shorten if you hit opponent, just deal damage to them
      if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Opponent"))
      {
        hit.transform.gameObject.SendMessage("Damage", _damagePerSecond / Time.fixedDeltaTime, SendMessageOptions.DontRequireReceiver);
      }
      // shorten cylinder (plasma beam) to location of hit if hitting a barrier
      else
      {
        _beamLength = hit.distance;
        transform.localScale = new Vector3 (transform.localScale.x, _beamLength / _baseBeamLength, transform.localScale.z);
        transform.position = transform.TransformPoint(transform.localPosition - (Vector3.up * (_baseBeamLength - _beamLength)));
      }

      // start/ reset waiting period to reset at 1 second
      _beamResetCooldown.Reset();
    }
    else if (!_beamResetCooldown.OnCooldown())
    {
      ResetBeamLength();
    }
  }

  private void ResetBeamLength()
  {
    // reset length
    transform.localScale = new Vector3 (transform.localScale.x, _baseBeamLength / 2.0f, transform.localScale.z);
    //reset position
    transform.position = Vector3.zero;
    //reset _beamLength var
    _beamLength = _baseBeamLength;
  }
}
