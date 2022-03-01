using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int target = 144;

    void Start()
    {
        Application.targetFrameRate = target;
        QualitySettings.vSyncCount = 0;
    }
}
