using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelTest : MonoBehaviour
{
    private Rigidbody rb;

    public Vector3 rotation;
    public Vector3 angularVelocity;
    public Vector3 rbVelocity;
    private Quaternion lastRotation;

    public bool rotate = true;
    bool oldRotate;

    bool change => oldRotate != rotate;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Quaternion q = transform.rotation * Quaternion.Inverse(lastRotation);
        //Quaternion q = lastRotation * Quaternion.Inverse(transform.rotation);
        Vector3 axis;
        float angle;
        q.ToAngleAxis(out angle, out axis);

        angularVelocity = axis * (angle / Time.deltaTime);

        lastRotation = transform.rotation;

        if (rotate)
        {
            transform.Rotate(rotation * Time.deltaTime);
            if (change)
            {
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.None;
            }
        }
        else
        {
            if (change)
            {
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.angularVelocity = angularVelocity * Mathf.Deg2Rad;
            }
        }

        //rbVelocity = rb.angularVelocity;

        oldRotate = rotate;
    }
}
