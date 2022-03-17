using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballistics
{
    //const int BUFFER_SIZE = 64;
    //
    //private static RaycastHit[] forwardBuffer = new RaycastHit[BUFFER_SIZE];
    //private static RaycastHit[] backwardBuffer = new RaycastHit[BUFFER_SIZE];

    public static BallisticsResult Cast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, BallisticsSettings settings)
    {
        direction.Normalize();

        return Cast_ColliderRaycast(origin, direction, maxDistance, layerMask, settings);
    }

    private static BallisticsResult Cast_ColliderRaycast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, BallisticsSettings settings)
    {
        RaycastHit frontHit;

        if (Physics.Raycast(origin, direction, out frontHit, maxDistance, layerMask))
        {
            // Hit something
            List<PenetrationData> allPenetrationData = new List<PenetrationData>();
            float currentFalloff = settings.falloff;

            RaycastHit backHit;
            float maxPenDepth = settings.GetMaxPenetrationDepth();
            Ray backRay = new Ray(frontHit.point + direction * maxPenDepth, -direction);
            if (frontHit.collider.Raycast(backRay, out backHit, maxPenDepth))
            {
                // Bullet exited wall
                PenetrationData penData = new PenetrationData();
                penData.penetratedCollider = frontHit.collider;
                penData.AddEntry(frontHit.point, frontHit.normal, currentFalloff);
                penData.AddExit(backHit.point, backHit.normal, currentFalloff);

                currentFalloff = settings.CalculateFalloff(currentFalloff, penData.penetrationThickness, backHit.collider);
                penData.exitFalloff = currentFalloff;

                allPenetrationData.Add(penData);


                byte numPenetrations = (byte)(settings.maxPenetrations - 1); // Already penetrated once wall
                Vector3 frontWallOfLastHit = backHit.point;
                float currentDistanceTravelled = frontHit.distance + penData.penetrationThickness;

                for (byte i = 0; i < numPenetrations; i++)
                {
                    if (currentFalloff <= 0) break;

                    frontWallOfLastHit = backHit.point;
                    currentDistanceTravelled = frontHit.distance + penData.penetrationThickness;

                    if (Physics.Raycast(frontWallOfLastHit, direction, out frontHit, maxDistance - currentDistanceTravelled, layerMask))
                    {
                        // Hit something again
                        backRay = new Ray(frontHit.point + direction * maxPenDepth, -direction);
                        if (frontHit.collider.Raycast(backRay, out backHit, maxPenDepth))
                        {
                            // Bullet exited wall again
                            penData = new PenetrationData();
                            penData.penetratedCollider = frontHit.collider;
                            penData.AddEntry(frontHit.point, frontHit.normal, currentFalloff);
                            penData.AddExit(backHit.point, backHit.normal, currentFalloff);

                            currentFalloff = settings.CalculateFalloff(currentFalloff, penData.penetrationThickness, backHit.collider);
                            penData.exitFalloff = currentFalloff;

                            allPenetrationData.Add(penData);

                            //currentPoint = backHit.point;
                        }
                        else
                        {
                            // Bullet ended in wall
                            penData = new PenetrationData();
                            penData.penetratedCollider = frontHit.collider;
                            penData.AddEntry(frontHit.point, frontHit.normal, currentFalloff);

                            allPenetrationData.Add(penData);
                            break;
                            //return BallisticsResult.FromData(allPenetrationData);
                        }
                    }
                    else
                    {
                        // No more things to hit
                        break;
                        //return BallisticsResult.FromData(allPenetrationData);
                    }
                }

                // Done penetrating things
                return BallisticsResult.FromData(allPenetrationData, origin, direction);
            }
            else
            {
                // Bullet ended in wall
                PenetrationData penData = new PenetrationData();
                penData.penetratedCollider = frontHit.collider;
                penData.AddEntry(frontHit.point, frontHit.normal, currentFalloff);

                allPenetrationData.Add(penData);
                return BallisticsResult.FromData(allPenetrationData, origin, direction);
            }
        }

        return BallisticsResult.Empty();
    }

    public static DamageResult ApplyDamage(BallisticsResult result, float damage, float rbForce, WeaponType weaponType)
    {
        bool appliedDamage = false;

        // Method assumes result.penetrations is not null and has > 0 values
        for (int i = 0; i < result.penetrations.Length; i++)
        {
            PenetrationData data = result.penetrations[i];
            float calibratedDamage = damage * data.entryFalloff;

            //if (data.penetratedCollider.TryGetComponent(out RangedAI ai))
            //{
            //    ai.TakeDamage(calibratedDamage);
            //    Debug.Log("Dealt damage: " + calibratedDamage);
            //}

            if (data.penetratedCollider.TryGetComponent(out IBulletDamagable damagable))
            {
                damagable.TakeBulletDamage(new DamageDetails(calibratedDamage, result.origin, result.direction, weaponType));
                appliedDamage = true;
                //Draw.DrawText(data.entryPoint, calibratedDamage.ToString(), Color.blue, 24, 1f, true);
            }

            if (data.penetratedCollider.attachedRigidbody != null)
            {
                data.penetratedCollider.attachedRigidbody.AddForceAtPosition(-data.entryNormal * rbForce, data.entryPoint, ForceMode.Impulse);
            }

            // Raycasts dont hit ai layer
            //if (data.penetratedCollider.TryGetComponent(out RangedAI ai))
            //{
            //    rbForce *= 15;
            //    //ai.AddForceToRagdoll(-data.entryNormal * rbForce, ForceMode.Impulse);
            //    ai.AddForceToRagdoll(result.direction * rbForce, ForceMode.Impulse);
            //}
            // Maybe change to GetComponents<>()?
        }

        return new DamageResult(appliedDamage);
    }
    
    public static void DrawResult(BallisticsResult result, float duration)
    {
        Color start = Color.green;
        Color air = Color.yellow;
        Color material = Color.red;
        Color endTrail = new Color(0.4f, 0f, 0.8f);

        if (!result.empty)
        {
            Draw.DrawLine(result.origin, result.penetrations[0].entryPoint, start, duration, false);
            //Gizmos.color = start;
            //Gizmos.DrawLine(result.origin, result.penetrations[0].entryPoint);

            for (int i = 0; i < result.penetrations.Length; i++)
            {
                if (result.penetrations[i].exited)
                {
                    //Gizmos.color = material;
                    //Gizmos.DrawLine(result.penetrations[i].entryPoint, result.penetrations[i].exitPoint);
                    Draw.DrawLine(result.penetrations[i].entryPoint, result.penetrations[i].exitPoint, material, duration, false);
                }

                if (i > 0)
                {
                    //Gizmos.color = air;
                    //Gizmos.DrawLine(result.penetrations[i - 1].exitPoint, result.penetrations[i].entryPoint);
                    Draw.DrawLine(result.penetrations[i - 1].exitPoint, result.penetrations[i].entryPoint, air, duration, false);
                }
            }

            PenetrationData lastPen = result.lastPenetration;
            if (lastPen.exited && lastPen.exitFalloff > 0)
            {
                Draw.DrawLine(lastPen.exitPoint, lastPen.exitPoint + result.direction * 10, endTrail, duration, false);
            }
        }
    }

    public static Vector3 GetDirectionWithInnaccuracy(Vector3 dir, float maxAngle)
    {
        //Vector3 forwardVector = dir;
        //float deviation = Random.Range(0f, maxAngle);
        //float angle = Random.Range(0f, 360f);
        //forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
        //forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
        //forwardVector = fpsCam.transform.rotation * forwardVector;

        return (dir + Random.insideUnitSphere * maxAngle * 0.01f).normalized;

        //Vector2 inaccuracyVector = OptimizedRandom.insideUnitCircle * maxAngle;// * Player.Movement.;
        //Quaternion inaccuracyRotation = Quaternion.Euler(inaccuracyVector.x, inaccuracyVector.y, 0);
        //return inaccuracyRotation * dir;
    }

    public static void SpawnFX(BallisticsResult result)
    {
        if (result.empty) return;

        for (int i = 0; i < result.penetrations.Length; i++)
        {
            PenetrationData data = result.penetrations[i];

            if (data.entered && data.penetratedCollider.GetSurface(out Surface surface))
            {
                surface.SpawnFX(data.entryPoint, data.entryNormal);
            }

            if (data.exited && data.penetratedCollider.GetSurface(out surface))
            {
                surface.SpawnFX(data.exitPoint, data.exitNormal);
                // Will have hit one shots
                //surface.PlayHitSound(data.exitPoint)
            }
        }
    }

    //private static BallisticsResult Cast_RaycastNonAlloc(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, BallisticsSettings settings)
    //{
    //    int forwardHits = Physics.RaycastNonAlloc(origin, direction, forwardBuffer, maxDistance, layerMask);
    //    if (forwardHits == 0) return BallisticsResult.Empty();
    //
    //    int backwardHits = Physics.RaycastNonAlloc(origin + direction * maxDistance, -direction, backwardBuffer, maxDistance, layerMask);
    //
    //    return BallisticsResult.Empty();
    //}
}

//public struct BallisticsResult
public class BallisticsResult
{
    public Vector3 origin;
    public Vector3 direction;

    public bool empty => penetrations == null || penetrations.Length == 0;
    public PenetrationData[] penetrations;
    public PenetrationData lastPenetration => penetrations[penetrations.Length - 1];

    public static BallisticsResult Empty()
    {
        return new BallisticsResult();
    }

    public static BallisticsResult FromData(List<PenetrationData> data, Vector3 origin, Vector3 direction)
    {
        BallisticsResult result = new BallisticsResult();
        result.penetrations = data.ToArray();
        result.origin = origin;
        result.direction = direction;
        return result;
    }
}

public struct PenetrationData
{
    public Collider penetratedCollider;

    public bool entered;
    public Vector3 entryPoint;
    public Vector3 entryNormal;
    public float entryFalloff;

    public bool exited;
    public Vector3 exitPoint;
    public Vector3 exitNormal;
    public float exitFalloff;

    public float penetrationThickness;

    public void AddEntry(Vector3 point, Vector3 normal, float falloff)
    {
        entered = true;
        entryPoint = point;
        entryNormal = normal;
        entryFalloff = falloff;
    }

    public void AddExit(Vector3 point, Vector3 normal, float falloff)
    {
        exited = true;
        exitPoint = point;
        exitNormal = normal;
        exitFalloff = falloff;

        penetrationThickness = Vector3.Distance(entryPoint, exitPoint);
    }

    public override string ToString()
    {
        return $@"Penetration Data:
-Collider: {penetratedCollider.name}

-Entered: {entered}
-Entry point: {entryPoint}
-Entry normal: {entryNormal}
-Entry falloff: {entryFalloff}

-Exited: {exited}
-Exit point: {exitPoint}
-Exit normal: {exitNormal}
-Exit falloff: {exitFalloff}

-Thickness: {penetrationThickness}
";
    }
}

[System.Serializable]
public struct BallisticsSettings
{
    [Range(0.1f, 1f)]
    [Tooltip("How far the projectile can go into a wall")]
    public float penetrationPower;
    // 0.0 - 0.3 pistol / shotgun
    // 0.3 - 0.6 rifle
    // 0.6 - 1.0 sniper

    [Tooltip("Max number of penetrations")]
    public byte maxPenetrations;

    [Range(0.3f, 1f)]
    [Tooltip("How quickly a bullet loses the ability to go through walls")]
    public float falloff;
    // 1.0 - 0.7 pistol / shotgun
    // 0.7 - 0.4 rifle
    // 0.4 - 0.0 sniper

    const float PEN_DEPTH_MULT = 5.0f;

    public float GetMaxPenetrationDepth()
    {
        return penetrationPower * PEN_DEPTH_MULT;
        // Pistol: 0.1 - 1.5 units
        // Rifle:  1.5 - 3.3 units
        // Sniper: 3.3 - 5.0 units
    }

    //public float GetCurrentPentrationDepth(float currentFalloff)
    //{
    //    return Mathf.Lerp(0, GetMaxPenetrationDepth(), currentFalloff);
    //}

    public float CalculateFalloff(float currentFalloff, float penetrationDepth, Collider col)
    {
        float fractionOfTotalPen = Mathf.InverseLerp(0, GetMaxPenetrationDepth(), penetrationDepth);

        if (col && col.GetSurface(out Surface surface)) fractionOfTotalPen *= surface.GetHardness();
        else fractionOfTotalPen *= SurfaceHardnesses.DEFAULT_HARDNESS;

        return Mathf.Clamp01(currentFalloff - fractionOfTotalPen);
    }
}

public struct DamageResult
{
    public bool appliedDamage;

    public DamageResult(bool appliedDamage)
    {
        this.appliedDamage = appliedDamage;
    }

    public static DamageResult True => new DamageResult(true);
    public static DamageResult False => new DamageResult(false);
}
