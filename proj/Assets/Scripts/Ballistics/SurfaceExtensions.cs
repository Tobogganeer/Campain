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

    public static float GetHardness(this Surface surface)
    {
        if (surface == null) return SurfaceHardnesses.DEFAULT_HARDNESS;

        return SurfaceHardnesses.GetHardness(surface.surfaceType);
    }

    public static SurfaceFootstepType GetFootstepType(this Surface surface)
    {
        if (surface == null) return SurfaceFootstepType.DefaultConcrete;

        return GetFootstepType(surface.surfaceType);
    }
    
    public static SurfaceFootstepType GetFootstepType(this SurfaceType fromType)
    {
        switch (fromType)
        {
            case SurfaceType.None:
                return SurfaceFootstepType.None;
            case SurfaceType.Default:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Item:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Ladder:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Concrete:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Brick:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Rock:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Barrel:
                return SurfaceFootstepType.MetalHollow;
            case SurfaceType.Chainlink:
                return SurfaceFootstepType.MetalChainlink;
            case SurfaceType.Metal:
                return SurfaceFootstepType.Metal;
            case SurfaceType.MetalBox:
                return SurfaceFootstepType.Metal;
            case SurfaceType.MetalGrate:
                return SurfaceFootstepType.MetalGrate;
            case SurfaceType.MetalPanel:
                return SurfaceFootstepType.Metal;
            case SurfaceType.MetalVent:
                return SurfaceFootstepType.MetalHollow;
            case SurfaceType.MetalVehicle:
                return SurfaceFootstepType.MetalHollow;
            case SurfaceType.MetalSmallProp:
                return SurfaceFootstepType.Metal;
            case SurfaceType.Wood:
                return SurfaceFootstepType.Wood;
            case SurfaceType.WoodCrate:
                return SurfaceFootstepType.WoodCrate;
            case SurfaceType.WoodPlank:
                return SurfaceFootstepType.Wood;
            case SurfaceType.WoodPanel:
                return SurfaceFootstepType.Wood;
            case SurfaceType.WoodSolid:
                return SurfaceFootstepType.Wood;
            case SurfaceType.Dirt:
                return SurfaceFootstepType.Dirt;
            case SurfaceType.Grass:
                return SurfaceFootstepType.Grass;
            case SurfaceType.Gravel:
                return SurfaceFootstepType.Dirt;
            case SurfaceType.Mud:
                return SurfaceFootstepType.Mud;
            case SurfaceType.Sand:
                return SurfaceFootstepType.Dirt;
            case SurfaceType.Water:
                return SurfaceFootstepType.Water;
            case SurfaceType.WaterWade:
                return SurfaceFootstepType.Water;
            case SurfaceType.WaterPuddle:
                return SurfaceFootstepType.Water;
            case SurfaceType.Ice:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Snow:
                return SurfaceFootstepType.Snow;
            case SurfaceType.Foliage:
                return SurfaceFootstepType.Grass;
            case SurfaceType.Flesh:
                return SurfaceFootstepType.Grass;
            case SurfaceType.Asphalt:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Glass:
                return SurfaceFootstepType.GlassTile;
            case SurfaceType.Tile:
                return SurfaceFootstepType.GlassTile;
            case SurfaceType.Paper:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Cardboard:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Plaster:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Plastic:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.PlasticBarrel:
                return SurfaceFootstepType.Rubber;
            case SurfaceType.Rubber:
                return SurfaceFootstepType.Rubber;
            case SurfaceType.Tire:
                return SurfaceFootstepType.Rubber;
            case SurfaceType.Carpet:
                return SurfaceFootstepType.Rubber;
            case SurfaceType.Ceiling:
                return SurfaceFootstepType.DefaultConcrete;
            case SurfaceType.Pottery:
                return SurfaceFootstepType.GlassTile;
            default:
                return SurfaceFootstepType.DefaultConcrete;
        }
    }

    public static void SpawnFX(this Surface surface, Vector3 point, Vector3 normal)
    {
        if (surface == null) return;

        SurfaceFX.SpawnFX(surface.surfaceType, point + normal * 0.01f, Quaternion.LookRotation(normal, Vector3.up), surface.transform);
    }

    public static void SpawnFX(this Surface surface, Vector3 position, Quaternion rotation)
    {
        if (surface == null) return;

        SurfaceFX.SpawnFX(surface.surfaceType, position, rotation, surface.transform);
    }

    public static void PlayFootstep(this Surface surface, Vector3 position, Foot foot)
    {
        SurfaceFX.PlayFootstepSound(surface.GetFootstepType(), position, foot);
    }

    public static void PlayFootstep(this SurfaceType surfaceType, Vector3 position, Foot foot)
    {
        SurfaceFX.PlayFootstepSound(surfaceType.GetFootstepType(), position, foot);
    }

    public static void PlayHitSound(this Surface surface, Vector3 position, Transform parent = null, float maxDistance = 10, float volume = 1, float minPitch = 0.85f, float maxPitch = 1.10f)
    {
        SurfaceFX.PlayHitSound(surface.surfaceType, position, parent, maxDistance, volume, minPitch, maxPitch);
    }

    public static void PlayHitSound(this SurfaceType surfaceType, Vector3 position, Transform parent = null, float maxDistance = 10, float volume = 1, float minPitch = 0.85f, float maxPitch = 1.10f)
    {
        SurfaceFX.PlayHitSound(surfaceType, position, parent, maxDistance, volume, minPitch, maxPitch);
    }

    //public static void PlayAudio(this Surface surface, Vector3 position)
}
