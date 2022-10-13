using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BulletVolleys : MonoBehaviour
{
    public float rpm = 600;
    public float startDist = 400;
    public float targetSize = 3;
    public float sourceSize = 3;
    public Transform targetTransform;
    public Weapon weapon;

    [Space]
    public int minBullets = 4;
    public int maxBullets = 35;

    float timer;
    int bullets;

    Vector3 origin;

    void Update()
    {
        timer -= Time.deltaTime;

        if (bullets > 0 && timer < 0)
        {
            timer = 60f / rpm;
            bullets--;
            Vector3 targetPos = targetTransform.position + Random.insideUnitSphere * targetSize;
            Vector3 bulletOrigin = origin + Random.insideUnitSphere * sourceSize;
            Vector3 dir = bulletOrigin.DirectionTo(targetPos);

            Bullet.Create(bulletOrigin, dir, weapon);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            bullets = Random.Range(minBullets, maxBullets);
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = Mathf.Clamp(randomDir.y, 0.05f, 0.4f);
            randomDir.Normalize();
            origin = targetTransform.position + randomDir * startDist;
        }
    }
}
