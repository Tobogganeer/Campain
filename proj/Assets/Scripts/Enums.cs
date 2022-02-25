
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
    UIHover,
    UIClick,
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
    P3K,
    Nateva,
    FNAL,
    Molkor,
    GR3_N,
    XRM
}