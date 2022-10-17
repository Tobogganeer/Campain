using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour
{
    public float speed = 75;
    //public LayerMask wallLayerMask;
    //public float maxDistance = 500;

    public void Init(Vector3 origin, Vector3 end)
    {
        Destroy(gameObject, Vector3.Distance(origin, end) / speed);

        //if (Physics.Raycast(shootFrom.origin, shootFrom.direction, out RaycastHit hit, maxDistance, wallLayerMask))
        //{
        //    Destroy(gameObject, Vector3.Distance(hit.point, transform.position) / speed);
        //    transform.rotation = Quaternion.LookRotation(transform.position.DirectionTo(hit.point), Vector3.up);
        //}
        //else
        //{
        //    Destroy(gameObject, maxDistance / speed);
        //    transform.rotation = Quaternion.LookRotation(transform.position.DirectionTo(shootFrom.direction * maxDistance), Vector3.up);
        //}
    }

    private void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }
}
