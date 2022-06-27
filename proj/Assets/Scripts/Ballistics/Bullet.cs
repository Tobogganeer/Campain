using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float velocityLossPerSecond;
    [HideInInspector] public float baseDamage;
    [HideInInspector] public AnimationCurve damageFalloff;
    [HideInInspector] public float rbForce;
    [HideInInspector] public float armourPenetration;
    [HideInInspector] public Hitbox.DamageRegions hitboxDamageMultipliers;
    [HideInInspector] public float baseVelocity;
    [HideInInspector] public BulletBallistics ballistics;

    /*[SerializeField]*/ private Vector3 velocity;

    private const string NAME = "Bullet";

    private const float GravityMultiplier = 1;
    private const float MinBulletVel = 20;

    private Transform audioListenerTransform;
    //private float lastDist;
    bool cracked;
    bool whized;

    private const float CrackDistThreshold = 4;
    private const float WhizDistThreshold = 15;

    public float totalDistTravelled = 0;

    public void Initialize(Vector3 position, Vector3 direction, Weapon weapon)
    {
        transform.position = position;
        transform.forward = direction;
        FX.SpawnTracer(transform);

        WeaponData data = weapon.Data;

        velocityLossPerSecond = data.velocityLossPerSecond;
        baseDamage = data.baseDamage * weapon.GetDamageMultiplier();
        damageFalloff = data.damageFalloff;
        rbForce = data.rbForce;
        armourPenetration = data.armourPenetration;
        hitboxDamageMultipliers = data.hitboxDamageMultipliers;
        baseVelocity = weapon.GetBulletVelocity();
        ballistics = data.ballistics;

        audioListenerTransform = Camera.main.transform;
        float lastDist = Vector3.Distance(audioListenerTransform.position, transform.position);
        if (lastDist < CrackDistThreshold)
            cracked = true;
        if (lastDist < WhizDistThreshold)
            whized = true;
    }

    private void Start()
    {
        velocity = transform.forward * baseVelocity;
        Destroy(gameObject, 8);
    }

    private void Update()
    {
        //if (!Input.GetKey(KeyCode.Mouse0)) return;

        if (velocity.sqrMagnitude < MinBulletVel * MinBulletVel)
            Destroy(gameObject);

        Vector3 point1 = transform.position;

        velocity += Physics.gravity * Time.deltaTime * GravityMultiplier;
        Vector3 point2 = point1 + velocity * Time.deltaTime;

        CalculatePenetration(point1, point2);

        transform.position = point2;

        velocity = velocity.normalized * (velocity.magnitude - velocityLossPerSecond * Time.deltaTime);
        /*
         * blech
        float flatMag = velocity.Flattened().magnitude;
        float y = velocity.y;
        flatMag -= velocityLossPerSecond * Time.deltaTime;
        velocity = velocity.Flattened().normalized * flatMag;
        velocity.y = y;
        */
        // Slow bullet, keep y

        float crackDist = Vector3.Distance(audioListenerTransform.position, transform.position);
        if (!cracked && crackDist < CrackDistThreshold)
        {
            cracked = true;
            float percent = Mathf.InverseLerp(CrackDistThreshold, 0, crackDist);
            if (Random.value > percent)
                AudioManager.Play(new Audio("BulletCrack").SetPosition(transform.position).SetDistance(20));
                //AudioManager.Play(AudioArray.BulletCrack, transform.position, null, 20);
        }
        if (!whized && Vector3.Distance(audioListenerTransform.position, transform.position) < WhizDistThreshold)
        {
            whized = true;
            AudioManager.Play(new Audio("BulletWhiz").SetPosition(transform.position));
            //AudioManager.Play(AudioArray.BulletWhiz, transform.position);
        }

        totalDistTravelled += velocity.magnitude * Time.deltaTime;
    }

    private /*Vector3*/ void CalculatePenetration(Vector3 origin, Vector3 destination)
    {
        Vector3 dir = destination - origin;
        float totalDist = dir.magnitude;
        dir /= totalDist;
        // Normalize without extra sqrt call

        RaycastHit frontOfObj;
        //RaycastHit backOfObj;
        float distTravelled = 0;

        //const int MaxChecks = 16;
        const int MaxChecks = 1;

        for (int i = 0; i < MaxChecks; i++)
        {
            if (Physics.Raycast(new Ray(origin, dir), out frontOfObj, totalDist, LayerMasks.BulletLayerMask))
            {
                distTravelled += frontOfObj.distance;

                float hitAngle = Vector3.Angle(frontOfObj.normal, dir) - 90;
                float surfaceHardness = SurfaceHardnesses.DEFAULT_HARDNESS;

                if (frontOfObj.collider.GetSurface(out Surface surface))
                    surfaceHardness = surface.GetHardness();

                float angle = Remap.Float(Mathf.Clamp(hitAngle, 0, 90), 0, 90, 0, 1);

                FX.SpawnBulletHit_Debug(frontOfObj.point, frontOfObj.normal, transform);

                if (!Bounce(surfaceHardness, angle, frontOfObj.normal, frontOfObj.point, ref dir, ref origin))
                    Penetrate(surfaceHardness, ref frontOfObj, ref dir, ref origin, ref distTravelled);

                if (frontOfObj.collider.TryGetComponent(out Rigidbody rb))
                    rb.AddForceAtPosition((-frontOfObj.normal + dir) * 3, frontOfObj.point, ForceMode.Impulse);


                if (frontOfObj.collider.TryGetComponent(out IBulletDamagable damagable))
                    damagable.TakeBulletDamage(new DamageDetails(baseDamage * Mathf.InverseLerp(0, baseVelocity, velocity.magnitude), DamageSource.Bullet));
            }
        }

        // Temp, return pos after velocity decreases are applied
        //return destination;
    }

    private bool Bounce(float surfaceHardness, float angle01, Vector3 normal, Vector3 hitPoint, ref Vector3 dir, ref Vector3 origin)
    {
        if (surfaceHardness >= ballistics.MinBounceHardness && Random.Range(0, 1f) < ballistics.BounceChance.Evaluate(angle01))
        {
            // Bounce off surface

            // TODO: Apply damage to obj
            // TODO: Surface FX

            FX.SpawnBulletHit_Debug(hitPoint, normal, null);

            float velMag = velocity.magnitude;
            //Vector3 velDir = velocity.WithY(velocity.y / GravityMultiplier).normalized;
            Vector3 velDir = velocity.normalized;

            Vector3 bounced = Vector3.Reflect(velDir, normal);
            bounced += Random.insideUnitSphere * ballistics.BounceDirectionRandomization * surfaceHardness;
            bounced.Normalize();

            velMag -= ballistics.BounceVelocityLoss.Evaluate(angle01);

            velMag = Mathf.Max(velMag, 0);

            velocity = bounced * velMag;
            dir = bounced;
            origin = hitPoint;

            return true;
        }

        return false;
    }

    private bool Penetrate(float surfaceHardness, ref RaycastHit frontHit, ref Vector3 dir, ref Vector3 origin, ref float distTravelled)
    {
        // Try to penetrate into surface
        const float BackCastLength = 100f;
        float wallThickness = 0;

        bool exits = frontHit.collider.Raycast(new Ray(frontHit.point + dir * BackCastLength, -dir), out RaycastHit backOfObj, BackCastLength);

        if (exits)
        {
            // TODO: Apply damage to obj
            // TODO: Surface FX

            wallThickness = (backOfObj.point - frontHit.point).magnitude;

            // for example, bullet wont try to penetrate hardness 3, unless its a thin wall
            bool canPenetrate = (surfaceHardness * Mathf.Clamp01(wallThickness)) < ballistics.MaxHardness;

            if (canPenetrate)
            {
                float velMag = velocity.magnitude;
                Vector3 velDir = velocity.normalized;

                velDir += Random.insideUnitSphere * ballistics.PenetrationDirectionRandomization * surfaceHardness
                    * Mathf.Clamp01(wallThickness) * Mathf.Clamp(Mathf.InverseLerp(baseVelocity, 0, velocity.magnitude), 0.4f, 1f)
                    * Mathf.Clamp(totalDistTravelled * 0.01f, 0.2f, 1f);
                velDir.Normalize();
                // Bounce less the faster bullet is going
                // Was making them bounce like nothing so clamped to -20%- 40%
                // After 100 meters as well

                velMag -= ballistics.VelocityLossPerUnit * wallThickness;

                velMag = Mathf.Max(velMag, 0);

                velocity = velDir * velMag;
                dir = velDir;
                origin = backOfObj.point;
                distTravelled += wallThickness;

                return true;
            }
            else
            {
                // TODO: Apply damage to obj
                // TODO: Surface FX
            }
        }
        else
        {
            // TODO: Apply damage to obj
            // TODO: Surface FX
        }

        return false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 point1 = transform.position;
        Vector3 predictedVel = velocity;
        float step = 0.05f;

        //float drag = 150;

        for (float i = 0; i < 1; i += step)
        {
            predictedVel += Physics.gravity * step * GravityMultiplier;
            //predictedVel.x = Mathf.MoveTowards(predictedVel.x, 0, drag * step);
            //predictedVel.z = Mathf.MoveTowards(predictedVel.z, 0, drag * step);
            Vector3 point2 = point1 + predictedVel * step;
            Gizmos.DrawLine(point1, point2);
            point1 = point2;
        }
    }



    public static Bullet Create(Vector3 position, Vector3 direction, Weapon weapon)
    {
        Bullet bullet = new GameObject(NAME).AddComponent<Bullet>();
        bullet.Initialize(position, direction, weapon);
        return bullet;
    }
}
