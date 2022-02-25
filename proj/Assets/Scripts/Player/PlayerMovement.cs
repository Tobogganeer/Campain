using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float slopeLimit = 45;
    public float slideFriction = 0.3f;
    public float gravity = 10f;
    public float acceleration = 12f;

    [Space]
    public Vector3 groundNormal;
    public bool grounded;
    private bool wasGrounded;

    //public bool onSlope;
    public float timeOnSlope;
    public bool groundNear;

    public float y;

    const float DOWNFORCE = 3f;
    //float slopeMult;

    Vector3 desiredVelocity;
    Vector3 moveVelocity;
    Vector3 actualVelocity;
    Vector3 lastPos;
    float slopeTime;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.slopeLimit = 80;
        lastPos = transform.position;
        slopeTime = 1;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundNormal = hit.normal;
    }


    private void Update()
    {
        Move();

        actualVelocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        desiredVelocity = transform.right * input.x + transform.forward * input.y;
        desiredVelocity *= speed;

        y -= gravity * Time.deltaTime;

        if (grounded) y = -DOWNFORCE;

        if (timeOnSlope > 0.2f && groundNear)
            desiredVelocity = Vector3.zero;

        if (timeOnSlope > 0.2f)
        {
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

        if (wasGrounded && !grounded)
            y += DOWNFORCE;

        //moveDir.y = y;

        Vector3 flatVel = actualVelocity.Flattened();
        //if (actualVelocity.y > 0)
            //y = actualVelocity.y;
            //this.moveVelocity.y = actualVelocity.y;

        this.moveVelocity = Vector3.Lerp(flatVel, desiredVelocity, Time.deltaTime * acceleration).WithY(y);

        controller.Move(this.moveVelocity * Time.deltaTime);

        if (Vector3.Angle(Vector3.up, groundNormal) > slopeLimit)
            timeOnSlope += Time.deltaTime;
        else
            timeOnSlope = 0f;
        //grounded = !onSlope;

        wasGrounded = grounded;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), 0.475f, 1.2f);

        groundNear = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.8f);

        if (!Physics.CheckSphere(transform.position + Vector3.down * 1.2f, 0.55f))
            groundNormal = Vector3.down;
    }
}
