using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Bullet Ballistics Profile")]
public class BulletBallistics : ScriptableObject
{
    [Header("Bounce")]
    [Tooltip("Hitting an object at less than this angle results in a bounce off.")]
    public AnimationCurve BounceChance;
    [Tooltip("Only bounce off of objects that are at least this hard.")]
    public float MinBounceHardness = SurfaceHardnesses.DEFAULT_HARDNESS;
    [Tooltip("How much speed the bullet loses on a bounce")]
    public AnimationCurve BounceVelocityLoss;
    [Tooltip("Amount of randomization applied to bounced angle. Multiplied with surface hardness.")]
    [Range(0, 1f)]
    public float BounceDirectionRandomization = 0.1f;

    [Space]
    [Header("Penetration")]
    [Tooltip("How much velocity is lost per unit penetrated. Multiplied with hardness of penetrated object.")]
    public float VelocityLossPerUnit = 100;
    [Tooltip("Cannot go through any material with a hardness higher than this value. " +
        "Multiplied with 0-1 thickness of potential hit, so shooting through corners will still work.")]
    public float MaxHardness = 3f;
    [Tooltip("Randomizes the exit direction of the bullet. Multiplied with surface hardness.")]
    [Range(0, 1f)]
    public float PenetrationDirectionRandomization = 0.1f;
}
