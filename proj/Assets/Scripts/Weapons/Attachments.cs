using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attachments
{
    [Header("Ensure arrays are in enum order")]
    [SerializeField] private Barrel[] barrels;
    [SerializeField] private Underbarrel[] underbarrels;
    [SerializeField] private Sight[] sights;

    public Barrel Get(BarrelType type)
    {
        return barrels[(int)type];
    }

    public Underbarrel Get(UnderbarrelType type)
    {
        return underbarrels[(int)type];
    }

    public Sight Get(SightType type)
    {
        return sights[(int)type];
    }


    public void Inspector_AssignNames()
    {
        // Not just using fixed size loop in case of more attachments later

        for (int i = 0; i < barrels.Length; i++)
        {
            barrels[i].name = barrels[i].type.ToString();
        }

        for (int i = 0; i < underbarrels.Length; i++)
        {
            underbarrels[i].name = underbarrels[i].type.ToString();
        }

        for (int i = 0; i < sights.Length; i++)
        {
            sights[i].name = sights[i].type.ToString();
        }
    }
}



[System.Serializable]
public class Barrel
{
    [SerializeField, HideInInspector]
    public string name;

    public AudioArray shootSound;
    public float shootVolume = 1f;
    public float bulletVelocity;
    public Vector2 recoil;
    public float innaccuracyMult;

    public BarrelType type;
}

[System.Serializable]
public class Underbarrel
{
    [SerializeField, HideInInspector]
    public string name;

    public UnderbarrelType type;
}

[System.Serializable]
public class Sight
{
    [SerializeField, HideInInspector]
    public string name;

    public Vector3 aimPos;
    public float fovMult = 0.8f;
    public float recoilMult = 0.8f;

    public SightType type;
}
