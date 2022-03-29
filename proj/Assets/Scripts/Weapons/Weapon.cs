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

        if (PlayerMovement.Crouched) finalRecoil *= Data.crouchRecoilMult;
        if (WeaponManager.InADS) finalRecoil *= Data.attachments.Get(sight).recoilMult;

        return finalRecoil;
    }

    public AudioArray GetShootSound()
    {
        return Data.attachments.Get(barrel).shootSound;
    }

    public float GetShootVolume()
    {
        return Data.attachments.Get(barrel).shootVolume;
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

    public FireAnimation fireAnimation;

    public GameObject suppresor;
    public GameObject defaultBarrel;
    public GameObject compensator;

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
            fireAnimation.Apply(fireAnimation.settings);
            AudioManager.Play(GetShootSound(), barrelTip.position, barrelTip, 35, AudioCategory.SFX, GetShootVolume());
            AudioManager.Play(AudioArray.NP5_Fire_Mech, barrelTip.position, barrelTip, 35);
            //Debug.Log("New timer: " + timer);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            defaultBarrel.SetActive(false);
            compensator.SetActive(false);
            suppresor.SetActive(true);
            barrel = BarrelType.Suppressed;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            defaultBarrel.SetActive(true);
            compensator.SetActive(false);
            suppresor.SetActive(false);
            barrel = BarrelType.Default;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            defaultBarrel.SetActive(false);
            compensator.SetActive(true);
            suppresor.SetActive(false);
            barrel = BarrelType.Compensator;
        }
    }
}
