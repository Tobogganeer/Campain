using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    public SurfaceSettings settings;

    public float Hardness => settings.Hardness;
    public string FootstepSound => settings.FootstepSound;
    public string HitSound => settings.HitSound;
}
