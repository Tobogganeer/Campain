
public enum Foot : byte
{
    Left,
    Right
}

public enum Level : byte
{
    MainMenu,
    Game
}

public enum AudioArray : byte
{
    Null,
    RightFoot,
    LeftFoot,
    Drop,
    Slide,
    Jump,
    UIHover,
    UIClick,
    NP5_Shoot = 150,
}

public enum PooledObject
{
    AudioSource
}

public enum GraphicsQualityLevels : byte
{
    Low,
    Medium,
    High,
    Ultra
}

public enum GameStage : byte
{
    MainMenu,
    Lobby,
    Game
}

public enum WeaponType
{
    None,
    NP5
}

public enum VisualEffect
{
    None,
    Explosion,
    BloodSplatter
}



// COMPLETE


public enum BarrelType : byte // Shoot sound, bullet velocity, recoil
{
	Default,
	Suppressed,
	Compensator
}

public enum UnderbarrelType : byte // Nothing
{
    Nothing,
    Flashlight,
    Laser
}

public enum SightType : byte // Aim pos, fov change
{
    IronSights,
    Holo,
    Acog,
 }


public enum SurfaceType : byte
{
    None,
    Default,
    Item,
    Ladder,
    Concrete,
    Brick,
    Rock,
    Barrel,
    Chainlink,
    Metal,
    MetalBox,
    MetalGrate,
    MetalPanel,
    MetalVent,
    MetalVehicle,
    MetalSmallProp,
    Wood,
    WoodCrate,
    WoodPlank,
    WoodPanel,
    WoodSolid,
    Dirt,
    Grass,
    Gravel,
    Mud,
    Sand,
    Water,
    WaterWade,
    WaterPuddle,
    Ice,
    Snow,
    Foliage,
    Flesh,
    Asphalt,
    Glass,
    Tile,
    Paper,
    Cardboard,
    Plaster,
    Plastic,
    PlasticBarrel,
    Rubber,
    Tire,
    Carpet,
    Ceiling,
    Pottery
}

public enum SurfaceFootstepType : byte
{
    None,
    DefaultConcrete,
    Wood,
    WoodCrate,
    Rubber,
    Snow,
    Dirt,
    Grass,
    Mud,
    GlassTile,
    Metal,
    MetalGrate,
    MetalChainlink,
    MetalHollow,
    Water
}

public enum HitboxRegion
{
    Chest,
    Abdomen,
    Arms,
    Legs,
    Head
}