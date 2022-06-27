using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DisableVMSun : MonoBehaviour
{
    private Camera cam;

    public LensFlareComponentSRP lensFlare;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnStartRender;
        RenderPipelineManager.endCameraRendering += OnEndRender;
        cam = GetComponent<Camera>();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnStartRender;
        RenderPipelineManager.endCameraRendering -= OnEndRender;
    }

    private void OnStartRender(ScriptableRenderContext context, Camera camera)
    {
        lensFlare.enabled = camera != cam;
    }

    private void OnEndRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
            lensFlare.enabled = true;
    }
}
