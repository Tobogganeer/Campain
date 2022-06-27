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

    public Transform temp_weaponRotBone;

    [Header("Mouse")]
    public bool invertMouse = true;
    public float mouseSwayAmount = 0.03f;
    public float mouseMaxAmount = 0.06f;
    public float mouseSmoothAmount = 12f;
    public float mouseRotAmount = 1f;
    public float mouseRotMaxAmount = 5f;
    public float mouseRotZMult = 3f;

    [Header("Movement")]
    public bool invertMovement = false;
    public float movementSwayAmount = 0.04f;
    public float movementMaxAmount = 0.1f;
    public float movementSwayRotAmountZ = 2f;
    public float movementSwayRotAmountY = 0.2f;
    public float movementSwayMaxRot = 4f;
    public float movementSmoothAmount = 5f;
    public float verticalMultiplier = 1.5f;

    [Header("Movement Bob")]
    public bool bobUp = true;
    public float bobSpeed = 2.5f;
    public float bobAmount = 0.03f;
    public Vector3 bobRotAmount = new Vector3(0, 0.25f, 1.8f);
    public Vector3 airOffset = new Vector3(0, -0.1f, -0.05f);
    public float airNoiseScale = 1f;
    public float airNoiseInfluence = 0.01f;
    public Vector3 airRotOffset = new Vector3(15, 0, 0f);
    public Vector3 movingOffset = new Vector3(0, -0.01f, -0.01f);
    public float maxMovingOffset = 1f;
    public float runningMultiplier = 1.5f;
    public float defaultSmoothSpeed = 5;
    public float aimingSmoothSpeed = 15;
    public AnimationCurve moveBobCurve;

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
    public static float SinValue { get; private set; }

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


        Vector3 rotMovement = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), Input.GetAxis("Mouse X")) * invert * mouseRotAmount;
        //Debug.Log("Mouse: " + Input.GetAxis("Mouse X") + " " + Input.GetAxis("Mouse Y"));

        rotMovement.x = Mathf.Clamp(rotMovement.x, -mouseRotMaxAmount, mouseRotMaxAmount);
        rotMovement.y = Mathf.Clamp(rotMovement.y, -mouseRotMaxAmount, mouseRotMaxAmount);
        rotMovement.z = Mathf.Clamp(rotMovement.z * mouseRotZMult, -mouseRotMaxAmount * mouseRotZMult, mouseRotMaxAmount * mouseRotZMult);

        if (IsInADS) rotMovement *= adsBobSwayMultiplier * 5;

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

        float angleZ = movementSwayRotAmountZ * PlayerMovement.LocalVelocity.x;
        float angleY = movementSwayRotAmountY * PlayerMovement.LocalVelocity.x;
        angleZ = Mathf.Clamp(angleZ, -movementSwayMaxRot, movementSwayMaxRot);
        angleY = Mathf.Clamp(angleY, -movementSwayMaxRot, movementSwayMaxRot);

        if (IsInADS) desiredMovement *= adsBobSwayMultiplier;

        movementSwayObj.localPosition = Vector3.Lerp(movementSwayObj.localPosition, desiredMovement, Time.deltaTime * movementSmoothAmount);
        movementSwayObj.localRotation = Quaternion.Slerp(movementSwayObj.localRotation, Quaternion.Euler(0, angleY, -angleZ), Time.deltaTime * movementSmoothAmount);
    }

    private void MovementBob()
    {
        Vector3 actualHorizontalVelocity = PlayerMovement.LocalVelocity.Flattened();

        float velocityMag = actualHorizontalVelocity.magnitude;

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);

        time += Time.deltaTime * bobSpeed * velocityMag * data.bobSpeedMult;

        SinValue = Mathf.Sin(time);
        Vector3 offset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;

        if (PlayerMovement.Grounded)
        {
            float runningMult = Mathf.Lerp(1, runningMultiplier, PlayerMovement.NormalizedSpeed);
            float crouchingMult = PlayerMovement.Crouched ? crouchingMultiplier : 1f;

            float movementScale = Mathf.InverseLerp(0, PlayerMovement.instance.movementProfile.runningSpeed, velocityMag);
            float multiplier = bobAmount * runningMult * crouchingMult * data.bobAmountMult;
            if (!PlayerMovement.Sliding)
            {
                offset = GetMovementOffset() * multiplier;
                rotOffset += GetMovementRot() * runningMult * crouchingMult * Mathf.Clamp01(velocityMag) * adsBobSwayMultiplier;
                //rotOffset += bobRotAmount * SinValue * runningMult * crouchingMult;
            }
            offset += movingOffset * Mathf.Min(velocityMag, maxMovingOffset) * movementScale;
        }
        else
        {
            offset = airOffset;
            float noise = Mathf.PerlinNoise(PlayerMovement.AirTime * airNoiseScale, PlayerMovement.AirTime * 2.5124f * airNoiseScale);
            offset += new Vector3(1, 1) * noise * airNoiseInfluence;
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

        float verticalDip = FPSCamera.VerticalDip * 0.1f;
        offset -= new Vector3(0, verticalDip, verticalDip);

        Footsteps.Calculate(SinValue, velocityMag, ref time);

        if (IsInADS) offset *= adsBobSwayMultiplier;

        // After aim reduc
        Vector3 leanRot = new Vector3(FPSCamera.VerticalDip * 20, 0, FPSCamera.CurrentLean * 1.3f);
        rotOffset += leanRot;

        float desiredSmoothSpeed = IsInADS ? aimingSmoothSpeed : defaultSmoothSpeed;
        smoothSpeed = Mathf.Lerp(smoothSpeed, desiredSmoothSpeed, Time.deltaTime * 5);
        // Un-ADS-ing while crouched was very slow

        movementBobObj.localPosition = Vector3.Lerp(movementBobObj.localPosition, offset, Time.deltaTime * smoothSpeed);
        movementBobObj.localRotation = Quaternion.Slerp(movementBobObj.localRotation, Quaternion.Euler(rotOffset), Time.deltaTime * smoothSpeed);
        //Vector3 movement;
    }

    private void LateUpdate()
    {
        //temp_weaponRotBone.localRotation = Quaternion.Euler(movementBobObj.localEulerAngles * 20 + Vector3.up * 90);
    }

    private Vector3 vec_move_left = new Vector3(-0.8f, 0.1f, 0);
    private Vector3 vec_move_center = new Vector3(0, -0.25f, -0.5f);
    private Vector3 vec_move_right = new Vector3(1, -0.1f, 0);

    private Vector3 vec_rot_left = new Vector3(0, 0.5f, -1.5f);
    private Vector3 vec_rot_center = new Vector3(1, 0, 0);
    private Vector3 vec_rot_right = new Vector3(0, -1, 3);

    private const float NEW_BOB_MULT = 1f;

    private Vector3 GetMovementOffset()
    {
        //float verticalMult = bobUp ? -1 : 1;
        //float normalizedSin = (SinValue + 1) * 0.5f;
        //if (Footsteps.foot == Foot.Right)
        //    normalizedSin = 1 - normalizedSin;
        //return new Vector3(SinValue, verticalMult * Mathf.Abs(SinValue)) * mult;
        //return new Vector3(SinValue, verticalMult * moveBobCurve.Evaluate(normalizedSin));
        if (SinValue < 0)
            return Vector3.Lerp(vec_move_left, vec_move_center, SinValue + 1) * NEW_BOB_MULT;
        else
            return Vector3.Lerp(vec_move_center, vec_move_right, SinValue) * NEW_BOB_MULT;
    }

    private Vector3 GetMovementRot()
    {
        if (SinValue < 0)
            return Vector3.Lerp(vec_rot_left, vec_rot_center, SinValue + 1) * NEW_BOB_MULT;
        else
            return Vector3.Lerp(vec_rot_center, vec_rot_right, SinValue) * NEW_BOB_MULT;
    }

    Vector3 adsVel;

    private void ADS()
    {
        if (!IsInADS)
        {
            adsObj.localPosition = Vector3.Lerp(adsObj.localPosition, Vector3.zero, Time.deltaTime * adsSpeed);
            //adsObj.localPosition = Vector3.SmoothDamp(adsObj.localPosition, Vector3.zero, ref adsVel, 0.1f, adsSpeed);
            return;
        }

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);
        adsObj.localPosition = Vector3.Lerp(adsObj.localPosition, WeaponManager.CurrentWeapon.GetADSOffset() * MaxADSInfluence, Time.deltaTime * adsSpeed * data.adsSpeed);
        //adsObj.localPosition = Vector3.SmoothDamp(adsObj.localPosition, WeaponManager.CurrentWeapon.GetADSOffset() * MaxADSInfluence, ref adsVel, 0.1f, adsSpeed * data.adsSpeed);
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
