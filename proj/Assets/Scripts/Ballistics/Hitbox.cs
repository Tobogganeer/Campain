using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour//, IBulletDamagable, IExplosionDamagable
{
    /*
    public HitboxRegion region;
    public Transform damageReceiver;
    private IDamagable damagable;

    private void Start()
    {
        damagable = damageReceiver.GetComponent<IDamagable>();
    }

    public static float GetBulletDamageMultiplier(DamageDetails details, HitboxRegion region)
    {
        DamageRegions regions = Weapons.GetProfile(details.weaponType).HitboxDamageMultipliers;

        switch (region)
        {
            case HitboxRegion.Chest:
                return regions.chest;
            case HitboxRegion.Abdomen:
                return regions.abdomen;
            case HitboxRegion.Arms:
                return regions.arms;
            case HitboxRegion.Legs:
                return regions.legs;
            case HitboxRegion.Head:
                return regions.head;
            default:
                return regions.chest;
        }
    }

    public float GetExplosiveDamageMultiplier()
    {
        return GetExplosiveDamageMultiplier(region);
    }

    public static float GetExplosiveDamageMultiplier(HitboxRegion region)
    {
        switch (region)
        {
            case HitboxRegion.Chest:
                return 0.4f;
            case HitboxRegion.Abdomen:
                return 0.15f;
            case HitboxRegion.Arms:
                return 0.05f;
            case HitboxRegion.Legs:
                return 0.05f;
            case HitboxRegion.Head:
                return 0.7f;
        }

        // Explosion will likely hit all body parts
        // 0.7 + 0.4 + 0.15 + (0.05 * 10)
        // A lot

        return 0.3f;
    }


    public void TakeBulletDamage(DamageDetails details)
    {
        details.amount *= GetBulletDamageMultiplier(details, region);
        damagable.TakeDamage(details);
    }

    public void TakeExplosiveDamage(DamageDetails details)
    {
        details.amount *= GetExplosiveDamageMultiplier();
        damagable.TakeDamage(details);
    }
    */

    [System.Serializable]
    public class DamageRegions
    {
        public float head = 3.5f;
        public float chest = 1.0f;
        public float abdomen = 1.25f;
        public float arms = 0.65f;
        public float legs = 0.45f;
    }
}
