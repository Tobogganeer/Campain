using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;
    private void Awake()
    {
        instance = this;
    }

    public WeaponType weaponType;
    public Weapon weapon;

    public static WeaponType CurrentWeaponType => instance.weaponType;
    public static Weapon CurrentWeapon => instance.weapon;

    public static bool InADS => WeaponSway.IsInADS;
}
