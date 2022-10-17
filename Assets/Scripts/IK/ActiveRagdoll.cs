using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRagdoll : MonoBehaviour
{
    public float height = 0.8f;
    public float force = 100;
    public float rotationBalance = 200;

    public Transform[] legRoots;
    public Transform[] legs;
    private Vector3[] legPositions;
    private float[] moveTimes;

    public float legRayDist = 0.8f;
    public float maxLegReach = 0.5f;
    public float legMoveSpeed = 3f;
    public float minMoveTime = 0.8f;
    public float legStationaryThreshold = 0.2f;
    public float legMaxDistance = 1f;
    public float velocityPrediction = 0.1f;
    public float verticalOffset = 1f;
    //public float balanceFactor = 1.25f;

    Rigidbody rb;

    int legsStationary;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        legPositions = new Vector3[legs.Length];
        moveTimes = new float[legs.Length];
        for (int i = 0; i < legs.Length; i++)
        {
            legPositions[i] = legRoots[i].position + Vector3.down * maxLegReach;
            moveTimes[i] = minMoveTime / i;
        }
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, height + 0.2f))
        {
            if (hit.distance <= height)
            {
                //float legsOnGround = legsStationary / (float)legs.Length;
                //float factor = Mathf.Lerp(0.3f, 1.1f, legsOnGround);
                float totalForce = force / (hit.distance / height) * Mathf.Clamp(rb.velocity.Flattened().sqrMagnitude * 0.5f, 1f, 3f);// * factor;
                totalForce = Mathf.Min(totalForce, force * 5);
                //totalForce /= Mathf.Clamp(-rb.velocity.y, 1f, 5f);
                rb.AddForce(Vector3.up * totalForce * Time.deltaTime);
            }
            rb.drag = 1.3f;
            //Debug.Log(hit.distance / height, balanceFactor);
            //rb.AddForce(Vector3.up * force * Time.deltaTime);
        }
        else
        {
            rb.drag = 0f;
        }

        var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
        rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * rotationBalance);

        UpdateLegs();
    }

    void UpdateLegs()
    {
        legsStationary = 0;

        for (int i = 0; i < legs.Length; i++)
        {
            if (Vector3.Distance(legs[i].position, legPositions[i]) < legStationaryThreshold)
                legsStationary++;
        }

        for (int i = 0; i < legs.Length; i++)
        {
            moveTimes[i] += Time.deltaTime;
            Vector3 vertOffset = Vector3.up * Mathf.Clamp01(Vector3.Distance(legs[i].position, legPositions[i])) * verticalOffset;
            legs[i].position = Vector3.Lerp(legs[i].position, legPositions[i] + vertOffset, Time.deltaTime * legMoveSpeed);

            if (Physics.Raycast(legRoots[i].position, Vector3.down, out RaycastHit hit, legRayDist))
            {
                Vector3 candidatePos = hit.point + Vector3.ClampMagnitude(rb.velocity.Flattened() * velocityPrediction, 1);
                //Debug.Log("VEL: " + Vector3.ClampMagnitude(rb.velocity.Flattened() * velocityPrediction, 1));
                bool canMove = legsStationary > 1 || (legsStationary == 1 && Vector3.Distance(legs[i].position, legPositions[i]) > legStationaryThreshold);
                float dist = Vector3.Distance(candidatePos, legPositions[i]);
                if (dist > legMaxDistance || ((dist > maxLegReach || moveTimes[i] > minMoveTime) && canMove))
                {
                    legPositions[i] = candidatePos;
                    moveTimes[i] = 0;
                }
            }
            else
                legPositions[i] = legRoots[i].position + Vector3.down * maxLegReach * Mathf.Clamp(-rb.velocity.y * 0.8f, 1, 2);
        }
    }
}
