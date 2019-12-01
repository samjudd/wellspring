using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSHand : MonoBehaviour
{
  [SerializeField]
  protected Transform _centerEye = null;

  private Quaternion _anchorOffsetRotation;
  private Vector3 _anchorOffsetPosition;
  private OVRInput.Controller _controller;
  private OVRCameraRig _cameraRig;
  private MeshRenderer _textMeshRenderer;
  private TextMesh _textMesh;
  private Transform _textMeshTransform;

  void Awake()
  {
    _anchorOffsetPosition = transform.localPosition;
    _anchorOffsetRotation = transform.localRotation;

    // get camerarig and controller from WSController
    _cameraRig = transform.GetComponentInParent<WSController>()._cameraRig;
    _controller = transform.GetComponentInParent<WSController>()._controller;

    // make sure cameraRig is setup upstream
    if (_cameraRig != null)
    {
      _cameraRig.UpdatedAnchors += (r) => { OnUpdatedAnchors(); };
      Debug.Log("WSHand: OVRCameraRig attached to WSHand " + name);
    }
    // if not warn that it was not found
    else 
    {
      Debug.LogWarning("WSHand: OVRCameraRig not attached for WSHand" + name);
    }
    // get reference to display text
    _textMeshTransform =  transform.Find("DisplayText");
    _textMeshRenderer = _textMeshTransform.GetComponent<MeshRenderer>();
    _textMesh = _textMeshTransform.GetComponent<TextMesh>();

    // hide display text
    _textMeshRenderer.enabled = false;

    // check if center eye is set, if not throw warning 
    if (_centerEye == null)
      Debug.LogWarning("WSHand: Center eye location not set, text pointing not enabled.");
  }

  void Update()
  {
    if (_textMeshRenderer.enabled)
      _textMeshTransform.LookAt(_centerEye);
  }

  void OnUpdatedAnchors()
  {
    // use code from OVRGrabber to move the hand objects around
    Vector3 handPos = OVRInput.GetLocalControllerPosition(_controller);
    Quaternion handRot = OVRInput.GetLocalControllerRotation(_controller);
    Vector3 destPos = _cameraRig.transform.TransformPoint(_anchorOffsetPosition + handPos);
    Quaternion destRot = _cameraRig.transform.rotation * handRot * _anchorOffsetRotation;
    
    // can use these if we want physics, but they lag behind for some reason when you move around
    //GetComponent<Rigidbody>().MovePosition(destPos);
    //GetComponent<Rigidbody>().MoveRotation(destRot);

    // no physics needed on the hands for now, these work
    transform.position = destPos;
    transform.rotation = destRot;
  }

  public void DisplayTextOn(string displayText)
  {
    _textMesh.text = displayText;
    _textMeshRenderer.enabled = true;

  }

  public void DisplayTextOff()
  {
    _textMeshRenderer.enabled = false;
  }
}
