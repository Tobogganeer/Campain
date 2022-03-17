using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float maxRange;
    [HideInInspector] public float baseDamage;
    [HideInInspector] public AnimationCurve damageFalloff;
    [HideInInspector] public float rbForce;
    [HideInInspector] public float armourPenetration;
    [HideInInspector] public Hitbox.DamageRegions hitboxDamageMultipliers;
    [HideInInspector] public float baseVelocity;
    [HideInInspector] public BulletBallistics ballistics;

    /*[SerializeField]*/ private Vector3 velocity;

    private const string NAME = "Bullet";

    private const float GravityMultiplier = 5;

    public void Initialize(Vector3 position, Vector3 direction, Weapon weapon)
    {
        transform.position = position;
        transform.forward = direction;
        FX.SpawnTracer(transform);

        WeaponData data = weapon.Data;

        maxRange = data.maxRange;
        baseDamage = data.baseDamage;
        damageFalloff = data.damageFalloff;
        rbForce = data.rbForce;
        armourPenetration = data.armourPenetration;
        hitboxDamageMultipliers = data.hitboxDamageMultipliers;
        baseVelocity = weapon.GetBulletVelocity();
        ballistics = data.ballistics;
    }

    private void Start()
    {
        velocity = transform.forward * baseVelocity;
        Destroy(gameObject, 5);
    }

    private void Update()
    {
        //if (!Input.GetKey(KeyCode.Mouse0)) return;

        Vector3 point1 = transform.position;

        velocity += Physics.gravity * Time.deltaTime * GravityMultiplier;
        Vector3 point2 = point1 + velocity * Time.deltaTime;
        Vector3 dir = point2 - point1;

        if (Physics.Raycast(new Ray(point1, dir), dir.magnitude))
        {
            Debug.Log("Hit obj");
        }

        transform.position = point2;
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


    [System.Serializable]
    public class BulletBallistics
    {
        [Tooltip("Hitting an object at less than this angle results in a bounce off.")]
        public float MinAngle = 10f;
        [Tooltip("How much speed the bullet loses on a bounce")]
        public float BounceVelocityLoss = 100;

        [Tooltip("How much velocity is lost per unit penetrated. Multiplied with hardness of penetrated object.")]
        public float VelocityLossPerUnit = 100;
        [Tooltip("Cannot go through any material with a hardness higher than this value. " +
            "Multiplied with 0-1 thickness of potential hit, so shooting through corners will still work.")]
        public float MaxHardness = 3f;
    }
}
