using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSPlayer : MonoBehaviour
{
  [Tooltip("The maximum force that you can apply to the player for movement.")]
  public float _maxForce = 30000.0f;

  [Tooltip("The maximum torque that you can apply to the player.")]
  public float _maxTorque = 400.0f;

  [Tooltip("Player speed limit [m/s].")]
  public float _maxSpeed = 4.0f;

  [Tooltip("Player angular velocity limit [rad/s].")]
  public float _maxAngularVelocity = 4.0f;

  [Tooltip("The amount of force to apply when jumping [N].")]
  public float _jumpForce = 750.0f;
  [Tooltip("Percent of max value to adjust to if there is only one hand being used as an input. Should not exceed 1.")]
  public float _singleHandInputFactor = 0.7f;

  // create enum to classify movement inputs to make things easier
  private enum InputType { NOINPUT, JUMP, MOVE, ROTATE };
  // robot states
  private enum MechState {SETUP, NOMINAL, JUMPING, GATHERING};
  private MechState _mechState = MechState.SETUP;
  private Dictionary<MechState, List<InputType>> AllowableInputs = new Dictionary<MechState, List<InputType>>()
  {
    {MechState.NOMINAL,   new List<InputType>() {InputType.NOINPUT, InputType.JUMP, InputType.MOVE, InputType.ROTATE}},
    {MechState.GATHERING, new List<InputType>() {InputType.NOINPUT}},
    {MechState.JUMPING,   new List<InputType>() {InputType.NOINPUT}},
    {MechState.SETUP,     new List<InputType>() {InputType.NOINPUT}}
  };

  private OVRCameraRig _cameraRig = null;
  private Rigidbody _playerBody = null;
  private CapsuleCollider _collider = null;
  private WSHand _leftHand = null;
  private WSHand _rightHand = null;
  private WSController _leftController = null;
  private WSController _rightController = null;
  private WSMech _mech = null;

  // Awake is called before Start so we can do stuff in Start() with the things we get here
  void Awake()
  {
    // We use OVRCameraRig to set rotations to cameras,
    // and to be influenced by rotation
    OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();
    if (CameraRigs.Length == 0)
      Debug.LogWarning("WSPlayer: No OVRCameraRig attached.");
    else if (CameraRigs.Length > 1)
      Debug.LogWarning("WSPlayer: More then 1 OVRCameraRig attached.");
    else
      _cameraRig = CameraRigs[0];

    // Get the RigidBody we're going to use
    Rigidbody[] RigidBodies = gameObject.GetComponents<Rigidbody>();
    if (RigidBodies.Length == 0)
      Debug.LogWarning("WSPlayer: No RigidBody attached.");
    else if (RigidBodies.Length > 1)
      Debug.LogWarning("WSPlayer: More then 1 RigidBodies attached.");
    else
      _playerBody = RigidBodies[0];
    _playerBody.maxAngularVelocity = _maxAngularVelocity;

    // Get the player's collider, warn if it is not found for some reason
    _collider = transform.GetComponent<CapsuleCollider>();
    if (_collider == null)
    {
      Debug.LogWarning("WSPlayer: Player collider not found.");
    }

    // Get references to left and right hands and controllers to use later
    _leftHand = transform.Find("LeftHand/Hand").GetComponent<WSHand>();
    _rightHand = transform.Find("RightHand/Hand").GetComponent<WSHand>();
    _leftController = transform.Find("LeftHand").GetComponent<WSController>();
    _rightController = transform.Find("RightHand").GetComponent<WSController>();

    // get reference to mech 
    _mech = transform.Find("mech").GetComponent<WSMech>();
  }

  // fixedUpdate is called at a constant rate, use for physics/rigidbody stuff
  void FixedUpdate()
  {
    // leave setup once ready to go, otherwise no need to bother with the rest of this function
    if (_mechState == MechState.SETUP && _leftController.Ready() && _rightController.Ready())
      _mechState = MechState.NOMINAL;

    // when you actually get off the ground change to jumping to start looking for landing time
    if (_mechState == MechState.GATHERING && !IsGrounded())
    {
      _mechState = MechState.JUMPING;
    }

    // once you're jumping, start waiting to detect landing
    if (_mechState == MechState.JUMPING && IsGrounded())
    {
      _mechState = MechState.NOMINAL;
      _mech.JumpShake();
    }

    // get positions of controllers
    Vector2 leftInput = _leftController.GetRelativePosition2D();
    Vector2 rightInput = _rightController.GetRelativePosition2D();

    // do appropriate movement based on input type 
    switch (classifyInput(leftInput, rightInput))
    {
      case InputType.JUMP:
        if (IsInputAllowable(InputType.JUMP))
        {
          _mechState = MechState.GATHERING;
          Debug.Log("WSPlayer: Jump input executed.");
          _playerBody.AddRelativeForce(0, _jumpForce, 0, ForceMode.Impulse);
        }
        break;

      case InputType.MOVE:
        if (IsInputAllowable(InputType.MOVE))
        {
          Vector2 force = (_leftController.GetControlInput() + _rightController.GetControlInput()) / 2.0f * _maxForce;
          if (_leftController.IsMechHandController() || _rightController.IsMechHandController())
            force = force * 2.0f * _singleHandInputFactor;
          _playerBody.AddRelativeForce(force.x, 0, force.y, ForceMode.Force);
        }
        break;

      case InputType.ROTATE:
        if (IsInputAllowable(InputType.ROTATE))
        {
          float torque = (_leftController.GetControlInput().y - _rightController.GetControlInput().y) / 2 * _maxTorque;
          _playerBody.AddRelativeTorque(0, torque, 0, ForceMode.Force);
        }
        break;

      case InputType.NOINPUT:
        break;
    }

    // limit speed
    _playerBody.velocity = _playerBody.velocity.magnitude > _maxSpeed ? _playerBody.velocity.normalized * _maxSpeed : _playerBody.velocity;
  }

  InputType classifyInput(Vector2 leftInput, Vector2 rightInput)
  {
    // if you're jumping don't worry about input just jump
    if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch) || OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
    {
      return InputType.JUMP;
    }
    // if neither hand is in the dead zone, check whether you're moving or rotating
    if (!_leftController.InDeadZone() && !_rightController.InDeadZone())
    {
      // if dot product is greater than zero, vectors are in the same direction so user is trying to move
      if (Vector2.Dot(leftInput, rightInput) > 0 || _leftController.IsMechHandController() || _rightController.IsMechHandController())
      {
        return InputType.MOVE;
      }
      // if z inputs are in opposite directions we're trying to rotate
      else if (leftInput.y * rightInput.y < 0)
      {
        return InputType.ROTATE;
      }
    }
    return InputType.NOINPUT;
  }

  bool IsGrounded()
  {
    Vector3 position = _collider.transform.position;
    position.y = _collider.bounds.min.y + 0.1f;
    float length = 0.3f;
    // layer 8 should be just the floor so i don't hit the player or something 
    // it uses a bitmask so somehow this does what i want, i don't understand it at all
    return Physics.Raycast(position, -_collider.transform.up, length, 1 << 8);
  }

  bool IsInputAllowable(InputType input)
  {
    return AllowableInputs[_mechState].Contains(input);
  }
}