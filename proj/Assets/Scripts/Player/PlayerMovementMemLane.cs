using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementMemLane : MonoBehaviour
{
    public static PlayerMovementMemLane instance;
    private void Awake()
    {
        instance = this;
    }

    private CharacterController controller;
    public Player player;

    public CharacterController Controller => instance.controller;

    // Tooltips show text when you hover the cursor above the var in the inspector

    #region Public Variables
    [Header("Change")]
    public MovementProfile movementProfile;
    //[Min(0f)] public float walkingSpeed = 4;
    //[Min(0f)] public float runningSpeed = 6;
    [Min(0f)] public float speedChangeSmoothing = 5;
    private float currentSpeed;

    //public float gravity = 10;
    //
    //[Min(0f)] public float walkingJumpHeight = 3;
    //[Min(0f)] public float runningJumpHeight = 4;
    private float currentJumpHeight;
    private float secondsFromJump;
    private bool justJumped;

    //[Min(0f)] public float groundAcceleration = 12;
    //[Min(0f)] public float airAcceleration = 1;

    public float edgePushMultiplier = 0.5f;
    public bool disableCrouchHopping;


    [Header("Don't change")]
    public float groundedRaycastSize = 0.5f;
    public float groundedRaycastLength = 0.6f;
    public float downforceCheckSize = 0.2f;
    public float downforceCheckLength = 0.5f;
    [Min(10f)] public float maxFallSpeed = 35;
    public float downForce = 3;

    public LayerMask groundLayermask;

    [Space]
    public float crouchRaycastSize = 0.5f;
    public float crouchRaycastLength = 0.6f;
    public LayerMask crouchBlockLayermask;
    public float standingControllerHeight = 2;
    public float crouchingControllerHeight = 1;
    public float crouchChangeSpeed = 5;
    #endregion


    public bool grounded { get; private set; }
    public bool wasGrounded { get; private set; }
    public bool applyDownforce { get; private set; }

    private Vector3 currentVelocity;
    private Vector3 desiredVelocity;
    private Vector3 transformedDesiredVelocity;
    private Vector3 actualVelocity;

    private Vector3 lastPos;

    /// <summary>
    /// The the velocity that the controller will move. Includes downforce and gravity
    /// </summary>
    public Vector3 CurrentVelocity => currentVelocity;
    /// <summary>
    /// The desired velocity in world space
    /// </summary>
    public Vector3 DesiredVelocity => desiredVelocity;
    /// <summary>
    /// The desired velocity in local space, with speed applied
    /// </summary>
    public Vector3 TransformedDesiredVelocity => transformedDesiredVelocity;
    /// <summary>
    /// The difference between the controllers last and current position
    /// </summary>
    public Vector3 ActualVelocity => actualVelocity;
    /// <summary>
    /// The actual velocity of the character, in local space
    /// </summary>
    public Vector3 LocalActualVelocity => transform.InverseTransformDirection(actualVelocity);

    public bool Moving => actualVelocity.Flattened().sqrMagnitude > 0.05f && desiredVelocity.sqrMagnitude > 0.05f;
    public bool Sprinting => Input.GetKey(Inputs.Sprint) && (!Crouched || !grounded);
    public bool WantsToCrouch => Input.GetKey(Inputs.Crouch);
    public bool Crouched => crouched;

    /// <summary>
    /// A value between 0-1 depending on if the player is walking or running
    /// </summary>
    public float NormalizedSpeed => Mathf.InverseLerp(movementProfile.walkingSpeed, movementProfile.runningSpeed, currentSpeed);

    /// <summary>
    /// A value between 0-1, where 0 is stationary and 1 is moving current max speed (walk speed or sprint speed, dependantly).
    /// </summary>
    public float FromStillToMaxSpeed01 => Mathf.InverseLerp(-currentSpeed, currentSpeed, currentVelocity.Flattened().magnitude) * 2 - 1;

    public static event Action<float> OnLand;
    private float airtime;
    private Vector3 currentNormal;

    private bool crouched;

    public bool inspector_crouching; // for inspector

    private const float CROUCH_ANTI_JITTER_FORCE = 0.02f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        lastPos = transform.position;
    }

    private void Update()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        Inputs.Update();

        UpdateCrouched();

        UpdateGrounded();

        UpdateSpeed();

        Move();

        UpdateFOV();

        wasGrounded = grounded;
        actualVelocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    private void Move()
    {
        //desiredVelocity.x = Input.GetAxis("Horizontal");
        //desiredVelocity.z = Input.GetAxis("Vertical");
        desiredVelocity.x = Inputs.HorizontalNoSmooth;
        desiredVelocity.z = Inputs.VerticalNoSmooth;
        // Sets the desired velocity

        if (controller.height < standingControllerHeight - 0.05f && !crouched) desiredVelocity.z += CROUCH_ANTI_JITTER_FORCE;
        // When standing up, weird jitter happens unless you are moving any amount

        transformedDesiredVelocity = transform.right * desiredVelocity.x + transform.forward * desiredVelocity.z;
        transformedDesiredVelocity = Vector3.ClampMagnitude(transformedDesiredVelocity, 1);
        transformedDesiredVelocity *= currentSpeed;

        float accel = grounded ? movementProfile.groundAcceleration : movementProfile.airAcceleration;

        float y = currentVelocity.y;

        //Vector3 flatVel = currentVelocity.Flattened();
        Vector3 flatVel = actualVelocity.Flattened();
        if (actualVelocity.y > 0)
            currentVelocity.y = actualVelocity.y;

        currentVelocity = Vector3.Lerp(flatVel, transformedDesiredVelocity, Time.deltaTime * accel).WithY(y);

        bool jump = Input.GetKeyDown(Inputs.Jump);

        if (grounded)
        {
            if (!wasGrounded)
            {
                OnLand?.Invoke(airtime);
                airtime = 0;
            }

            if (jump)
            {
                float movingJumpBonus = Mathf.Clamp(desiredVelocity.sqrMagnitude, 0, 0.5f); // 0.5 unit bonus if you are moving
                currentVelocity.y = currentJumpHeight + movingJumpBonus; // If you are grounded and want to jump, jump
                justJumped = true;
            }
            else if (applyDownforce && !justJumped && controller.height > standingControllerHeight - 0.1f) currentVelocity.y = -downForce; // Otherwise, apply downforce
            // Controller has some difficulty standing from crouch when stationary, maybe this will fix it
        }
        else
        {
            airtime += Time.deltaTime;
        }

        if (justJumped) secondsFromJump += Time.deltaTime;

        if (secondsFromJump > 0.2f)
        {
            justJumped = false;
            secondsFromJump = 0;
        }

        currentVelocity.y = Mathf.Clamp(currentVelocity.y - movementProfile.gravity * Time.deltaTime, -maxFallSpeed, 5000);
        // Adds gravity and also clamps the max vertical speed

        float angle = Vector3.Angle(Vector3.up, currentNormal);

        if (controller.collisionFlags.HasFlag(CollisionFlags.Below) && angle > controller.slopeLimit)
        {
            Vector3 direction = currentNormal.normalized;//.Flattened().normalized;
            float factor = Mathf.InverseLerp(controller.slopeLimit, 90, angle);
            //factor = Mathf.Max(factor, 0.1f);
            currentVelocity += direction * edgePushMultiplier * factor;
        }

        controller.Move(currentVelocity * Time.deltaTime);
    }

    private void UpdateFOV()
    {
        float value = Moving && Sprinting ? NormalizedSpeed : 0f;
        float multiplier = 1f + value * 0.3f; // Will go between 1 and 1.3

        CameraFOV.Set(multiplier);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (controller.collisionFlags.HasFlag(CollisionFlags.Below))
            currentNormal = hit.normal;
        else
            currentNormal = Vector3.up;
    }

    private void UpdateCrouched()
    {
        //if (!Physics.SphereCast(new Ray(transform.position, Vector3.up), crouchRaycastSize, crouchRaycastLength + controller.height / 2f, crouchBlockLayermask))

        Vector3 pos = Vector3.up * crouchRaycastLength;
        pos.y -= controller.height / 2f;
        
        if (Physics.Raycast(transform.position, Vector3.up * (crouchRaycastLength - controller.height / 2f), crouchBlockLayermask))
        {

        }
        else if (Physics.CheckSphere(transform.position + pos, crouchRaycastSize, crouchBlockLayermask))
        {
            crouched = true;
        }
        else
        {
            crouched = WantsToCrouch;
        }

        float desiredControllerHeight = crouched ? crouchingControllerHeight : standingControllerHeight;

        //controller.height = Mathf.Lerp(controller.height, desiredControllerHeight, Time.deltaTime * crouchChangeSpeed);
        controller.height = Mathf.MoveTowards(controller.height, desiredControllerHeight, Time.deltaTime * crouchChangeSpeed);

        inspector_crouching = crouched;
    }

    private void UpdateGrounded()
    {
        grounded = Physics.CheckSphere(transform.position - new Vector3(0, groundedRaycastLength + controller.skinWidth + controller.height / 2f, 0), groundedRaycastSize, groundLayermask);
        applyDownforce = Physics.CheckSphere(transform.position - new Vector3(0, downforceCheckLength + controller.skinWidth + controller.height / 2f, 0), downforceCheckSize, groundLayermask);

        //grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), groundedRaycastSize, groundedRaycastLength + controller.skinWidth, groundLayermask);
        //applyDownforce = Physics.SphereCast(new Ray(transform.position, Vector3.down), downforceCheckSize, downforceCheckLength + controller.skinWidth, groundLayermask);
    }

    private void UpdateSpeed()
    {
        //float airMult = crouched ? movementProfile.crouchAirSpeedMultiplier : movementProfile.airSpeedMultiplier;
        //float airSpeedMult = grounded ? 1f : airMult;
        //WeaponProfile currentProfile = Weapons.GetProfile(player.currentWeapon);
        //
        //float baseStandingSpeed = Sprinting && Moving ? movementProfile.runningSpeed : movementProfile.walkingSpeed;
        //
        //bool canCrouchHop = !disableCrouchHopping && !grounded;
        //float baseSpeed = crouched && !canCrouchHop ? movementProfile.crouchSpeed : baseStandingSpeed;
        //
        //float adsMult = WeaponSway.IsInADS ? currentProfile.ADSSpeedMultiplier : 1f;
        //float desiredSpeed = baseSpeed * airSpeedMult * currentProfile.SpeedMultiplier * adsMult;
        //
        //float baseStandingJump = Sprinting && Moving ? movementProfile.runningJumpHeight : movementProfile.walkingJumpHeight;
        //float baseJump = crouched ? movementProfile.crouchJumpHeight : baseStandingJump;
        //float desiredJump = baseJump * currentProfile.JumpMultiplier;

        //if (grounded)
        //    currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * speedChangeSmoothing);
        //else
        //    currentSpeed = Mathf.Max(currentSpeed, Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * speedChangeSmoothing));
        //currentJumpHeight = Mathf.Lerp(currentJumpHeight, desiredJump, Time.deltaTime * speedChangeSmoothing);
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.down * (groundedRaycastLength + controller.skinWidth + controller.height / 2f), groundedRaycastSize);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + Vector3.down * (downforceCheckLength + controller.skinWidth + controller.height / 2f), downforceCheckSize);
        Gizmos.color = Color.blue;
        Vector3 pos = Vector3.up * crouchRaycastLength;
        pos.y -= controller.height / 2f;
        Gizmos.DrawSphere(transform.position + pos, crouchRaycastSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * (crouchRaycastLength - controller.height / 2f));
        //Gizmos.color = Color.black;
        //Gizmos.DrawSphere(transform.position, crouchRaycastSize / 2f);

        // Just draws the ground check rays so you can make sure they intersect the ground
    }
}
