using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Attachments
{
    [SerializeField] private SerializableDictionary<BarrelType, Barrel> barrels = new SerializableDictionary<BarrelType, Barrel>();
    [SerializeField] private SerializableDictionary<UnderbarrelType, Underbarrel> underbarrels = new SerializableDictionary<UnderbarrelType, Underbarrel>();
    [SerializeField] private SerializableDictionary<SightType, Sight> sights = new SerializableDictionary<SightType, Sight>();

    public int BarrelCount => barrels.Dict.Count;
    public int UnderbarrelCount => underbarrels.Dict.Count;
    public int SightCount => sights.Dict.Count;

    public Barrel Get(BarrelType type)
    {
        return barrels.Dict[type];
    }

    public Underbarrel Get(UnderbarrelType type)
    {
        return underbarrels.Dict[type];
    }

    public Sight Get(SightType type)
    {
        return sights.Dict[type];
    }


    public void AssignNamesAndTypes()
    {
        try
        {
            foreach (var item in barrels.values)
                item.name = item.key.ToString();
            foreach (var item in underbarrels.values)
                item.name = item.key.ToString();
            foreach (var item in sights.values)
                item.name = item.key.ToString();

            foreach (var item in barrels.Dict)
                if (item.Value != null)
                    item.Value.type = item.Key;
            foreach (var item in underbarrels.Dict)
                if (item.Value != null)
                    item.Value.type = item.Key;
            foreach (var item in sights.Dict)
                if (item.Value != null)
                    item.Value.type = item.Key;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Caught expected error (weird OnValidate thing): " + ex);
        }
    }
}



[System.Serializable]
public class Barrel
{
    [SerializeField, HideInInspector]
    public string name;

    public string shootSound;
    public float shootVolume = 1f;
    public float shootMechVolume = 1f;
    public float bulletVelocity;
    public Vector2 recoil;
    public float innaccuracyMult;
    public float damageMult = 1f;

    [SerializeField, HideInInspector]
    public BarrelType type;
}

[System.Serializable]
public class Underbarrel
{
    [SerializeField, HideInInspector]
    public string name;

    [SerializeField, HideInInspector]
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
    public float sensMult = 0.5f;
    public float adsBlur = 20;

    [SerializeField, HideInInspector]
    public SightType type;
}
