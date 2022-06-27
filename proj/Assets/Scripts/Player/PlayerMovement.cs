using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//https://answers.unity.com/questions/1358491/character-controller-slide-down-slope.html

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    private void Awake()
    {
        instance = this;
    }

    private CharacterController controller;

    public MovementProfile movementProfile;

    public LayerMask groundLayerMask;
    public LayerMask crouchBlockLayermask;
    public bool forceCrouch;

    private Vector3 groundNormal;
    private bool grounded;
    private bool wasGrounded;
    private bool groundNear;

    private float y;

    private float timeOnSlope;
    private float timeOnRamp;
    private float timeSinceLastJump;
    private float jumpReduction;

    private float groundAngle;

    const float DOWNFORCE = 3f;
    //float slopeMult;

    Vector3 desiredVelocity;
    Vector3 bonusSlideVelocity;
    Vector3 moveVelocity;
    Vector3 actualVelocity;
    Vector3 lastPos;
    float slopeTime;

    #region Constants

    //Jump
    const float JumpFalloff = 0.65f;
    const float JumpFalloffMul = 1.40f;
    const float JumpMaxFalloff = 3.25f;
    const float JumpChargeTime = 0.4f;
    const float JumpCamDip = 1.2f;

    //Crouch
    const float CrouchRaySize = 0.4f;
    const float CrouchRayLength = 1f;
    const float StandingHeight = 2f;
    const float CrouchingHeight = 1f;
    const float CrouchHeightDif = StandingHeight - CrouchingHeight;

    //Grounded
    const float GroundedSphereRadius = 0.475f;
    const float GroundedSphereDist = 0.7f;
    const float GroundNearDist = 1.8f;
    const float NearSurfaceDist = 0.8f;
    const float NearSurfaceRadius = 0.55f;
    const float GroundedRayDist = 1.2f; // backup for sphere


    // Other
    const float BonusSlideVelocityFadeMult = 2.35f;
    const float BonusSlideVelocityMult = 2.5f;
    const float SqrSpeedToKeepSliding = 10f;
    const float SlideSpeedDecreaseMult = 2.65f;
    const float SlideMoveDirInfluence = 0.6f;
    const float CrouchHopSpeedDecrease = 1.25f;

    #endregion

    private float jumpCharge;

    private float cur_speed;
    private float cur_accel;
    private float cur_jumpHeight;

    private float airtime;

    public static event Action<float> OnLand;

    private bool crouched;

    private bool slidingFromSpeed;

    // Properties

    private bool CanRun => Input.GetKey(Inputs.Sprint) && !crouched && Inputs.VerticalNoSmooth > 0.2f && !WeaponManager.InADS;

    public static bool Crouched => instance.crouched;
    public static bool Grounded => instance.grounded;
    public static bool Moving { get; private set; }
    public static bool Running { get; private set; }
    public static bool Sliding { get; private set; }
    public static Vector3 Position => instance.transform.position;

    public static Vector3 LocalVelocity { get; private set; }
    public static Vector3 WorldVelocity { get; private set; }

    public static float NormalizedSpeed { get; private set; }
    public static float AirTime { get; private set; }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.slopeLimit = 80;
        lastPos = transform.position;
        slopeTime = 1;
        groundNormal = Vector3.up;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundNormal = hit.normal;
    }


    private void Update()
    {
        Inputs.Update();

        IncrementValues(Time.deltaTime);

        UpdateCrouched();
        UpdateSpeed();
        UpdateAcceleration();
        UpdateJumpHeight();

        Move();

        UpdateFOV();

        UpdateGrounded();

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    Time.timeScale = Time.timeScale < 0.3f ? 1f : 0.25f;

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    FPSCamera.VerticalDip += 1f;

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    AddImpulse(FPSCamera.instance.transform.forward);

        actualVelocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        SetProperties();
    }

    private void SetProperties()
    {
        WorldVelocity = actualVelocity;
        LocalVelocity = transform.InverseTransformVector(WorldVelocity);
        Moving = desiredVelocity.sqrMagnitude > 0.1f && WorldVelocity.Flattened().sqrMagnitude > 0.1f;
        Running = CanRun && Moving && LocalVelocity.z > 0.02f;
        Sliding = Grounded && /*Crouched &&*/ (bonusSlideVelocity.sqrMagnitude > 0.5f || timeOnSlope > 0.3f || timeOnRamp > 0.3f || slidingFromSpeed);
        NormalizedSpeed = Mathf.InverseLerp(movementProfile.walkingSpeed, movementProfile.runningSpeed, cur_speed);
        AirTime = airtime;
    }

    private void IncrementValues(float dt)
    {
        timeSinceLastJump += dt;
        jumpReduction -= dt;

        float fadeMult = 1f;

        float angle = Vector3.Angle(Vector3.up, groundNormal);
        if (angle > movementProfile.rampLimit)
        {
            const float UP_HILL_SLIDE_FADE = 5f;
            const float STEEPNESS_FADE_FACTOR = 3f;

            float hillAmount = Mathf.InverseLerp(movementProfile.rampLimit, controller.slopeLimit, angle);
            hillAmount += 1f; // rebound from 0-1 to 1-2
            hillAmount *= STEEPNESS_FADE_FACTOR; // rebound to 1-2 * STEEPNESS_FADE_FACTOR

            Vector3 slopeHorDir = groundNormal.Flattened().normalized;
            Vector3 velDir = actualVelocity.Flattened().normalized;
            float velSimilarity = Vector3.Dot(slopeHorDir, velDir);
            fadeMult = Mathf.Clamp(-velSimilarity * UP_HILL_SLIDE_FADE, 0, UP_HILL_SLIDE_FADE) * hillAmount * actualVelocity.Flattened().magnitude * 0.5f;
            //Debug.Log("Slope Fade: " + fadeMult);
        }

        //bonusSlideVelocity = Vector3.MoveTowards(bonusSlideVelocity, Vector3.zero, dt * BonusSlideVelocityFadeMult * fadeMult);

        const float LerpMul = 2f;
        bonusSlideVelocity = Vector3.Lerp(bonusSlideVelocity, Vector3.zero, dt * BonusSlideVelocityFadeMult * fadeMult * LerpMul);

        // Switched after discovering airtime was reset before multiplication
    }

    private void Move()
    {
        const float GlobalSlideReduction = 0.75f;

        const float JustJumpedThreshold = 0.4f;
        const float SlopeSpeedIncrease = 5;
        const float MaxSlopeTime = SlopeSpeedIncrease * 4.5f;

        Vector2 input = new Vector2(Inputs.HorizontalNoSmooth, Inputs.VerticalNoSmooth);
        input.Normalize();

        desiredVelocity = transform.right * input.x + transform.forward * input.y;
        desiredVelocity *= cur_speed;

        y -= movementProfile.gravity * Time.deltaTime;

        if (grounded && timeSinceLastJump > JustJumpedThreshold) y = -DOWNFORCE;

        if (grounded)
        {
            if (!wasGrounded)
            {
                // Just landed
                OnLand?.Invoke(airtime);
                float dipMult = crouched ? 0.5f : 1f;
                FPSCamera.VerticalDip += Mathf.Lerp(0.0f, 2f, airtime * 0.6f) * dipMult;
                jumpReduction = Mathf.Max(jumpReduction, Mathf.Clamp(airtime * airtime * 3, 1.25f, JumpMaxFalloff * 1.5f));
                // Reduce jump upon landing
                //airtime = 0;

                if (crouched)
                {
                    const float MinAirtimeForSlideBonus = 0.65f; // Flat ground jump takes about 0.7 sec

                    bool onSteepSlope = Vector3.Angle(Vector3.up, groundNormal) > movementProfile.slopeLimit;

                    if (actualVelocity.Flattened().sqrMagnitude > 3f && airtime > MinAirtimeForSlideBonus && !onSteepSlope)
                    {
                        float airtimeMult = Mathf.Clamp(airtime * 0.4f, 0.75f, 1.45f);
                        //bonusSlideVelocity += actualVelocity.Flattened().normalized * BonusSlideVelocityMult * airtimeMult;
                        const float MultOfOGVelocity = 0.9f; // Was 0.7 before switch to lerp

                        bonusSlideVelocity += actualVelocity.Flattened() * (MultOfOGVelocity * BonusSlideVelocityMult * airtimeMult * GlobalSlideReduction);
                        // Thanks unity analyzer for optimization tip - do scalar maths before vector
                    }
                    // Add bonus vel

                    timeOnRamp = 0.5f;
                    if (groundAngle > movementProfile.slopeLimit)
                        timeOnSlope = 0.5f;
                    // Can immediately slide down ramps
                }

                airtime = 0;
            }
        }
        else airtime += Time.deltaTime;


        // On a slope, but not above a void or something (will fall onto ground)
        if (timeOnSlope > 0.2f)// && groundNear) // Commented out to try to counteract walking up edges of slopes
        {
            if (groundNear)
                SlopeMovement();
            else
                AirSlopeMovement();
        }
        // Let player still move sideways on slopes

        // Reset bonus vel if on slope or not crouching
        if (crouched && timeOnSlope < 0.2f)
            desiredVelocity += bonusSlideVelocity;
        else
            bonusSlideVelocity = Vector3.zero;

        bool crouchingDownRamp = timeOnRamp > 0.2f && crouched;
        bool goingWithRamp = false;

        if (crouchingDownRamp)
        {
            Vector3 slopeHorDir = groundNormal.Flattened().normalized;
            Vector3 velDir = desiredVelocity.Flattened().normalized;
            Vector3 viewDir = FPSCamera.ViewDir.Flattened().normalized;

            float velSimilarity = Vector3.Dot(slopeHorDir, velDir);
            float viewSimilarity = Vector3.Dot(slopeHorDir, viewDir);

            goingWithRamp = velSimilarity > 0.2f || (viewSimilarity > 0.4f && velSimilarity > -0.2f);
            if (!grounded) goingWithRamp = false; // potential fix to phantom mid-air-crouch-gliding?

            if (!goingWithRamp) timeOnRamp = 0f;
            // If crouching down a mild incline, allow sliding
        }

        if ((timeOnSlope > 0.2f || goingWithRamp) && timeSinceLastJump > 0.6f)
        {
            // On slope and hasnt like just jumped

            //moveDir = Vector3.zero;
            Vector3 slopeHorDir = groundNormal.Flattened().normalized;

            if (groundNear)
            {
                //float crouch = 2.5f;
                float crouch = crouched ? 2.5f : 1f;
                
                Vector3 velDir = actualVelocity.Flattened().normalized;
                float velSimilarity = Vector3.Dot(slopeHorDir, velDir);

                if (velSimilarity > -0.1f)
                    slopeTime += Time.deltaTime * SlopeSpeedIncrease * crouch;
                else
                    slopeTime -= Time.deltaTime * SlopeSpeedIncrease * crouch;

                if (slopeTime > MaxSlopeTime)
                    slopeTime = MaxSlopeTime;

                if (slopeTime < 1f)
                    slopeTime = 1f;

                y = -DOWNFORCE * slopeTime;
            }
            else
                slopeTime = 1f;

            desiredVelocity.x += groundNormal.x * slopeTime * (1f - movementProfile.slideFriction);
            desiredVelocity.z += groundNormal.z * slopeTime * (1f - movementProfile.slideFriction);

            float angle = Vector3.Angle(Vector3.up, groundNormal);
            if (angle > movementProfile.rampLimit)
            {
                const float ViewInfluence = 1.25f;
                const float MaxViewInfluence = 2.5f;
                const float Mult = 0.2f;

                Vector3 viewDir = FPSCamera.ViewDir.Flattened().normalized;
                Vector3 slopeSide = Vector3.Cross(slopeHorDir, Vector3.up);
                float viewSimilarity = Vector3.Dot(slopeSide, viewDir);

                float mul = ViewInfluence * Mathf.Abs(viewSimilarity) * slopeTime * Mult;
                mul = Mathf.Clamp(mul, 0, MaxViewInfluence);

                desiredVelocity.x += viewDir.x * mul;
                desiredVelocity.z += viewDir.z * mul;
            }
        }
        else
        {
            slopeTime = 1f;
        }

        if (wasGrounded && !grounded && timeSinceLastJump > JustJumpedThreshold)
        {
            // Left ground (not from a jump, otherwise why cancel y velocity)
            //y += DOWNFORCE; // counteract downforce, set y to 0
            y = 0; // didn't work as downforce was set multiplied with downforce earlier, just set to 0
            jumpCharge = 0; // cancel any pending jump
        }

        if (grounded && Input.GetKeyDown(Inputs.Jump) && timeSinceLastJump > JustJumpedThreshold && timeOnSlope < 0.2f && jumpCharge <= 0)
        {
            //float timeMult = crouched ? 2f : 1f;
            jumpCharge = JumpChargeTime;// * timeMult;
            float dip = JumpCamDip;
            dip = Mathf.Clamp(dip - jumpReduction * 0.3f, 0.1f, JumpCamDip);
            float dipMult = crouched ? 0.5f : 1f;
            //float dipMult = crouched ? 2.5f : 1f; // now crouch jumping makes you stand up
            FPSCamera.VerticalDip += dip * dipMult;
        }

        if (jumpCharge > 0)
        {
            jumpCharge -= Time.deltaTime;

            if (jumpCharge <= 0)
            {
                Jump();
            }
        }

        //moveDir.y = y;

        Vector3 flatVel = actualVelocity.Flattened();
        //if (actualVelocity.y > 0)
        //y = actualVelocity.y;
        //this.moveVelocity.y = actualVelocity.y;

        //moveVelocity = Vector3.Lerp(flatVel, desiredVelocity, Time.deltaTime * cur_accel).WithY(y);
        moveVelocity = Vector3.Lerp(flatVel, desiredVelocity, Time.deltaTime * cur_accel).WithY(0);

        slidingFromSpeed = false;

        // New velocity is slower than current vel
        if (moveVelocity.sqrMagnitude < flatVel.sqrMagnitude)
        {
            if (crouched && grounded && flatVel.sqrMagnitude > SqrSpeedToKeepSliding)
            {
                // Sliding on ground
                slidingFromSpeed = true;

                const float GroundNormalDirectionInfluence = 0.4f;
                Vector3 normalMove = groundNormal.Flattened() * GroundNormalDirectionInfluence;
                Vector3 moveDir = Vector3.Lerp(flatVel.normalized, (moveVelocity.normalized + normalMove).normalized, Time.deltaTime * SlideMoveDirInfluence);

                // Decrease speed if moving up slope

                float fadeMult = 1f;
                const float HILL_FADE_EFFECT = 0.03f;

                float angle = Vector3.Angle(Vector3.up, groundNormal);
                if (angle > movementProfile.rampLimit)
                {
                    const float UP_HILL_SLIDE_FADE = 5f;
                    const float STEEPNESS_FADE_FACTOR = 3f;

                    float hillAmount = Mathf.InverseLerp(movementProfile.rampLimit, controller.slopeLimit, angle);
                    hillAmount += 1f; // rebound from 0-1 to 1-2
                    hillAmount *= STEEPNESS_FADE_FACTOR; // rebound to 1-2 * STEEPNESS_FADE_FACTOR

                    Vector3 slopeHorDir = groundNormal.Flattened().normalized;
                    Vector3 velDir = actualVelocity.Flattened().normalized;
                    float velSimilarity = Vector3.Dot(slopeHorDir, velDir);
                    fadeMult = Mathf.Clamp(-velSimilarity * UP_HILL_SLIDE_FADE, 0, UP_HILL_SLIDE_FADE) * hillAmount * actualVelocity.Flattened().magnitude * HILL_FADE_EFFECT;
                    //Debug.Log("Slope Fade: " + fadeMult);
                }

                const float InverseGlobalReduction = 1f / GlobalSlideReduction;
                moveVelocity = moveDir * (flatVel.magnitude - Time.deltaTime * SlideSpeedDecreaseMult * fadeMult * InverseGlobalReduction);
            }
            else if (crouched && !grounded)
                moveVelocity = moveVelocity.normalized * (flatVel.magnitude - Time.deltaTime * CrouchHopSpeedDecrease);
        }
        // Keep velocity when sliding fast enough

        moveVelocity.y = y;

        controller.Move(moveVelocity * Time.deltaTime);


        // PHANTOM GHOST CROUCH FLOATING:
        // ground normal not being reset when crouching in air
    }

    private void SlopeMovement()
    {
        // Normal slope movement
        Vector3 slopeHorDir = groundNormal.Flattened().normalized;
        Vector3 slopeSide = Vector3.Cross(slopeHorDir, Vector3.up);
        Vector3 velDir = desiredVelocity.Flattened().normalized;

        float similarity = Vector3.Dot(slopeSide, velDir);
        similarity *= similarity;
        
        const float MaxControl = 0.7f;
        float crouchFactor = crouched ? 1.75f : 1f;

        float control = Mathf.Clamp(Mathf.Abs(similarity) * movementProfile.slopeAccelPercent * slopeTime * crouchFactor, 0.1f, MaxControl * crouchFactor);
        desiredVelocity *= control;
    }

    private void AirSlopeMovement()
    {
        // Slope movement with no ground beneath, like say walking on a fence top
        Vector3 slopeHorDir = groundNormal.Flattened().normalized;
        // ^^^ points directly towards edge
        Vector3 velDir = desiredVelocity.Flattened().normalized;

        float similarity = Vector3.Dot(slopeHorDir, velDir);
        //similarity *= similarity;

        const float MaxControl = 0.9f;
        float crouchFactor = crouched ? 1.75f : 1f;

        float control = Mathf.Clamp(Mathf.Abs(similarity) * movementProfile.slopeAccelPercent * slopeTime * crouchFactor, 0.1f, MaxControl * crouchFactor);
        desiredVelocity *= control;
    }

    private void Jump()
    {
        if (grounded && timeSinceLastJump > 0.4f && timeOnSlope < 0.2f && !crouched)
        {
            // Jump!
            AudioManager.Play(new Audio("Jump").SetPosition(transform.position.WithY(transform.position.y - 0.4f)));
            //AudioManager.Play(AudioArray.Jump, transform.position.WithY(transform.position.y - 0.4f));

            jumpReduction = Mathf.Max(1, jumpReduction);
            float calibratedHeight = cur_jumpHeight / Mathf.Min(jumpReduction, JumpFalloffMul);
            float red = (jumpReduction + JumpFalloff) * JumpFalloffMul;
            //if (red < JUMP_MAX_FALLOFF)
            if (jumpReduction < JumpMaxFalloff)
                jumpReduction = red;

            y += DOWNFORCE + calibratedHeight;
            timeSinceLastJump = 0;
        }

        jumpCharge = 0;
    }

    private void UpdateFOV()
    {
        float value = Moving && Running ? NormalizedSpeed : 0f;
        float runMultiplier = value * 0.2f; // Will go between 0 and 0.2

        float aimMult = WeaponManager.InADS ? WeaponManager.CurrentWeapon.GetADSFov() : 1f;

        CameraFOV.Set(aimMult + runMultiplier);
    }



    private void UpdateSpeed()
    {
        float target = movementProfile.walkingSpeed;

        if (crouched) target = movementProfile.crouchSpeed;
        else if (CanRun) target = movementProfile.runningSpeed;
        //else if (CanRun) target = no weapon equipped ? movementProfile.unarmedRunSpeed : movementProfile.runningSpeed;

        if (!grounded) target *= crouched ? movementProfile.crouchAirSpeedMultiplier : movementProfile.airSpeedMultiplier;

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);
        if (WeaponManager.InADS) target *= data.adsMoveSpeedMult;
        target *= data.speedMult;

        cur_speed = Mathf.Lerp(cur_speed, target, Time.deltaTime * movementProfile.speedLerpSpeed);
    }

    private void UpdateAcceleration()
    {
        float target = grounded ? movementProfile.groundAcceleration : movementProfile.airAcceleration;

        cur_accel = Mathf.Lerp(cur_accel, target, Time.deltaTime * movementProfile.accelLerpSpeed);
    }

    private void UpdateJumpHeight()
    {
        float target = CanRun ? movementProfile.runningJumpHeight : movementProfile.walkingJumpHeight;

        WeaponData data = Weapons.Get(WeaponManager.CurrentWeaponType);
        target *= data.jumpMult;

        cur_jumpHeight = Mathf.Lerp(cur_jumpHeight, target, Time.deltaTime * movementProfile.jumpLerpSpeed);
    }

    private void UpdateCrouched()
    {
        Vector3 pos = Vector3.up * (CrouchRayLength - CrouchRaySize);

        if (Physics.Raycast(transform.position, Vector3.up, CrouchRayLength, crouchBlockLayermask)
            || Physics.CheckSphere(transform.position + pos, CrouchRaySize, crouchBlockLayermask))
            crouched = true;

        else
            crouched = Input.GetKey(Inputs.Crouch) && jumpCharge <= 0;

        crouched |= forceCrouch;

        const float CrouchJumpReduction = 2.25f;

        if (crouched)
            jumpReduction = Mathf.Max(jumpReduction, CrouchJumpReduction);

        float desiredControllerHeight = crouched ? CrouchingHeight : StandingHeight;

        //controller.height = Mathf.Lerp(controller.height, desiredControllerHeight, Time.deltaTime * crouchChangeSpeed);
        controller.height = Mathf.MoveTowards(controller.height, desiredControllerHeight, Time.deltaTime * movementProfile.crouchChangeSpeed);
        float offset = Mathf.Lerp(CrouchHeightDif * 0.5f, 0, Mathf.InverseLerp(CrouchingHeight, StandingHeight, controller.height));
        controller.center = new Vector3(0, -offset, 0);

        FPSCamera.Crouched = crouched;
    }

    private void UpdateGrounded()
    {
        groundAngle = Vector3.Angle(Vector3.up, groundNormal);

        if (groundAngle > movementProfile.slopeLimit)
            timeOnSlope += Time.deltaTime;
        else
            timeOnSlope = 0;

        if (groundAngle > movementProfile.rampLimit && crouched)
            timeOnRamp += Time.deltaTime;
        else
            timeOnRamp = 0f;
        //grounded = !onSlope;

        wasGrounded = grounded;
        RaycastHit hit;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), GroundedSphereRadius, out hit, GroundedSphereDist, groundLayerMask)
            || Physics.Raycast(transform.position, Vector3.down, out hit, GroundedRayDist, groundLayerMask);

        if (grounded)
            groundNormal = hit.normal;

        groundNear = Physics.Raycast(new Ray(transform.position, Vector3.down), GroundNearDist, groundLayerMask);

        if (!Physics.CheckSphere(transform.position + Vector3.down * NearSurfaceDist, NearSurfaceRadius, groundLayerMask))
            groundNormal = Vector3.up;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.down * GroundedSphereDist, GroundedSphereRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * GroundedRayDist);
        // Grounded

        if (controller == null)
            controller = GetComponent<CharacterController>();

        Vector3 pos = Vector3.up * (CrouchRayLength - CrouchRaySize);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + pos, CrouchRaySize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * CrouchRayLength);
        //Crouch
    }


    public void AddImpulse(Vector3 force)
    {
        lastPos -= force;
        y += force.y * 10;
    }
}
