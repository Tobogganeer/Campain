using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;
    private Transform camTrans;
    public float rate = 0.01f;
    public float exp = 1.05f;
    private void Start()
    {
        cam = Camera.main;
        camTrans = cam.transform;
    }
    void LateUpdate()
    {
        transform.LookAt(transform.position + camTrans.forward);
        transform.localScale = Vector3.one * Mathf.Pow(rate * Vector3.Distance(transform.position, camTrans.position), exp) * CameraFOV.FOVFactor;
    }
}
