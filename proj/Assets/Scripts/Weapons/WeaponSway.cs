using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public static WeaponSway instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform mouseSwayObj;
    public Transform movementSwayObj;
    public Transform movementBobObj;
    public Transform adsObj;

    [Header("Mouse")]
    public bool invertMouse = true;
    public float mouseSwayAmount = 0.03f;
    public float mouseMaxAmount = 0.06f;
    public float mouseSmoothAmount = 12f;
    public float mouseRotAmount = 1f;
    public float mouseRotMaxAmount = 5f;

    [Header("Movement")]
    public bool invertMovement = false;
    public float movementSwayAmount = 0.04f;
    public float movementMaxAmount = 0.1f;
    public float movementSwayRotAmount = 1f;
    public float movementSwayMaxRot = 4f;
    public float movementSmoothAmount = 5f;
    public float verticalMultiplier = 1.5f;

    [Header("Movement Bob")]
    public bool bobUp = true;
    public float bobSpeed = 2.5f;
    public float bobAmount = 0.03f;
    public Vector3 airOffset = new Vector3(0, -0.1f, -0.05f);
    public Vector3 airRotOffset = new Vector3(15, 0, 0f);
    public Vector3 movingOffset = new Vector3(0, -0.01f, -0.01f);
    public float runningMultiplier = 1.5f;
    public float defaultSmoothSpeed = 5;
    public float aimingSmoothSpeed = 15;

    [Space]
    public float crouchingMultiplier = 0.6f;

    [Header("ADS")]
    public float adsBobSwayMultiplier = 0.1f;
    public float adsSpeed = 5;

    [Space]
    public bool debugAlwaysADS;

    private float smoothSpeed;

    public static bool IsInADS => IsInADS_Method();
    public static float MaxADSInfluence = 1f;

    private float time;

    
    private void Update()
    {
        MouseSway();
        MovementSway();
        MovementBob();
        ADS();
    }

    
    private void MouseSway()
    {
        float invert = invertMouse ? -1 : 1;
        Vector2 desiredMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * invert * mouseSwayAmount;
        //desiredMovement *= Time.deltaTime;
        desiredMovement.x = Mathf.Clamp(desiredMovement.x, -mouseMaxAmount, mouseMaxAmount);
        desiredMovement.y = Mathf.Clamp(desiredMovement.y, -mouseMaxAmount, mouseMaxAmount);

        if (IsInADS) desiredMovement *= adsBobSwayMultiplier;

        //Vector3 finalPosition = new Vector3(desiredMovement.x, 0, desiredMovement.y);
        mouseSwayObj.localPosition = Vector3.Lerp(mouseSwayObj.localPosition, desiredMovement, Time.deltaTime * mouseSmoothAmount);


        Vector3 rotMovement = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * invert * mouseRotAmount;
        //Debug.Log("Mouse: " + Input.GetAxis("Mouse X") + " " + Input.GetAxis("Mouse Y"));

        rotMovement.x = Mathf.Clamp(rotMovement.x, -mouseRotMaxAmount, mouseRotMaxAmount);
        rotMovement.y = Mathf.Clamp(rotMovement.y, -mouseRotMaxAmount, mouseRotMaxAmount);

        if (IsInADS) rotMovement *= adsBobSwayMultiplier;

        mouseSwayObj.localRotation = Quaternion.Slerp(mouseSwayObj.localRotation, Quaternion.Euler(rotMovement), Time.deltaTime * mouseSmoothAmount);
    }

    private void MovementSway()
    {
        float invert = invertMovement ? -1 : 1;
        Vector3 desiredMovement = invert * PlayerMovement.LocalVelocity * movementSwayAmount;
        desiredMovement.x = Mathf.Clamp(desiredMovement.x, -movementMaxAmount, movementMaxAmount);
        desiredMovement.y = -1 * verticalMultiplier * Mathf.Clamp(desiredMovement.y,
            -movementMaxAmount * verticalMultiplier, movementMaxAmount * verticalMultiplier);
        desiredMovement.z = Mathf.Clamp(desiredMovement.z, -movementMaxAmount, movementMaxAmount);

        float angle = movementSwayRotAmount * PlayerMovement.LocalVelocity.x;
        angle = Mathf.Clamp(angle, -movementSwayMaxRot, movementSwayMaxRot);

        if (IsInADS) desiredMovement *= adsBobSwayMultiplier;

        movementSwayObj.localPosition = Vector3.Lerp(movementSwayObj.localPosition, desiredMovement, Time.deltaTime * movementSmoothAmount);
        movementSwayObj.localRotation = Quaternion.Slerp(movementSwayObj.localRotation, Quaternion.Euler(0, 0, -angle), Time.deltaTime * movementSmoothAmount);
    }

    private void MovementBob()
    {
        Vector3 actualHorizontalVelocity = PlayerMovement.LocalVelocity.Flattened();

        float velocityMag = actualHorizontalVelocity.magnitude;

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);

        time += Time.deltaTime * bobSpeed * velocityMag * data.bobSpeedMult;

        float sinValue = Mathf.Sin(time);
        Vector3 offset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;

        if (PlayerMovement.Grounded)
        {
            float verticalMult = bobUp ? -1 : 1;
            float runningMult = Mathf.Lerp(1, runningMultiplier, PlayerMovement.NormalizedSpeed);
            float crouchingMult = PlayerMovement.Crouched ? crouchingMultiplier : 1f;

            float movementScale = Mathf.InverseLerp(0, PlayerMovement.instance.movementProfile.runningSpeed, velocityMag);
            float multiplier = bobAmount * runningMult * crouchingMult * data.bobAmountMult;
            if (!PlayerMovement.Sliding)
                offset = new Vector3(sinValue, verticalMult * Mathf.Abs(sinValue)) * multiplier;
            offset += movingOffset * velocityMag * movementScale;
        }
        else
        {
            offset = airOffset;
            rotOffset += airRotOffset;
        }

        if (velocityMag < 1f && PlayerMovement.Grounded) offset *= velocityMag;

        if (PlayerMovement.Crouched)
        {
            CrouchOffsets offsets = data.crouchOffsets;

            float influence = IsInADS ? MaxADSInfluence : 0f;
            Vector3 crouchOffset = Vector3.Lerp(offsets.pos, Vector3.zero, influence);
            offset += crouchOffset;
            rotOffset += Vector3.Lerp(offsets.rot, offsets.rot_aim, influence);
        }

        Vector3 leanRot = new Vector3(FPSCamera.VerticalDip * 20, 0, FPSCamera.CurrentLean * 0.8f);
        rotOffset += leanRot;

        float verticalDip = FPSCamera.VerticalDip * 0.1f;
        offset -= new Vector3(0, verticalDip, verticalDip);

        Footsteps.Calculate(sinValue, velocityMag, ref time);

        if (IsInADS) offset *= adsBobSwayMultiplier;

        float desiredSmoothSpeed = IsInADS ? aimingSmoothSpeed : defaultSmoothSpeed;
        smoothSpeed = Mathf.Lerp(smoothSpeed, desiredSmoothSpeed, Time.deltaTime * 5);
        // Un-ADS-ing while crouched was very slow

        movementBobObj.localPosition = Vector3.Lerp(movementBobObj.localPosition, offset, Time.deltaTime * smoothSpeed);
        movementBobObj.localRotation = Quaternion.Slerp(movementBobObj.localRotation, Quaternion.Euler(rotOffset), Time.deltaTime * smoothSpeed);
        //Vector3 movement;
    }

    private void ADS()
    {
        if (!IsInADS)
        {
            adsObj.localPosition = Vector3.Lerp(adsObj.localPosition, Vector3.zero, Time.deltaTime * adsSpeed);
            return;
        }

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);
        adsObj.localPosition = Vector3.Lerp(adsObj.localPosition, WeaponManager.CurrentWeapon.GetADSOffset() * MaxADSInfluence, Time.deltaTime * adsSpeed * data.adsSpeed);
    }

    public static bool IsInADS_Method()
    {
        bool wantsToADS = Input.GetKey(Inputs.ADS) || instance.debugAlwaysADS;
        bool grounded = PlayerMovement.Grounded;

        return wantsToADS && grounded;
    }

    [System.Serializable]
    public class CrouchOffsets
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 rot_aim;
    }
}
