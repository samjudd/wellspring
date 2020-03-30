using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSPlasmaSwordBeam : MonoBehaviour
{
  public float _baseBeamLength = 2.0f;
  public float _beamResetTime = 1.0f;
  public float _damagePerSecond = 0.5f;
  private LayerMask _layerMask;
  private Transform _raycastPoint;
  private Transform _swordBeam;
  private float _beamLength;
  private Cooldown _beamResetCooldown;

  public void Awake()
  {
    _raycastPoint = transform.Find("Raycast Point");
    _swordBeam = transform.Find("Sword Beam");
    // want weapon to hit on anything, but will do diff things depending on what it hits
    _layerMask = LayerMask.GetMask("Opponent", "Floor", "Stadium", "Player", "Weapon", "Blocker");
    _beamLength = _baseBeamLength;
    _beamResetCooldown = new Cooldown(_beamResetTime);
  }

  public void Update()
  {
    // update the beam reset cooldown
    _beamResetCooldown.Update();
  }

  public void FixedUpdate()
  {
    // get position of base of cylinder
    RaycastHit hit;
    if (Physics.Raycast(_raycastPoint.position, _raycastPoint.TransformDirection(Vector3.up), out hit, _beamLength, _layerMask))
    {
      Debug.Log("WSPlasmaSwordBeam: Hit");
      // don't shorten if you hit opponent, just deal damage to them
      if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Opponent"))
        hit.transform.gameObject.SendMessage("Damage", _damagePerSecond * Time.fixedDeltaTime, SendMessageOptions.DontRequireReceiver);
      // shorten cylinder (plasma beam) to location of hit if hitting a barrier
      else
      {
        _beamLength = hit.distance;
        _swordBeam.localScale = new Vector3 (_swordBeam.localScale.x, _beamLength / _baseBeamLength, _swordBeam.localScale.z);
        _swordBeam.position = _raycastPoint.TransformPoint(0, _beamLength / 2.0f, 0);
      }
      // start/ reset waiting period to reset at 1 second
      _beamResetCooldown.Reset();
    }
    else if (!_beamResetCooldown.OnCooldown() && _beamLength < _baseBeamLength)
    {
      ResetBeamLength();
    }
  }

  private void ResetBeamLength()
  {
    // reset length
    _swordBeam.localScale = new Vector3 (_swordBeam.localScale.x, _baseBeamLength / 2.0f, _swordBeam.localScale.z);
    //reset position
    _swordBeam.position = _raycastPoint.TransformPoint(0, _baseBeamLength / 2.0f, 0);
    //reset _beamLength var
    _beamLength = _baseBeamLength;
  }
}
