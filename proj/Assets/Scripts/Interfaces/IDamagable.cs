using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(DamageDetails details);
}

public struct DamageDetails
{
    public float amount;
    public Vector3 origin;
    public Vector3 direction;
    public WeaponType weaponType;

    public DamageDetails(float amount, Vector3 origin, Vector3 direction, WeaponType weaponType)
    {
        this.amount = amount;
        this.origin = origin;
        this.direction = direction;
        this.weaponType = weaponType;
    }

    public static DamageDetails NoDir(float amount, WeaponType weaponType)
    {
        return new DamageDetails(amount, Vector3.zero, Vector3.zero, weaponType);
    }
}
