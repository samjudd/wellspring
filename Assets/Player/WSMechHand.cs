using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSMechHand : MonoBehaviour
{
  [Tooltip("WSController instance this mech hand should be linked to.")]
  public WSController _controller;
  [Tooltip("x, y, z scalings for controller to mech hand.")]
  public Vector3 _mechControllerScaling = new Vector3(3.5f, 3.0f, 3.5f); 
  [Tooltip("Origin for the mech hands.")]
  public Transform _handCenter;
  [Tooltip("Transform to set the mech hand position relative to.")]
  public Transform _handRelativeCenter;
  [Tooltip("Whether the mech hands track position relative to the player head or not.")]
  public bool _useRelativeTracking = false;
  [Tooltip("Furthest distance from the mech center the hands can go in x,y,z.")]
  public float _outerLimit = 3.0f;
  private Quaternion _anchorOffsetRotation;
  private Vector3 _anchorOffsetPosition;

  // Start is called before the first frame update
  void Awake()
  {
    bool useOffset = false;
    if (useOffset)
    {
      _anchorOffsetPosition = transform.localPosition;
      _anchorOffsetRotation = transform.localRotation;
    }
    else
    {
      _anchorOffsetPosition = Vector3.zero;
      _anchorOffsetRotation = Quaternion.identity;
    }
    // if a controller hasn't been selected throw an error
    if(_controller == null)
    {
      Debug.LogError("No controller selected for WSMechHand named " + name);
    }

    // whenever the hand anchors are updated by the camera rig, update the position of these hands
    _controller._cameraRig.UpdatedAnchors += (r) => { OnUpdatedAnchors(); };
  }

  void OnUpdatedAnchors()
  {
    if (_controller.IsMechHandController())
    {
      // use code from OVRGrabber to move the hand objects around
      Vector3 handPos = _controller.GetPosition();
      // get relative position between controller and tracking center
      if (_useRelativeTracking)
        handPos = handPos - _handRelativeCenter.localPosition;
      // scale with scaling vector
      handPos.Scale(_mechControllerScaling);
      Vector3 destPos = _handCenter.TransformPoint(_anchorOffsetPosition + handPos);

      Quaternion handRot = _controller.GetRotation();
      Quaternion destRot = _handCenter.rotation * handRot * _anchorOffsetRotation;

      // no physics needed on the hands for now, these work
      transform.position = destPos.magnitude <= _outerLimit ? destPos : destPos.normalized * _outerLimit;
      transform.rotation = destRot;
    }
  }
}
