using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FPSCamera : MonoBehaviour
{
    public static FPSCamera instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform playerBody;
    public Transform lookTransform;

    [Space]
    public Transform vertMoveTransform;
    public Transform vertMoveWeaponTransform;

    [Space]
    public Transform leanTransform;
    public Transform leanWeaponTransform;

    [Space]
    public VolumeProfile depthOfFieldProfile;

    private float yRotation;
    private float currentLean;
    private float currentLeanMove;

    public static float CurrentLean => instance.currentLean;

    //public float sensitivity = 3;
    public float maxVerticalRotation = 90;

    public float sensitivityMultiplier = 0.1f;//3 / 50;
    //                           Default sens / Good cam sens

    private float sensitivity => CurrentSensFromSettings * sensitivityMultiplier;

    public static float CurrentSensFromSettings = 50;

    public static float VerticalDip = 0f;

    private const float CrouchOffset = 1f;
    private const float EyeHeight = 0.8f;
    private const float VertDipSmoothing = 4;
    private const float VertDipSpeed = 6;
    private const float VertDipRotationMult = 6f;

    private const float LeanSpeed = 5f;

    [Space]
    public float leanAngle = 25f;
    public float leanOffset = 0.3f;
    public float leanBlockedMult = 0.5f;
    public float leanCrouchMult = 0.7f;
    public float leanRaycastLength = 0.75f;
    public LayerMask leanRayMask;

    [Space]
    public float recoilDecaySpeed = 10;
    private static Vector2 desiredRecoil;
    private Vector2 appliedRecoil;
    private float recoilResetTimer;
    private const float RecoilResetTime = 0.5f;

    public Shaker weaponHolderShaker;

    public ShakePreset debugShake;
    public ShakePreset debugWeaponShake;
    public ShakePreset debugAimedShake;
    public ShakePreset debugAimedWeaponShake;

    [Space]
    public float sprintHorShake = 0.18f;
    public float sprintVertShake = 0.2f;
    public float sprintShakePosMult = 1f;
    public float sprintRunMul = 4;
    public Transform sprintTransform;


    public static bool Crouched;

    public static Vector3 ViewDir => instance.transform.forward;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    Shake(Input.GetKey(Inputs.ADS) ? debugAimedShake : debugShake, Input.GetKey(Inputs.ADS) ? debugAimedWeaponShake : debugWeaponShake);

        Recoil();
        MouseLook();

        VerticalMovement();
        Lean();
        SprintShake();
        ADSBlur();
    }

    private void Recoil()
    {
        recoilResetTimer -= Time.deltaTime;
        desiredRecoil = Vector2.Lerp(desiredRecoil, Vector2.zero, Time.deltaTime * recoilDecaySpeed);
        appliedRecoil.x += desiredRecoil.x * Time.deltaTime;
        if (yRotation - desiredRecoil.y * Time.deltaTime < -maxVerticalRotation)
            appliedRecoil.y += maxVerticalRotation + yRotation;
        else
            appliedRecoil.y += desiredRecoil.y * Time.deltaTime;
        yRotation -= desiredRecoil.y * Time.deltaTime;
        playerBody.Rotate(Vector3.up * (desiredRecoil.x * Time.deltaTime));

        if (desiredRecoil.y < 0) desiredRecoil.y = 0;

        //if (recoilResetTimer < 0)
        //if (desiredRecoil.magnitude < 0.01f)
        //{
        Vector2 apply = RemoveRecoil(ref appliedRecoil);
        yRotation += apply.y;
        playerBody.Rotate(Vector3.up * -apply.x);
        //}
    }

    private Vector2 RemoveRecoil(ref Vector2 value)
    {
        Vector2 val = value;
        float slow = recoilResetTimer > 0 ? Remap.Float(recoilResetTimer, 0, RecoilResetTime, 1f, 0f) : 1f;
        value = Vector2.MoveTowards(value, Vector2.zero, RecoilFactor() * Time.deltaTime * recoilDecaySpeed * slow);
        return val - value;
    }

    private void SubRecoilX(float mouseX)
    {
        if (appliedRecoil.x == 0) return;

        float sign = Mathf.Sign(appliedRecoil.x);

        if (sign != Mathf.Sign(mouseX))
        {
            appliedRecoil.x += mouseX;
            if (Mathf.Sign(appliedRecoil.x) != sign)
                appliedRecoil.x = 0;
        }
    }

    float RecoilFactor() => Mathf.Pow((-recoilResetTimer + RecoilResetTime) * 2f, 2);

    private void MouseLook()
    {
        //float x = Input.GetAxisRaw("Mouse X");
        //float y = Input.GetAxisRaw("Mouse Y");
        float x = PlayerInputs.Look.x;
        float y = PlayerInputs.Look.y;

        float adsSens = WeaponManager.InADS ? WeaponManager.CurrentWeapon.Sight.sensMult : 1f;

        float xVal = x * sensitivity * adsSens;
        float yVal = y * sensitivity * adsSens;

        SubRecoilX(xVal);
        appliedRecoil.y = Mathf.Max(appliedRecoil.y + Mathf.Min(yVal, 0), 0f);

        playerBody.Rotate(Vector3.up * xVal);
        // Rotates the body horizontally

        yRotation = Mathf.Clamp(yRotation - yVal, -maxVerticalRotation, maxVerticalRotation);
        //float clampedRotWithRecoil = yRotation;
        float clampedRotWithRecoil = Mathf.Clamp(yRotation, -maxVerticalRotation, maxVerticalRotation);

        // Clamps the Y rotation so you can only look straight up or down, not backwards
        lookTransform.localRotation = Quaternion.Euler(new Vector3(clampedRotWithRecoil, 0));
    }

    private void VerticalMovement()
    {
        float crouchOffset = Crouched ? CrouchOffset : 0f;
        vertMoveTransform.localPosition = Vector3.Lerp(vertMoveTransform.localPosition, Vector3.down * (VerticalDip/* - EyeHeight*/ + crouchOffset), Time.deltaTime * VertDipSmoothing);
        vertMoveTransform.localRotation = Quaternion.Slerp(vertMoveTransform.localRotation, Quaternion.Euler(VerticalDip * VertDipRotationMult, 0, 0), Time.deltaTime * VertDipSmoothing);
        //vertMoveWeaponTransform

        //VerticalDip = Mathf.MoveTowards(VerticalDip, 0, Time.deltaTime * VertDipSpeed);
        VerticalDip = Mathf.Lerp(VerticalDip, 0, Time.deltaTime * VertDipSpeed);
    }

    private void Lean()
    {
        float lean = PlayerInputs.Lean;
        float leanMove = 0;

        //if (Input.GetKey(Inputs.LeanLeft)) lean += 1f;
        //if (Input.GetKey(Inputs.LeanRight)) lean -= 1f;

        if (Mathf.Abs(lean) > 0.2f)
        {
            if (Crouched) lean *= leanCrouchMult;

            leanMove = lean * leanOffset;

            lean *= leanAngle;

            Vector3 rayOrigin = vertMoveTransform.position + (Vector3.up * EyeHeight);
            Vector3 dir = lean > 0f ? -vertMoveTransform.right : vertMoveTransform.right;

            if (Physics.Raycast(rayOrigin, dir, leanRaycastLength, leanRayMask))
            {
                lean *= leanBlockedMult;
                leanMove *= leanBlockedMult;
            }
        }

        currentLean = Mathf.Lerp(currentLean, lean, Time.deltaTime * LeanSpeed);
        currentLeanMove = Mathf.Lerp(currentLeanMove, leanMove, Time.deltaTime * LeanSpeed);

        leanTransform.localRotation = Quaternion.Euler(0, 0, currentLean);
        leanTransform.localPosition = new Vector3(-currentLeanMove, -0.5f, 0);
        //leanWeaponTransform
    }

    private void SprintShake()
    {
        if (!PlayerMovement.Grounded)
        {
            sprintTransform.localRotation = Quaternion.Slerp(sprintTransform.localRotation, Quaternion.identity, Time.deltaTime * 3);
            sprintTransform.localPosition = Vector3.Lerp(sprintTransform.localPosition, Vector3.zero, Time.deltaTime * 3);
            return;
        }

        float sin = WeaponSway.SinValue;
        //Debug.Log(sin);
        float vert = sin * sin * sin;
        Vector3 desired = new Vector3(Mathf.Abs(vert) * sprintVertShake, -sin * sprintHorShake);
        Vector3 desiredPos = desired * sprintShakePosMult;
        //Vector3 desired = Vector3.zero;

        if (PlayerMovement.Running)
            desired *= sprintRunMul;

        sprintTransform.localRotation = Quaternion.Slerp(sprintTransform.localRotation, Quaternion.Euler(desired), Time.deltaTime * 10);
        sprintTransform.localPosition = Vector3.Lerp(sprintTransform.localPosition, desiredPos, Time.deltaTime * 10);
    }

    private void ADSBlur()
    {
        if (depthOfFieldProfile == null) return;

        if (!depthOfFieldProfile.TryGet(out DepthOfField dof)) return;

        float desired = 3; // small, default focal length, could use 0

        if (WeaponManager.InADS)
            desired = WeaponManager.CurrentWeapon.Sight.adsBlur;

        dof.focalLength.value = Mathf.Lerp(dof.focalLength.value, desired, Time.deltaTime * 5f);
    }

    public void LookAt_Local(Vector3 point)
    {
        Vector3 eulers = Quaternion.LookRotation(transform.position.DirectionTo(point), Vector3.up).eulerAngles;
        playerBody.eulerAngles = new Vector3(0, eulers.y, 0);
        yRotation = Mathf.Clamp(eulers.x, -maxVerticalRotation, maxVerticalRotation);
        lookTransform.localRotation = Quaternion.Euler(new Vector3(yRotation, 0));
    }

    public static void LookAt(Vector3 point)
    {
        Vector3 eulers = Quaternion.LookRotation(instance.transform.position.DirectionTo(point), Vector3.up).eulerAngles;
        instance.playerBody.eulerAngles = new Vector3(0, eulers.y, 0);
        instance.yRotation = Mathf.Clamp(eulers.x, -instance.maxVerticalRotation, instance.maxVerticalRotation);
        instance.lookTransform.localRotation = Quaternion.Euler(new Vector3(instance.yRotation, 0));
    }

    public static void Shake(ShakePreset cams, ShakePreset weapons)
    {
        ShakeCams(cams);
        ShakeWeapons(weapons);
    }

    public static void ShakeCams(ShakePreset preset)
    {
        Shaker.ShakeAllSeparate(preset);
    }

    public static void ShakeWeapons(ShakePreset preset)
    {
        instance.weaponHolderShaker.Shake(preset);
    }

    public static void AddRecoil(Vector2 amount)
    {
        desiredRecoil += amount;
        instance.recoilResetTimer = RecoilResetTime;
    }


    private void OnDrawGizmos()
    {
        float lean = 1f;

        Vector3 rayOrigin = vertMoveTransform.position + (Vector3.up * EyeHeight);
        Vector3 dir = lean > 0f ? -vertMoveTransform.right : vertMoveTransform.right;

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(rayOrigin, rayOrigin + dir * leanRaycastLength);
    }
}
