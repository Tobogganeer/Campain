using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollItem : MonoBehaviour
{
    public Rigidbody rb;
    public new Collider collider;

    public void Drop(Vector3 force)
    {
        rb.isKinematic = false;
        collider.enabled = true;

        rb.AddForce(force, ForceMode.Impulse);
    }
}
