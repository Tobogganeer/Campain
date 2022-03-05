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

    public float speed = 3;
    public float crouchMult = 0.45f;
    public float jumpHeight = 5.3f;
    public float slopeLimit = 40;
    public float rampLimit = 15;
    public float slideFriction = 0.3f;
    public float gravity = 14f;
    public float acceleration = 7f;
    public float airAcceleration = 1.5f;
    public float slopeAccelPercent = 0.4f;
    public LayerMask crouchBlockLayermask;

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
    const float ChangeSpeed = 4f;

    //Grounded
    const float GroundedSphereRadius = 0.475f;
    const float GroundedSphereDist = 0.7f;
    const float GroundNearDist = 1.8f;
    const float NearSurfaceDist = 0.8f;
    const float NearSurfaceRadius = 0.55f;


    // Other
    const float BonusSlideVelocityFadeMult = 2f;
    const float BonusSlideVelocityMult = 2.5f;
    const float SqrSpeedToKeepSliding = 15f;
    const float SlideSpeedDecreaseMult = 1.65f;
    const float SlideMoveDirInfluence = 0.6f;

    #endregion

    private float jumpCharge;

    private float cur_speed;
    private float cur_accel;
    private float cur_jumpHeight;

    private float airtime;

    public static event Action<float> OnLand;

    private bool crouched;

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

        UpdateGrounded();

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    FPSCamera.VerticalDip += 1f;

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    AddImpulse(FPSCamera.instance.transform.forward);

        actualVelocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    private void IncrementValues(float dt)
    {
        timeSinceLastJump += dt;
        jumpReduction -= dt;
        bonusSlideVelocity = Vector3.MoveTowards(bonusSlideVelocity, Vector3.zero, dt * BonusSlideVelocityFadeMult);
    }

    private void Move()
    {
        const float JustJumpedThreshold = 0.4f;
        const float SlopeSpeedIncrease = 5;

        Vector2 input = new Vector2(Inputs.HorizontalNoSmooth, Inputs.VerticalNoSmooth);
        input.Normalize();

        desiredVelocity = transform.right * input.x + transform.forward * input.y;
        desiredVelocity *= cur_speed;

        y -= gravity * Time.deltaTime;

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
                airtime = 0;

                if (crouched)
                {
                    if (actualVelocity.Flattened().sqrMagnitude > 3f)
                    {
                        float airtimeMult = Mathf.Clamp(airtime, 0.5f, 2.5f);
                        //bonusSlideVelocity += actualVelocity.Flattened().normalized * BonusSlideVelocityMult * airtimeMult;
                        const float MultOfOGVelocity = 0.7f;

                        // TODO: Multiply bonus vel by decimal if going up a slope

                        bonusSlideVelocity += actualVelocity.Flattened() * (MultOfOGVelocity * BonusSlideVelocityMult * airtimeMult);
                        // Thanks unity analyzer for optimization tip - do scalar maths before vector
                    }
                    // Add bonus vel

                    timeOnRamp = 0.5f;
                    if (groundAngle > slopeLimit)
                        timeOnSlope = 0.5f;
                    // Can immediately slide down ramps
                }
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
            if (groundNear)
            {
                //float crouch = 2.5f;
                float crouch = crouched ? 2.5f : 1f;
                slopeTime += Time.deltaTime * SlopeSpeedIncrease * crouch;
                y = -DOWNFORCE * slopeTime;
            }
            else
                slopeTime = 1f;

            desiredVelocity.x += groundNormal.x * slopeTime * (1f - slideFriction);
            desiredVelocity.z += groundNormal.z * slopeTime * (1f - slideFriction);
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
            jumpCharge = JumpChargeTime;
            float dip = JumpCamDip;
            dip = Mathf.Clamp(dip - jumpReduction * 0.3f, 0.1f, JumpCamDip);
            float dipMult = crouched ? 0.5f : 1f;
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

        // New velocity is slower than current vel
        if (moveVelocity.sqrMagnitude < flatVel.sqrMagnitude)
        {
            if (crouched && grounded && flatVel.sqrMagnitude > SqrSpeedToKeepSliding)
            {
                // Sliding on ground
                Vector3 moveDir = Vector3.Lerp(flatVel.normalized, moveVelocity.normalized, Time.deltaTime * SlideMoveDirInfluence);
                moveVelocity = moveDir * (flatVel.magnitude - Time.deltaTime * SlideSpeedDecreaseMult);
            }
            else if (crouched && !grounded)
                moveVelocity = moveVelocity.normalized * flatVel.magnitude;
        }
        // Keep velocity when sliding fast enough

        moveVelocity.y = y;

        controller.Move(moveVelocity * Time.deltaTime);
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

        float control = Mathf.Clamp(Mathf.Abs(similarity) * slopeAccelPercent * slopeTime * crouchFactor, 0.1f, MaxControl * crouchFactor);
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

        float control = Mathf.Clamp(Mathf.Abs(similarity) * slopeAccelPercent * slopeTime * crouchFactor, 0.1f, MaxControl * crouchFactor);
        desiredVelocity *= control;
    }

    private void Jump()
    {
        if (grounded && timeSinceLastJump > 0.4f && timeOnSlope < 0.2f)
        {
            // Jump!
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


    private void UpdateSpeed()
    {
        // Lerping?
        cur_speed = speed;
        if (crouched) cur_speed *= crouchMult;

        if (Input.GetKey(Inputs.Sprint)) cur_speed *= 1.45f;
    }

    private void UpdateAcceleration()
    {
        // change crouched to sliding
        //cur_accel = grounded ? crouched ? slideAcceleration : acceleration : airAcceleration;
        cur_accel = grounded ? acceleration : airAcceleration;
    }

    private void UpdateJumpHeight()
    {
        cur_jumpHeight = jumpHeight;
        if (crouched) cur_jumpHeight *= crouchMult;
    }

    private void UpdateCrouched()
    {
        Vector3 pos = Vector3.up * (CrouchRayLength - CrouchRaySize);

        if (Physics.Raycast(transform.position, Vector3.up, CrouchRayLength, crouchBlockLayermask)
            || Physics.CheckSphere(transform.position + pos, CrouchRaySize, crouchBlockLayermask))
            crouched = true;

        else
            crouched = Input.GetKey(Inputs.Crouch);

        float desiredControllerHeight = crouched ? CrouchingHeight : StandingHeight;

        //controller.height = Mathf.Lerp(controller.height, desiredControllerHeight, Time.deltaTime * crouchChangeSpeed);
        controller.height = Mathf.MoveTowards(controller.height, desiredControllerHeight, Time.deltaTime * ChangeSpeed);
        float offset = Mathf.Lerp(CrouchHeightDif * 0.5f, 0, Mathf.InverseLerp(CrouchingHeight, StandingHeight, controller.height));
        controller.center = new Vector3(0, -offset, 0);

        FPSCamera.Crouched = crouched;
    }

    private void UpdateGrounded()
    {
        groundAngle = Vector3.Angle(Vector3.up, groundNormal);

        if (groundAngle > slopeLimit)
            timeOnSlope += Time.deltaTime;
        else
            timeOnSlope = 0;

        if (groundAngle > rampLimit && crouched)
            timeOnRamp += Time.deltaTime;
        else
            timeOnRamp = 0f;
        //grounded = !onSlope;

        wasGrounded = grounded;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), GroundedSphereRadius, GroundedSphereDist);

        groundNear = Physics.Raycast(new Ray(transform.position, Vector3.down), GroundNearDist);

        if (!Physics.CheckSphere(transform.position + Vector3.down * NearSurfaceDist, NearSurfaceRadius))
            groundNormal = Vector3.up;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.down * GroundedSphereDist, GroundedSphereRadius);
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
