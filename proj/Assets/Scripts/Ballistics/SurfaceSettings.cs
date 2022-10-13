using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Surface Settings")]
public class SurfaceSettings : ScriptableObject
{
    const float DefaultHardness = 2.0f;
    const string DefaultFootstep = "Foot"; // Left/Right appended
    const string DefaultHit = "DefaultHit"; // Left/Right appended

    [SerializeField] private float hardness = DefaultHardness;
    [Header("Sounds")]
    [SerializeField] private string footstepSound = DefaultFootstep;
    [SerializeField] private string hitSound = DefaultHit;

    public float Hardness => hardness;
    public string FootstepSound => footstepSound;
    public string HitSound => hitSound;

    private static SurfaceSettings _default;
    public static SurfaceSettings Default
    {
        get
        {
            if (_default == null)
                _default = CreateInstance<SurfaceSettings>();
            return _default;
        }
    }
}
