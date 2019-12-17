using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSController : MonoBehaviour
{
  [Tooltip("Boundary of the input dead zone. Controls how much leeway there is in moving the controller before movement starts.")]
  protected float _deadZoneDiameter = 0.05f;
  [SerializeField]
  public OVRCameraRig _cameraRig = null;
  [SerializeField]
  public OVRInput.Controller _controller;

  private Vector3 _home;
  private GameObject _homeMarker;
  private WSHand _hand;
  private Transform _ctrlRegionMarker;
  private Vector3 _ctrlRegionVec = Vector3.zero;
  private float _ctrlRegionRadius = 0.0f;
  private bool _isCtrlRegionSet = false;
  private bool _settingCtrlRegion = false;
  private bool _isHomeSet = false;

  // Start is called before the first frame update
  void Start()
  {
    // Hide the home position marker and control region marker
    _homeMarker = transform.Find("HomePosition").gameObject;
    _homeMarker.GetComponent<MeshRenderer>().enabled = false;
    _ctrlRegionMarker = transform.Find("CtrlRegionMarker");
    _ctrlRegionMarker.GetComponent<MeshRenderer>().enabled = false;

    // get a reference to other stuff we need
    _hand = transform.Find("Hand").GetComponent<WSHand>();
  }

  // Update is called once per frame
  void Update()
  {
    // Top button (B/Y) used to home controller
    if (OVRInput.GetDown(OVRInput.Button.Two, _controller))
    {
      if (_settingCtrlRegion)
        SetControlRegion();
      else
      {
        // home the controller
        HomeController();
        if (!_isCtrlRegionSet)
        {
          // start control region setup
          _settingCtrlRegion = true;
          _hand.DisplayTextOn("Hand Fwd \n Press Y/B");
        }
      }
    }
  }

  private void HomeController()
  {
    _home = OVRInput.GetLocalControllerPosition(_controller);
    Debug.Log("WSController: " + _controller.ToString() +  " homed to: " + _home.ToString("G4"));
    _homeMarker.transform.localScale = (new Vector3(_deadZoneDiameter, _deadZoneDiameter, _deadZoneDiameter)) * 2;
    _homeMarker.transform.position = _cameraRig.transform.TransformPoint(_home);
    _ctrlRegionMarker.position = _cameraRig.transform.TransformPoint(_home);
    // first time stuff only, unhide all markers and whatnot
    if (!_isHomeSet)
    {
      _homeMarker.GetComponent<MeshRenderer>().enabled = true;
      _ctrlRegionMarker.GetComponent<MeshRenderer>().enabled = true;
      _isHomeSet = true;
    }
  }

  private void SetControlRegion()
  {
    // set fwd z direction 
    if (_ctrlRegionVec.x == 0.0f)
    {
      _ctrlRegionVec.x = GetRelativePosition2D().magnitude;
      _hand.DisplayTextOn("Hand Back \n Press Y/B");
    }
    // set backwards z direction
    else if (_ctrlRegionVec.y == 0.0f)
    {
      _ctrlRegionVec.y = GetRelativePosition2D().magnitude;
      _hand.DisplayTextOn("Hand Out \n Press Y/B");
    }
    // set sideways x direction and finish
    else if (_ctrlRegionVec.z == 0.0f)
    {
      _ctrlRegionVec.z = GetRelativePosition2D().magnitude;
      Debug.Log("WSController:" + _controller.ToString() + " control radius set.");
      _hand.DisplayTextOff();
      _ctrlRegionRadius = (_ctrlRegionVec.x + _ctrlRegionVec.y + _ctrlRegionVec.z) / 3;
      // multiply by 2 to convert radius to diam for scaling purposes
      _ctrlRegionMarker.localScale = new Vector3(_ctrlRegionRadius * 2, _ctrlRegionRadius * 2, 1);
      _settingCtrlRegion = false;
      _isCtrlRegionSet = true;
    }
  }

  public Vector3 GetRelativePosition()
  {
    // get the location of the controller relative to the home position
    return OVRInput.GetLocalControllerPosition(_controller) - _home;
  }

  public Vector2 GetRelativePosition2D()
  {
    Vector3 vec = GetRelativePosition();
    return new Vector2(vec.x, vec.z);
  }

  public Vector2 GetControlInput()
  {
    Vector2 pos = GetRelativePosition2D();
    // pass through filtering function position normalized to 0-1 in input range
    return pos.normalized * ControlFilter(Mathf.Clamp01(pos.magnitude / _ctrlRegionRadius));
  }

  public bool Ready()
  {
    return _isHomeSet && _isCtrlRegionSet && !_settingCtrlRegion;
  }

  public bool InDeadZone()
  {
    return GetRelativePosition2D().magnitude <= _deadZoneDiameter;
  }

  private float ControlFilter(float input)
  {
    // unit circle with center at (0,1)
    return 1 - Mathf.Sqrt(1 - Mathf.Pow(input, 2));
  }
}
