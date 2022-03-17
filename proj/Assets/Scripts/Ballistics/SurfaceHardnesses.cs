using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SurfaceHardnesses
{
    public const float DEFAULT_HARDNESS = 2.0f;
    public const float HARD_ROCK = 3.5f;
    public const float THIN_METAL = 1.8f;
    public const float THICK_METAL = 4.0f;
    public const float THIN_WOOD = 0.8f;
    public const float THICK_WOOD = 1.5f;
    public const float TERRAIN = 4.5f;
    public const float FLESH = 2.5f;
    public const float DRYWALL = 2.3f;
    public const float RUBBER = 3.3f;
    public const float GLASS_TILE = 1.3f;
    public const float WATER = 0.5f;
    public const float FOLIAGE = 0.2f;

    public static float GetHardness(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.None:
                return 0;
            case SurfaceType.Default:
                return DEFAULT_HARDNESS;

            case SurfaceType.Item:
                return DEFAULT_HARDNESS;

            case SurfaceType.Ladder:
                return DEFAULT_HARDNESS;

            case SurfaceType.Concrete:
                return HARD_ROCK;

            case SurfaceType.Brick:
                return HARD_ROCK;

            case SurfaceType.Rock:
                return HARD_ROCK;

            case SurfaceType.Barrel:
                return THICK_METAL;

            case SurfaceType.Chainlink:
                return THIN_METAL;

            case SurfaceType.Metal:
                return THICK_METAL;

            case SurfaceType.MetalBox:
                return THICK_METAL;

            case SurfaceType.MetalGrate:
                return THIN_METAL;

            case SurfaceType.MetalPanel:
                return THIN_METAL;

            case SurfaceType.MetalVent:
                return THIN_METAL;

            case SurfaceType.MetalVehicle:
                return THICK_METAL;

            case SurfaceType.MetalSmallProp:
                return THIN_METAL;

            case SurfaceType.Wood:
                return THICK_WOOD;

            case SurfaceType.WoodCrate:
                return THICK_WOOD;

            case SurfaceType.WoodPlank:
                return THIN_WOOD;

            case SurfaceType.WoodPanel:
                return THIN_WOOD;

            case SurfaceType.WoodSolid:
                return THICK_WOOD;

            case SurfaceType.Dirt:
                return TERRAIN;

            case SurfaceType.Grass:
                return TERRAIN;

            case SurfaceType.Gravel:
                return TERRAIN;

            case SurfaceType.Mud:
                return TERRAIN;

            case SurfaceType.Sand:
                return TERRAIN;

            case SurfaceType.Water:
                return WATER;

            case SurfaceType.WaterWade:
                return WATER;

            case SurfaceType.WaterPuddle:
                return WATER;

            case SurfaceType.Ice:
                return TERRAIN;

            case SurfaceType.Snow:
                return TERRAIN;

            case SurfaceType.Foliage:
                return FOLIAGE;

            case SurfaceType.Flesh:
                return FLESH;

            case SurfaceType.Asphalt:
                return HARD_ROCK;

            case SurfaceType.Glass:
                return GLASS_TILE;

            case SurfaceType.Tile:
                return GLASS_TILE;

            case SurfaceType.Paper:
                return THIN_WOOD;

            case SurfaceType.Cardboard:
                return THIN_WOOD;

            case SurfaceType.Plaster:
                return DRYWALL;

            case SurfaceType.Plastic:
                return RUBBER;

            case SurfaceType.PlasticBarrel:
                return RUBBER;

            case SurfaceType.Rubber:
                return RUBBER;

            case SurfaceType.Tire:
                return RUBBER;

            case SurfaceType.Carpet:
                return DRYWALL;

            case SurfaceType.Ceiling:
                return DRYWALL;

            case SurfaceType.Pottery:
                return GLASS_TILE;

            default:
                return DEFAULT_HARDNESS;
        }
    }
}
