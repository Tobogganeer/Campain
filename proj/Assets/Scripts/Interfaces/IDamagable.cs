using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(DamageDetails details);
}

public class DamageDetails
{
    public float amount;
    public DamageSource source;

    public Vector3 origin;
    public Vector3 direction;
    public WeaponType weaponType;
    public GameObject hitObj;

    public DamageDetails(float amount, DamageSource source, Vector3 direction, WeaponType weaponType)
    {
        this.amount = amount;
        this.source = source;
        this.direction = direction;
        this.weaponType = weaponType;
    }

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

public enum DamageSource
{
    None,
    World,
    Bullet,
    Shrapnel,
    Explosion
}
