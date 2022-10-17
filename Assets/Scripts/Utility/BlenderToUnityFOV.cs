using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlenderToUnityFOV : MonoBehaviour
{
    public float bFOV;
    public float uFOV;
    public Camera cam;

    private void OnValidate()
    {
        uFOV = GetUFov(cam, bFOV);
    }

    public static float GetUFov(Camera cam, float bFOV) // bFOV: Horizontal FOV from Blender
    {
        float uFOV = Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * bFOV / 2) / cam.aspect) * 2;
        uFOV *= Mathf.Rad2Deg;
        //cam.fieldOfView = uFOV;
        return uFOV;
    }
}
