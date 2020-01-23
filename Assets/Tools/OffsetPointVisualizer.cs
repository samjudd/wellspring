using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetPointVisualizer : MonoBehaviour
{
    [SerializeField]
    Transform _toVisualize;
    [SerializeField]
    Vector3 _offset;

    // Update is called once per frame
    void Update()
    {
        transform.SetPositionAndRotation(_toVisualize.position + _offset, _toVisualize.rotation);
    }
}
