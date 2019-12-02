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
  public float _maxSpeed = 8.0f;

  [Tooltip("Player angular velocity limit [rad/s].")]
  public float _maxAngularVelocity = 7.0f;

  [Tooltip("The amount of force to apply when jumping.")]
  public float _jumpForce = 1000.0f;

  // create enum to classify movement inputs to make things easier
  private enum InputType { NOINPUT, JUMP, MOVE, ROTATE };

  private OVRCameraRig _cameraRig = null;
  private Rigidbody _playerBody = null;
  private Collider _collider = null;
  private WSHand _leftHand = null;
  private WSHand _rightHand = null;
  private WSController _leftController = null;
  private WSController _rightController = null;

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

    // Get the CharacterController we're going to use
    Rigidbody[] RigidBodies = gameObject.GetComponents<Rigidbody>();
    if (RigidBodies.Length == 0)
      Debug.LogWarning("WSPlayer: No RigidBody attached.");
    else if (RigidBodies.Length > 1)
      Debug.LogWarning("WSPlayer: More then 1 RigidBodies attached.");
    else
      _playerBody = RigidBodies[0];
    _playerBody.maxAngularVelocity = _maxAngularVelocity;

    // Get the player's collider, warn if it is not found for some reason
    _collider = gameObject.GetComponent<CapsuleCollider>();
    if (_collider == null)
    {
      Debug.LogWarning("WSPlayer: Player collider not found.");
    }

    // Get references to left and right hands and controllers to use later
    _leftHand = transform.Find("LeftHand/Hand").GetComponent<WSHand>();
    _rightHand = transform.Find("RightHand/Hand").GetComponent<WSHand>();
    _leftController = transform.Find("LeftHand").GetComponent<WSController>();
    _rightController = transform.Find("RightHand").GetComponent<WSController>();
  }

  // fixedUpdate is called at a constant rate, use for physics/rigidbody stuff
  void FixedUpdate()
  {
    // if we're grounded (i.e. not jumping) and both controllers are all setup
    if (IsGrounded() && _leftController.Ready() && _rightController.Ready())
    {
      Vector2 leftInput = _leftController.GetRelativePosition2D();
      Vector2 rightInput = _rightController.GetRelativePosition2D();

      // do appropriate movement based on input type 
      switch (classifyInput(leftInput, rightInput))
      {
        case InputType.JUMP:
          // apply impulse in y direction to jump
          _playerBody.AddRelativeForce(0, _jumpForce, 0, ForceMode.Impulse);
          break;

        case InputType.MOVE:
          Vector2 force = (_leftController.GetControlInput() + _rightController.GetControlInput()) / 2 * _maxForce;
          _playerBody.AddRelativeForce(force.x, 0, force.y, ForceMode.Force);
          break;

        case InputType.ROTATE:
          float torque = (_leftController.GetControlInput().y - _rightController.GetControlInput().y) / 2 * _maxTorque;
          _playerBody.AddRelativeTorque(0, torque, 0, ForceMode.Force);
          break;

        case InputType.NOINPUT:
          break;
      }

      // limit speed
      _playerBody.velocity = _playerBody.velocity.magnitude > _maxSpeed ? _playerBody.velocity.normalized * _maxSpeed : _playerBody.velocity;
    }
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
      if (Vector2.Dot(leftInput, rightInput) > 0)
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
    float length = 0.2f;
    // layer 8 should be just the floor so i don't hit the player or something 
    // it uses a bitmask so somehow this does what i want, i don't understand it at all
    return Physics.Raycast(position, -_collider.transform.up, length, 1 << 8);
  }
}