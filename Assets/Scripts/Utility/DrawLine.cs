using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public Color colour;
    public float length = 5;

    private void OnDrawGizmos()
    {
        Gizmos.color = colour;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * length);
    }
}
