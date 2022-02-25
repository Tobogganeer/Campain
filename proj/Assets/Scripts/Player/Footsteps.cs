using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public Transform footSource;
    public float stepSpeed = 2.5f;

    public PlayerMovementDSM playerMovement;
    private Foot foot;
    private float time;

    private const float MIN_AIRTIME = 0.3f;

    public float walkingRange;
    public float runningMult;
    public float crouchingMult;

    private void OnEnable()
    {
        PlayerMovementDSM.OnLand += PlayerMovement_OnLand;
    }

    private void OnDisable()
    {
        PlayerMovementDSM.OnLand -= PlayerMovement_OnLand;
    }

    private void OnFootstep(Foot foot, float magnitude)
    {
        float vol;
        float range = walkingRange;
        bool running = playerMovement.running;
        if (playerMovement.crouching)
        {
            range *= crouchingMult;

            if (!running)
                vol = 0;
            else
                vol = 0.4f;
        }
        else
        {
            if (!running)
                vol = 0.75f;
            else
                vol = 1f;
        }

        if (running)
            range *= runningMult;

        AudioManager.Play(GetSound(foot), footSource.position, null, range, AudioCategory.SFX, vol);
    }

    private void PlayerMovement_OnLand(float airtime)
    {
        if (airtime > MIN_AIRTIME) AudioManager.Play(AudioArray.Drop, footSource.position, null, 35, AudioCategory.SFX, 1f);
    }

    private AudioArray GetSound(Foot foot)
    {
        switch (foot)
        {
            case Foot.Left:
                return AudioArray.LeftFoot;
            case Foot.Right:
                return AudioArray.RightFoot;
        }

        return AudioArray.Null;
    }


    private void Update()
    {
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        Vector3 actualHorizontalVelocity = playerMovement.LocalActualVelocity.Flattened();

        float velocityMag = actualHorizontalVelocity.magnitude;

        time += Time.deltaTime * stepSpeed * velocityMag;

        float sinValue = Mathf.Sin(time);

        CalculateFootstep(sinValue, velocityMag);
    }

    private void CalculateFootstep(float sinValue, float magnitude)
    {
        if (magnitude < 1f || !playerMovement.grounded)
        {
            time = 0;
            foot = Foot.Right;
        }

        if (sinValue > 0.5f && foot == Foot.Right && playerMovement.grounded)
        {
            OnFootstep(Foot.Right, magnitude);
            foot = Foot.Left;
        }
        else if (sinValue < -0.5f && foot == Foot.Left && playerMovement.grounded)
        {
            OnFootstep(Foot.Left, magnitude);
            foot = Foot.Right;
        }
    }
}
