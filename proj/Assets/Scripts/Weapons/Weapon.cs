using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponType type;

    [SerializeField] private BarrelType barrel;
    [SerializeField] private UnderbarrelType underbarrel;
    [SerializeField] private SightType sight;

    private WeaponData _data;
    public WeaponData Data
    {
        get
        {
            if (_data == null) _data = Weapons.Get(type);
            return _data;
        }
    }

    public Barrel Barrel => Data.attachments.Get(barrel);
    public Underbarrel Underbarrel => Data.attachments.Get(underbarrel);
    public Sight Sight => Data.attachments.Get(sight);

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

    public float GetSensitivityMult()
    {
        return Data.attachments.Get(sight).sensMult;
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

    public string GetShootSound()
    {
        return Data.attachments.Get(barrel).shootSound;
    }

    public float GetShootVolume()
    {
        return Data.attachments.Get(barrel).shootVolume;
    }
    public float GetShootMechVolume()
    {
        return Data.attachments.Get(barrel).shootMechVolume;
    }

    public float GetDamageMultiplier()
    {
        return Data.attachments.Get(barrel).damageMult;
    }

    public void ShakeCamera()
    {
        if (WeaponManager.InADS)
            FPSCamera.Shake(Data.aimedCamShakePreset, Data.aimedWeaponShakePreset);
        else
            FPSCamera.Shake(Data.camShakePreset, Data.weaponShakePreset);
    }

    public Transform barrelTip;
    public float recoil = 8;
    float timer;

    public FireAnimation fireAnimation;

    public GameObject suppresor;
    public GameObject defaultBarrel;
    public GameObject compensator;

    public GameObject holo;
    public GameObject acog;

    public ParticleSystem bulletCasings;

    public Animator animator;

    int bullets = 30;

    int curBarrel = 0;
    int curSight = 0;

    float incomingBulletTimer;

    private void Start()
    {
        SetBarrel();
        SetSight();
    }

    private void Update()
    {
        // TEMP TESTING

        timer -= Time.deltaTime;
        incomingBulletTimer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.K) && incomingBulletTimer < 0)
        {
            incomingBulletTimer = 60f / Data.fireRateRPM;
            Vector3 pos = barrelTip.position + barrelTip.forward * 200 + Random.insideUnitSphere * 3;
            Vector3 dir = pos.DirectionTo_NoNormalize(barrelTip.position) + Random.insideUnitSphere * 7;
            Bullet.Create(pos, dir.normalized, this);
        }

        if (timer < 0 && Input.GetKey(KeyCode.Mouse0) && bullets > 0)
        {
            bullets--;
            Bullet.Create(barrelTip.position, barrelTip.forward, this);
            timer = 60f / Data.fireRateRPM;
            ShakeCamera();
            FPSCamera.AddRecoil(GetRecoil());
            fireAnimation.Apply(fireAnimation.settings);
            //Transform parent = null;// barrelTip;
            Transform parent = barrelTip;

            float vol = GetShootVolume();
            float mechVol = GetShootMechVolume();
            AudioManager.Play(new Audio(GetShootSound()).SetPosition(barrelTip.position).SetParent(parent).SetVolume(vol));
            AudioManager.Play(new Audio("NP5_Fire_Mech").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.55f * mechVol));
            AudioManager.Play(new Audio("NP5_Fire_Omph").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.45f * mechVol));
            AudioManager.Play(new Audio("NP5_Fire_Tech").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.45f * mechVol));
            //AudioManager.Play(AudioArray.NP5_Fire_Mech, barrelTip.position, parent, 35, AudioCategory.SFX, 0.55f * mechVol);
            //AudioManager.Play(AudioArray.NP5_Fire_Omph, barrelTip.position, parent, 35, AudioCategory.SFX, 0.45f * mechVol);
            //AudioManager.Play(AudioArray.NP5_Fire_Tech, barrelTip.position, parent, 35, AudioCategory.SFX, 0.45f * mechVol);
            bulletCasings.Play();
            //Debug.Log("New timer: " + timer);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            curBarrel++;
            if (curBarrel > 2)
                curBarrel = 0;
            SetBarrel();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            curSight++;
            if (curSight > 2)
                curSight = 0;
            SetSight();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            AudioManager.Play(new Audio("NP5_Reload").SetPosition(transform.position).SetParent(transform).SetPitch(0.97f, 1.03f));
            //AudioManager.Play(AudioArray.NP5_Reload, transform.position, transform, 35, AudioCategory.SFX, 1, 0.97f, 1.03f);
			//AudioManager.Play(AudioArray.NP5_Reload, transform.position, transform, 35, AudioCategory.SFX, 1, 0.97f, 1.03f);
            bullets = 30;
            animator.Play("Reload");
        }
    }

    void SetBarrel()
    {
        defaultBarrel.SetActive(false);
        compensator.SetActive(false);
        suppresor.SetActive(false);
        barrel = (BarrelType)curBarrel;

        switch (barrel)
        {
            case BarrelType.Default:
                defaultBarrel.SetActive(true);
                break;
            case BarrelType.Suppressed:
                suppresor.SetActive(true);
                break;
            case BarrelType.Compensator:
                compensator.SetActive(true);
                break;
        }
    }

    void SetSight()
    {
        holo.SetActive(false);
        acog.SetActive(false);
        sight = (SightType)curSight;

        switch (sight)
        {
            case SightType.Holo:
                holo.SetActive(true);
                break;
            case SightType.Acog:
                acog.SetActive(true);
                break;
        }
    }
}
