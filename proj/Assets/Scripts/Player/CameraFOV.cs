using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class CameraFOV : MonoBehaviour
{
    public Camera[] cameras;
    //public Camera cam;
    public RenderObjects viewmodels;
    private float[] fovs;
    private float vmFov;
    //private static float desiredFOV;
    private static float desiredMultiplier = 1;
    public float transitionSpeed = 5;
    public float defaultVMFov = 45.5f;

    public static float FOVFactor { get; private set; }

    private void Start()
    {
        fovs = new float[cameras.Length];
        for (int i = 0; i < fovs.Length; i++)
        {
            fovs[i] = cameras[i].fieldOfView;
        }
        viewmodels.settings.cameraSettings.cameraFieldOfView = defaultVMFov;
        vmFov = viewmodels.settings.cameraSettings.cameraFieldOfView;
    }

    private void Update()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].fieldOfView = Mathf.Lerp(cameras[i].fieldOfView, fovs[i] * desiredMultiplier, Time.deltaTime * transitionSpeed);
        }
        viewmodels.settings.cameraSettings.cameraFieldOfView = Mathf.Lerp(viewmodels.settings.cameraSettings.cameraFieldOfView, vmFov * desiredMultiplier, Time.deltaTime * transitionSpeed);

        FOVFactor = cameras[0].fieldOfView / fovs[0];
    }

    //public static void Set(float newFOV)
    //{
    //    desiredFOV = newFOV;
    //}

    public static void Set(float multiplier)
    {
        desiredMultiplier = multiplier;
    }
}
