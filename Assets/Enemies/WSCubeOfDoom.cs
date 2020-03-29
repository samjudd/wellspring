using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSCubeOfDoom : MonoBehaviour
{
  public float _flashTime = 0.1f;
  //adjust this to change speed
  private float _speed = 5f;
  //adjust this to change how high it goes
  private float _height = 0.5f;
  private float _health = 100.0f;
  private MeshRenderer _meshRenderer;
  private Color _originalColor;

  void Start()
  {
    _meshRenderer = GetComponent<MeshRenderer>();
  }

  void Update()
  {
    //get the objects current position and put it in a variable so we can access it later with less code
    Vector3 pos = transform.position;
    //calculate what the new Y position will be
    float newY = Mathf.Sin(Time.time * _speed);
    //set the object's Y to the new calculated Y
    transform.position = new Vector3(pos.x, newY, pos.z) * _height;
  }

  public void Damage(float damageValue)
  {
    _health -= damageValue;
    _meshRenderer.material.color = Color.red;
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