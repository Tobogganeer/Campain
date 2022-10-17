using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    public static Weapons instance;

    [SerializeField]
    private WeaponData[] weaponData;
    private static Dictionary<WeaponType, WeaponData> data = new Dictionary<WeaponType, WeaponData>();

    private void Awake()
    {
        if (instance == null) instance = this;

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        data.Clear();

        foreach (WeaponData profile in weaponData)
        {
            data.Add(profile.type, profile);
        }
    }

    public static WeaponData Get(WeaponType type)
    {
        return data[type];
    }
}
