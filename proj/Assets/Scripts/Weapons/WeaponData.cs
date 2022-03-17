using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public WeaponType type;

    [Space]
    public bool fullAuto = true;

    [Space]
    public float speedMult = 1;
    public float jumpMult = 1;

    [Space]
    public float adsMoveSpeedMult = 0.75f;
    public float adsSpeed = 1f;
    public float bobSpeedMult = 1;
    public float bobAmountMult = 1;

    [Space]
    public MilkShake.ShakePreset camShakePreset;
    public MilkShake.ShakePreset aimedCamShakePreset;
    public MilkShake.ShakePreset weaponShakePreset;
    public MilkShake.ShakePreset aimedWeaponShakePreset;

    [Space]
    public int fireRateRPM = 600;

    [Space]
    public float crouchRecoilMult = 0.7f;

    [Space]
    public WeaponSway.CrouchOffsets crouchOffsets;

    [Space]
    public Bullet.BulletBallistics ballistics;
    public float maxRange;
    public float baseDamage;
    public AnimationCurve damageFalloff;
    public AccuracyProfile accuracyProfile;
    public float rbForce;
    [Range(0, 1)]
    public float armourPenetration;
    public Hitbox.DamageRegions hitboxDamageMultipliers;


    [Space]
    public Attachments attachments;

    private void OnValidate()
    {
        attachments.Inspector_AssignNames();
    }
}
