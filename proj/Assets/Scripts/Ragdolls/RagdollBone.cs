using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollBone : MonoBehaviour
{
    private Rigidbody rb;
    //private new Collider collider;

    private Vector3 velocity;
    public Vector3 angularVelocity;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    float twitchTime;
    float twitchSpeed;
    Vector3 twitchDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.None;
        lastRotation = transform.rotation;
        lastPosition = transform.position;
        //collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        //velocity = (lastPosition - transform.position) / Time.deltaTime;
        velocity = (transform.position - lastPosition) / Time.deltaTime;

        Quaternion q = transform.rotation * Quaternion.Inverse(lastRotation);
        //Quaternion q = lastRotation * Quaternion.Inverse(transform.rotation);
        Vector3 axis;
        float angle;
        q.ToAngleAxis(out angle, out axis);

        angularVelocity = axis * (angle / Time.deltaTime);

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        ApplyTwitch();
    }

    private void ApplyTwitch()
    {
        twitchTime -= Time.deltaTime;

        if (twitchTime > 0 && !rb.isKinematic)
        {
            float time = twitchSpeed * twitchTime;
            rb.AddForce(twitchDir * (1 - Mathf.Abs(Mathf.Sin(time))), ForceMode.Force);
        }
    }


    const float MinRandom = 0.8f;
    const float MaxRandom = 1.2f;

    public void Enable()
    {
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        //collider.enabled = true;

        rb.velocity = velocity * Random.Range(MinRandom, MaxRandom);
        rb.angularVelocity = angularVelocity * Mathf.Deg2Rad * Random.Range(MinRandom, MaxRandom);
    }

    public void Disable()
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        //collider.enabled = false;
    }

    public void SetState(bool active)
    {
        if (active)
            Enable();
        else
            Disable();
    }

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        rb.AddForce(force, forceMode);
    }

    public void DeathTwitch(float maxTime, float force)
    {
        twitchTime = Random.Range(0, maxTime);
        twitchSpeed = Random.Range(0.35f, 1.3f); // Per second
        twitchDir = Random.onUnitSphere * Random.Range(0, force) * twitchSpeed;
    }
}