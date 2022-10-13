using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public SerializableDictionary<BarrelType, GameObject> barrels;
    public SerializableDictionary<UnderbarrelType, GameObject> underbarrels;
    public SerializableDictionary<SightType, GameObject> sights;

    /*
    public GameObject suppresor;
    public GameObject defaultBarrel;
    public GameObject compensator;

    public GameObject holo;
    public GameObject acog;
    */

    public ParticleSystem bulletCasings;
    public GameObject fireball;
    public GameObject swayBone;

    public Animator animator;

    public int magSize = 30;
    int bullets;

    int curBarrel = 0;
    int curSight = 0;
    int curUnderbarrel = 0;

    float incomingBulletTimer;

    private void Start()
    {
        SetBarrel();
        SetUnderbarrel();
        SetSight();
        bullets = magSize;
    }

    private void Update()
    {
        // TEMP TESTING

        timer -= Time.deltaTime;
        incomingBulletTimer -= Time.deltaTime;

        fireball.transform.localScale = Vector3.Lerp(fireball.transform.localScale, Vector3.zero, Time.deltaTime * 20);
        if (fireball.transform.localScale.x < 0.01f)
            fireball.SetActive(false);

        if (Keyboard.current.kKey.isPressed && incomingBulletTimer < 0)
        {
            incomingBulletTimer = 60f / Data.fireRateRPM;
            Vector3 pos = barrelTip.position + barrelTip.forward * 200 + Random.insideUnitSphere * 3;
            Vector3 dir = pos.DirectionTo_NoNormalize(barrelTip.position) + Random.insideUnitSphere * 7;
            Bullet.Create(pos, dir.normalized, this);
        }

        if (timer < 0 && ((PlayerInputs.FireHeld && Data.fullAuto) || (PlayerInputs.Fire && !Data.fullAuto)) && bullets > 0)
        {
            bullets--;
            Bullet.Create(barrelTip.position, barrelTip.forward, this);
            timer = 60f / Data.fireRateRPM;
            ShakeCamera();
            //animator.Play("Fire");
            animator.Play("Fire", -1, 0);
            //animator.CrossFadeInFixedTime("Fire", 0.1f);
            FPSCamera.AddRecoil(GetRecoil());
            fireAnimation.Apply(fireAnimation.settings);
            fireball.SetActive(true);
            fireball.transform.localScale = Vector3.one * 0.2f;
            //Transform parent = null;// barrelTip;
            Transform parent = barrelTip;

            float vol = Barrel.shootVolume;
            float mechVol = Barrel.shootMechVolume;
            AudioManager.Play(new Audio(Barrel.shootSound).SetPosition(barrelTip.position).SetParent(parent).SetVolume(vol));
            AudioManager.Play(new Audio("NP5_Fire_Mech").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.55f * mechVol));
            AudioManager.Play(new Audio("NP5_Fire_Omph").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.45f * mechVol));
            AudioManager.Play(new Audio("NP5_Fire_Tech").SetPosition(barrelTip.position).SetParent(parent).SetVolume(0.45f * mechVol));
            //AudioManager.Play(AudioArray.NP5_Fire_Mech, barrelTip.position, parent, 35, AudioCategory.SFX, 0.55f * mechVol);
            //AudioManager.Play(AudioArray.NP5_Fire_Omph, barrelTip.position, parent, 35, AudioCategory.SFX, 0.45f * mechVol);
            //AudioManager.Play(AudioArray.NP5_Fire_Tech, barrelTip.position, parent, 35, AudioCategory.SFX, 0.45f * mechVol);
            bulletCasings.Play();
            //Debug.Log("New timer: " + timer);
        }

        if (PlayerInputs.AttachBarrel)
        {
            curBarrel++;
            if (curBarrel >= Data.attachments.BarrelCount)
                curBarrel = 0;
            SetBarrel();
        }

        if (PlayerInputs.AttachMisc)
        {
            curUnderbarrel++;
            if (curUnderbarrel >= Data.attachments.UnderbarrelCount)
                curUnderbarrel = 0;
            SetUnderbarrel();
        }

        if (PlayerInputs.AttachSight)
        {
            curSight++;
            if (curSight >= Data.attachments.SightCount)
                curSight = 0;
            SetSight();
        }

        if (PlayerInputs.Reload)
        {
            AudioManager.Play(new Audio(Data.reloadSound).SetPosition(transform.position).SetParent(transform).SetPitch(0.97f, 1.03f));
            //AudioManager.Play(AudioArray.NP5_Reload, transform.position, transform, 35, AudioCategory.SFX, 1, 0.97f, 1.03f);
            //AudioManager.Play(AudioArray.NP5_Reload, transform.position, transform, 35, AudioCategory.SFX, 1, 0.97f, 1.03f);
            bullets = magSize;
            animator.Play("Reload");
        }
    }

    void SetBarrel()
    {
        foreach (var item in barrels.Dict.Values)
            item.SetActive(false);

        //barrel = barrels.values[curBarrel].key;
        barrel = (BarrelType)curBarrel;

        if (barrels.Dict.ContainsKey(barrel))
            barrels.Dict[barrel].SetActive(true);
        else
            if (barrels.Dict.TryGetValue(BarrelType.Default, out var val)) val.SetActive(true);
    }

    void SetUnderbarrel()
    {
        foreach (var item in underbarrels.Dict.Values)
            item.SetActive(false);

        //underbarrel = underbarrels.values[curUnderbarrel].key;
        underbarrel = (UnderbarrelType)curUnderbarrel;

        if (underbarrels.Dict.ContainsKey(underbarrel))
            underbarrels.Dict[underbarrel].SetActive(true);
        else
            if (underbarrels.Dict.TryGetValue(UnderbarrelType.Nothing, out var val)) val.SetActive(true);
    }

    void SetSight()
    {
        foreach (var item in sights.Dict.Values)
            item.SetActive(false);

        //sight = sights.values[curSight].key;
        sight = (SightType)curSight;

        if (sights.Dict.ContainsKey(sight))
            sights.Dict[sight].SetActive(true);
        else
            if (sights.Dict.TryGetValue(SightType.IronSights, out var val)) val.SetActive(true);
    }
}
