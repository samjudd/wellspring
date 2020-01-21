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
      //handPos -= _controller._cameraRig.trackingSpace.position;
      // scale with scaling vector
      handPos.Scale(_mechControllerScaling);
      Quaternion handRot = _controller.GetRotation();
      Vector3 destPos = _handCenter.TransformPoint(_anchorOffsetPosition + handPos);
      Quaternion destRot = _handCenter.rotation * handRot * _anchorOffsetRotation;

      // no physics needed on the hands for now, these work
      transform.position = destPos;
      transform.rotation = destRot;
    }
  }
}
