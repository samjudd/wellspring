using UnityEngine;

public class WSCubeOfDoom : MonoBehaviour
{
  public float _flashTime = 0.1f;
  //adjust this to change speed
  public float _bobSpeed = 5f;
  //adjust this to change how high it goes
  public float _bobHeight = 0.5f;
  public float _health = 100.0f;
  private MeshRenderer _meshRenderer;
  private Rigidbody _rigidBody;
  private Color _originalColor;
  private Vector3 _originalPosition;

  void Start()
  {
    _meshRenderer = GetComponent<MeshRenderer>();
    _rigidBody = GetComponent<Rigidbody>();
    _originalPosition = transform.position;
    _originalColor = _meshRenderer.material.color;
  }

  void Update()
  {
    //calculate what the new Y position will be
    float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
    //calculate new position and move to it
    Vector3 newPosition = new Vector3(_originalPosition.x, _originalPosition.y + yOffset, _originalPosition.z);
    _rigidBody.MovePosition(newPosition);
  }

  public void Damage(float damageValue)
  {
    _health -= damageValue;
    _meshRenderer.material.color = Color.red;
    Debug.Log("WSCubeOfDoom " + damageValue.ToString() + " damage taken by " + name + ".");
    if (_health <= 0.0f)
      Destroy(gameObject);
    else
      Invoke("ResetColor", _flashTime);
  }

  private void ResetColor()
  {
    _meshRenderer.material.color = _originalColor;
  }
}