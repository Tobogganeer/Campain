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

    private Weapon weapon;

    public Weapon primary;
    public Weapon secondary;

    public static WeaponType CurrentWeaponType => CurrentWeapon.type;
    public static Weapon CurrentWeapon => instance.weapon;

    public static bool InADS => WeaponSway.IsInADS;


    private void Start()
    {
        weapon = primary;
        SwapWeapons(primary);
    }

    private void Update()
    {
        if (PlayerInputs.SwapWeapons)
            SwapWeapons();
    }

    private void SwapWeapons()
    {
        if (weapon == primary)
            SwapWeapons(secondary);
        else
            SwapWeapons(primary);
    }

    private void SwapWeapons(Weapon weapon)
    {
        primary.gameObject.SetActive(false);
        secondary.gameObject.SetActive(false);

        this.weapon = weapon;
        weapon.gameObject.SetActive(true);
        WeaponSway.instance.temp_weaponRotBone = weapon.swayBone.transform;
    }
}
