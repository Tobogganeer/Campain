using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QualityManager
{
    public static void SetQualityLevel(GraphicsQualityLevels level)
    {
        QualitySettings.SetQualityLevel((int)level);
    }

    public static GraphicsQualityLevels GetCurrentQualityLevel()
    {
        return (GraphicsQualityLevels)QualitySettings.GetQualityLevel();
    }

    public static void SetMaxFramerate(int maxFramerate)
    {
        Application.targetFrameRate = maxFramerate;
    }

    public static void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 2 : 0;
    }
}
