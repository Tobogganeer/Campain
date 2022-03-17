using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponType type;

    public BarrelType barrel;
    public UnderbarrelType underbarrel;
    public SightType sight;

    private WeaponData _data;
    public WeaponData Data
    {
        get
        {
            if (_data == null) _data = Weapons.Get(type);
            return _data;
        }
    }

    public Vector3 GetADSOffset()
    {
        return Data.attachments.Get(sight).aimPos;
    }

    public float GetADSFov()
    {
        return Data.attachments.Get(sight).fovMult;
    }

    public float GetBulletVelocity()
    {
        return Data.attachments.Get(barrel).bulletVelocity;
    }

    public Vector2 GetRecoil()
    {
        Vector2 rawRecoil = Data.attachments.Get(barrel).recoil;
        Vector2 finalRecoil = Vector2.zero;

        finalRecoil.x = Random.Range(-rawRecoil.x, rawRecoil.x);

        float yRange = rawRecoil.y * 0.65f;
        finalRecoil.y = Random.Range(rawRecoil.y - yRange, rawRecoil.y + yRange);

        return finalRecoil;
    }

    public AudioArray GetShootSound()
    {
        return Data.attachments.Get(barrel).shootSound;
    }

    public void ShakeCamera()
    {
        if (WeaponManager.InADS)
            FPSCamera.Shake(Data.aimedCamShakePreset, Data.aimedWeaponShakePreset);
        else
            FPSCamera.Shake(Data.camShakePreset, Data.weaponShakePreset);
    }

    public Transform barrelTip;
    public float fireRateRPM;
    public float recoil = 8;
    float timer;

    private void Update()
    {
        // TEMP TESTING

        timer -= Time.deltaTime;

        if (timer < 0 && Input.GetKey(KeyCode.Mouse0))
        {
            Bullet.Create(barrelTip.position, barrelTip.forward, this);
            timer = 60f / fireRateRPM;
            ShakeCamera();
            FPSCamera.AddRecoil(GetRecoil());
            //Debug.Log("New timer: " + timer);
        }
    }
}
