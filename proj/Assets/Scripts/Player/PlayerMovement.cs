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

    public float speed = 5;
    public float jumpHeight = 4f;
    public float slopeLimit = 45;
    public float slideFriction = 0.3f;
    public float gravity = 10f;
    public float acceleration = 7f;
    public float airAcceleration = 1f;

    [Space]
    public Vector3 groundNormal;
    public bool grounded;
    private bool wasGrounded;

    public bool groundNear;

    public float y;

    private float timeOnSlope;
    private float timeSinceLastJump;
    private float jumpReduction;

    const float DOWNFORCE = 3f;
    //float slopeMult;

    Vector3 desiredVelocity;
    Vector3 moveVelocity;
    Vector3 actualVelocity;
    Vector3 lastPos;
    float slopeTime;

    private const float JUMP_FALLOFF = 2.2f;
    private const float JUMP_MAX_FALLOFF = 3.25f;
    private const float JUMP_CHARGE = 0.4f;
    private const float JUMP_DIP = 1.2f;
    private float jumpCharge;

    private float cur_speed;
    private float cur_accel;
    private float cur_jumpHeight;

    private float airtime;

    public static event Action<float> OnLand;

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
        IncrementValues(Time.deltaTime);

        UpdateSpeed();
        UpdateAcceleration();
        UpdateJumpHeight();

        Move();

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
    }

    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        desiredVelocity = transform.right * input.x + transform.forward * input.y;
        desiredVelocity *= cur_speed;

        y -= gravity * Time.deltaTime;

        if (grounded && timeSinceLastJump > 0.4f) y = -DOWNFORCE;

        if (grounded)
        {
            if (!wasGrounded)
            {
                // Just landed
                OnLand?.Invoke(airtime);
                FPSCamera.VerticalDip += Mathf.Lerp(0.0f, 2f, airtime * 0.4f);
                jumpReduction = Mathf.Max(jumpReduction, Mathf.Max(airtime, JUMP_MAX_FALLOFF * 1.5f));
                // Reduce jump upon landing
                airtime = 0;
            }
        }
        else
            airtime += Time.deltaTime;

        if (timeOnSlope > 0.2f && groundNear)
            desiredVelocity = Vector3.zero;
        // On a slope, but not above a void or something (will fall onto ground)

        if (timeOnSlope > 0.2f && timeSinceLastJump > 0.6f)
        {
            // On slope and hasnt like just jumped

            //moveDir = Vector3.zero;
            if (groundNear)
            {
                slopeTime += Time.deltaTime * 5;
                y = -DOWNFORCE * slopeTime;
            }
            else
                slopeTime = 1f;

            //moveVelocity.x += /*(1f - groundNormal.y) **/ groundNormal.x /* slopeMult */ * (1f - slideFriction);
            //moveVelocity.z += /*(1f - groundNormal.y) **/ groundNormal.z /* slopeMult */ * (1f - slideFriction);
            desiredVelocity.x += groundNormal.x * slopeTime * (1f - slideFriction);
            desiredVelocity.z += groundNormal.z * slopeTime * (1f - slideFriction);
            //actualVelocity.x += groundNormal.x * slopeTime * (1f - slideFriction);
            //actualVelocity.z += groundNormal.z * slopeTime * (1f - slideFriction);
        }
        else
        {
            slopeTime = 1f;
        }

        if (wasGrounded && !grounded && timeSinceLastJump > 0.4f)
        {
            // Left ground
            y += DOWNFORCE; // counteract downforce, set y to 0
            jumpCharge = 0; // cancel any pending jump
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space) && timeSinceLastJump > 0.4f && timeOnSlope < 0.2f && jumpCharge <= 0)
        {
            jumpCharge = JUMP_CHARGE;
            float dip = JUMP_DIP;
            dip = Mathf.Clamp(dip - jumpReduction * 0.2f, JUMP_DIP - 0.75f, JUMP_DIP);
            FPSCamera.VerticalDip += dip;
        }

        if (jumpCharge > 0)
        {
            jumpCharge -= Time.deltaTime;

            if (jumpCharge <= 0)
            {
                if (grounded && timeSinceLastJump > 0.4f && timeOnSlope < 0.2f)
                {
                    jumpReduction = Mathf.Max(1, jumpReduction);
                    float calibratedHeight = cur_jumpHeight / Mathf.Min(jumpReduction, JUMP_FALLOFF);
                    jumpReduction *= JUMP_FALLOFF;
                    if (jumpReduction > JUMP_MAX_FALLOFF) jumpReduction = JUMP_MAX_FALLOFF;
                    y += DOWNFORCE + calibratedHeight;
                    timeSinceLastJump = 0;
                }

                jumpCharge = 0;
            }
        }

        //moveDir.y = y;

        Vector3 flatVel = actualVelocity.Flattened();
        //if (actualVelocity.y > 0)
            //y = actualVelocity.y;
            //this.moveVelocity.y = actualVelocity.y;

        this.moveVelocity = Vector3.Lerp(flatVel, desiredVelocity, Time.deltaTime * cur_accel).WithY(y);

        controller.Move(this.moveVelocity * Time.deltaTime);

        if (Vector3.Angle(Vector3.up, groundNormal) > slopeLimit)
            timeOnSlope += Time.deltaTime;
        else
            timeOnSlope = 0f;
        //grounded = !onSlope;

        wasGrounded = grounded;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), 0.475f, 0.7f);

        groundNear = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.8f);

        if (!Physics.CheckSphere(transform.position + Vector3.down * 0.8f, 0.55f))
            groundNormal = Vector3.up;
    }

    public void AddImpulse(Vector3 force)
    {
        lastPos -= force;
        y += force.y * 10;
    }


    private void UpdateSpeed()
    {
        cur_speed = speed;
    }

    private void UpdateAcceleration()
    {
        cur_accel = grounded ? acceleration : airAcceleration;
    }

    private void UpdateJumpHeight()
    {
        cur_jumpHeight = jumpHeight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.down * 0.7f, 0.475f);
    }
}
