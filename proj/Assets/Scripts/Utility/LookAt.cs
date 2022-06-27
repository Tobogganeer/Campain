using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class LookAt : MonoBehaviour
{
    public Transform target;

    //public LookAtAxis x;
    //public LookAtAxis y;
    //public LookAtAxis z;

    public float maxAngle;
    public float rotSpeed = 3;

    private Quaternion startingRot;
    private Quaternion desiredRot;

    private Quaternion lastRot;

    private void Start()
    {
        startingRot = transform.localRotation;
        lastRot = transform.localRotation;
        if (target == null)
            target = Camera.main.transform;
    }

    //private void LateUpdate()
    //{
    //    if (target == null) return;
    //
    //    if (!x.use && !y.use && !z.use) return;
    //
    //    Quaternion rot = transform.localRotation;
    //    transform.LookAt(target);
    //
    //    Quaternion desiredRot = transform.localRotation;
    //
    //    transform.localRotation = rot;
    //
    //    Vector3 desiredEulers = desiredRot.eulerAngles;
    //    Vector3 endEulers = rot.eulerAngles;
    //
    //    if (x.use) endEulers.x = Mathf.Clamp(desiredEulers.x, x.min, x.max);
    //    if (y.use) endEulers.y = Mathf.Clamp(desiredEulers.y, y.min, y.max);
    //    if (z.use) endEulers.z = Mathf.Clamp(desiredEulers.z, z.min, z.max);
    //
    //    transform.localRotation = Quaternion.Euler(endEulers);
    //}

    private void LateUpdate()
    {
        SetRotation();

        //transform.localRotation = Quaternion.Slerp(transform.localRotation, desiredRot, Time.deltaTime * rotSpeed);
        //transform.localRotation = desiredRot;
        transform.localRotation = Quaternion.Slerp(lastRot, desiredRot, Time.deltaTime * rotSpeed);

        lastRot = transform.localRotation;
    }

    private void SetRotation()
    {
        if (target == null)
        {
            this.desiredRot = startingRot;
            return;
        }

        //Quaternion rot = transform.localRotation;
        //transform.LookAt(target);
        //
        //Quaternion desiredRot = transform.localRotation;
        //
        //transform.localRotation = rot;

        //if (Vector3.Angle(transform.forward, desiredRot.eulerAngles) > maxAngle)


        Quaternion rot = transform.localRotation;

        transform.localRotation = startingRot;

        if (Vector3.Angle(transform.forward, transform.position.DirectionTo(target.position)) > maxAngle)
        {
            this.desiredRot = startingRot;
            return;
        }

        //transform.rotation = rot;

        transform.LookAt(target);
        
        desiredRot = transform.localRotation;
        
        transform.localRotation = rot;

        //this.desiredRot = desiredRot;
    }

    //[System.Serializable]
    //public struct LookAtAxis
    //{
    //    public bool use;
    //    public float min;
    //    public float max;
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5);

        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.position.DirectionTo(target.position) * 5);
    }
}
