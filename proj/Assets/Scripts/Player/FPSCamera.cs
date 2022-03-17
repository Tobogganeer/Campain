using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;

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

    private float yRotation;
    private float currentLean;

    public static float CurrentLean => instance.currentLean;

    //public float sensitivity = 3;
    public float maxVerticalRotation = 90;

    private const float SENSITIVITY_MULT = 0.1f;//3 / 50;
    //                           Default sens / Good cam sens

    private float sensitivity => CurrentSensFromSettings * SENSITIVITY_MULT;

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
    public float leanBlockedMult = 0.5f;
    public float leanCrouchMult = 0.7f;
    public float leanRaycastLength = 0.75f;
    public LayerMask leanRayMask;

    [Space]
    public float recoilDecaySpeed = 10;
    private static Vector2 desiredRecoil;

    public Shaker weaponHolderShaker;

    public static bool Crouched;

    public static Vector3 ViewDir => instance.transform.forward;

    public ShakePreset debugShake;
    public ShakePreset debugWeaponShake;
    public ShakePreset debugAimedShake;
    public ShakePreset debugAimedWeaponShake;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    Shake(Input.GetKey(Inputs.ADS) ? debugAimedShake : debugShake, Input.GetKey(Inputs.ADS) ? debugAimedWeaponShake : debugWeaponShake);

        MouseLook();

        VerticalMovement();
        Lean();
    }

    private void MouseLook()
    {
        desiredRecoil = Vector2.Lerp(desiredRecoil, Vector2.zero, Time.deltaTime * recoilDecaySpeed);
        yRotation -= desiredRecoil.y * Time.deltaTime;
        playerBody.Rotate(Vector3.up * desiredRecoil.x * Time.deltaTime);

        if (desiredRecoil.y < 0) desiredRecoil.y = 0;

        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");

        playerBody.Rotate(Vector3.up * x * sensitivity);
        // Rotates the body horizontally

        yRotation = Mathf.Clamp(yRotation - y * sensitivity, -maxVerticalRotation, maxVerticalRotation);
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
        float lean = 0;

        if (Input.GetKey(Inputs.LeanLeft)) lean += 1f;
        if (Input.GetKey(Inputs.LeanRight)) lean -= 1f;

        if (Mathf.Abs(lean) > 0.2f)
        {
            lean *= leanAngle;

            if (Crouched) lean *= leanCrouchMult;

            Vector3 rayOrigin = vertMoveTransform.position + (Vector3.up * EyeHeight);
            Vector3 dir = lean > 0f ? -vertMoveTransform.right : vertMoveTransform.right;

            if (Physics.Raycast(rayOrigin, dir, leanRaycastLength, leanRayMask))
                lean *= leanBlockedMult;
        }

        currentLean = Mathf.Lerp(currentLean, lean, Time.deltaTime * LeanSpeed);

        leanTransform.localRotation = Quaternion.Euler(0, 0, currentLean);
        //leanWeaponTransform
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
