using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceEffect : MonoBehaviour
{
    public SurfaceType type;
    //public VisualEffect vfx;
    //public new AudioArray audio;
    public float destroyAfter = 5;

    public VisualEffect[] vfxToSpawn;

    private void Start()
    {
        if (destroyAfter > 0)
            Destroy(gameObject, destroyAfter);

        foreach (VisualEffect effect in vfxToSpawn)
        {
            FX.SpawnVisualEffect(effect, transform.position, transform.rotation, transform);
        }
    }
}
