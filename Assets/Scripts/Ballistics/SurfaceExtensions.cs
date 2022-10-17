using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SurfaceExtensions
{
    // GetSurface() - transform, gameobject, monobehaviour
    // Surface extensions - GetHardness(), GetFootstepSound(), SpawnEffect()

    public static bool GetSurface(this Component comp, out Surface surface)
    {
        if (comp == null)
        {
            surface = null;
            return false;
        }

        return comp.TryGetComponent(out surface);
    }
    public static void SpawnFX(this Surface surface, Vector3 point, Vector3 normal)
    {
        if (surface == null) return;

        //SurfaceFX.SpawnFX(surface.surfaceType, point + normal * 0.01f, Quaternion.LookRotation(normal, Vector3.up), surface.transform);
    }

    public static void SpawnFX(this Surface surface, Vector3 position, Quaternion rotation)
    {
        if (surface == null) return;

        //SurfaceFX.SpawnFX(surface.surfaceType, position, rotation, surface.transform);
    }

    public static void PlayFootstep(this Surface surface, Vector3 position, Foot foot)
    {
        //SurfaceFX.PlayFootstepSound(surface.GetFootstepType(), position, foot);
    }

    public static void PlayFootstep(this SurfaceType surfaceType, Vector3 position, Foot foot)
    {
        //SurfaceFX.PlayFootstepSound(surfaceType.GetFootstepType(), position, foot);
    }

    public static void PlayHitSound(this Surface surface, Vector3 position, Audio audio)
    {
        //surface.surfaceType.PlayHitSound(position, audio);
        //SurfaceFX.PlayHitSound(surface.surfaceType, audio.SetPosition(position));
    }

    public static void PlayHitSound(this SurfaceType surfaceType, Vector3 position, Audio audio)
    {
        SurfaceFX.PlayHitSound(surfaceType, audio.SetPosition(position));
        //SurfaceFX.PlayHitSound(surfaceType, position, parent, maxDistance, volume, minPitch, maxPitch);
    }

    //public static void PlayAudio(this Surface surface, Vector3 position)
}
