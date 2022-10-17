using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampedVectorTest : MonoBehaviour
{
    public Transform follower;
    private Functions.DampedVector vector;

    [Range(0f, 15f)]
    public float f = 5f;
    [Range(0f, 5f)]
    public float z = 1f;
    [Range(-5f, 5f)]
    public float r = 0f;

    float oldF, oldZ, oldR;

    private void Start()
    {
        vector = new Functions.DampedVector(f, z, r, transform.position);
        oldF = f;
        oldZ = z;
        oldR = r;
    }

    private void Update()
    {
        if (oldF != f || oldZ != z || oldR != r)
        {
            vector = new Functions.DampedVector(f, z, r, transform.position);
            oldF = f;
            oldZ = z;
            oldR = r;
        }

        follower.position = vector.Update(Time.deltaTime, transform.position, null);
    }
}
