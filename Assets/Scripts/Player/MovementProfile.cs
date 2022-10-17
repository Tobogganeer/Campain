using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Movement Profile")]
public class MovementProfile : ScriptableObject 
{
    [Min(0f)] public float walkingSpeed = 3.0f;
    [Min(0f)] public float runningSpeed = 4.5f;
    [Min(0f)] public float unarmedRunSpeed = 5.5f;
    [Min(0f)] public float airSpeedMultiplier = 1.2f;

    [Space]
    [Min(0f)] public float crouchSpeed = 1.4f;
    [Min(0f)] public float crouchChangeSpeed = 3.5f;
    [Min(0f)] public float crouchAirSpeedMultiplier = 1.2f;

    [Space]
    public float slopeLimit = 40;
    public float rampLimit = 15;
    public float slideFriction = 0.3f;
    public float slopeAccelPercent = 0.4f;

    [Space]
    public float gravity = 10f;

    [Space]
    [Min(0f)] public float walkingJumpHeight = 3.5f;
    [Min(0f)] public float runningJumpHeight = 4.0f;
    //[Min(0f)] public float crouchJumpHeight = 4.0f;

    [Space]
    [Min(0f)] public float groundAcceleration = 10;
    [Min(0f)] public float airAcceleration = 2;

    [Space]
    public float speedLerpSpeed = 5;
    public float jumpLerpSpeed = 5;
    public float accelLerpSpeed = 5;
}
