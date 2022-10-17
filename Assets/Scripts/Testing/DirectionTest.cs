using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionTest : MonoBehaviour
{
    public Vector3 direction;
    public float randomness = 0.1f;

    public bool toggleLol;
    bool oldToggle;

    private void OnValidate()
    {
        if (toggleLol != oldToggle)
        {
            UpdateDir();
            oldToggle = toggleLol;
        }
    }

    private void UpdateDir()
    {
        direction.Normalize();
        direction += Random.insideUnitSphere * randomness;
        direction.Normalize();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + direction * 5);
    }
}
