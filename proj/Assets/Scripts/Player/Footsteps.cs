using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public static Footsteps instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform footSource;
    //public float stepSpeed = 2.5f;

    private Foot foot;

    private const float MIN_AIRTIME = 0.2f;

    public static event System.Action<Foot, float> OnFootstep;

    private void OnEnable()
    {
        PlayerMovement.OnLand += PlayerMovement_OnLand;
    }

    private void OnDisable()
    {
        PlayerMovement.OnLand -= PlayerMovement_OnLand;
    }

    private void Footstep(Foot foot, float magnitude)
    {
        OnFootstep?.Invoke(foot, magnitude);

        float vol = 0.65f;
        float range = 25f;

        if (PlayerMovement.Running)
        {
            vol = 1f;
            range = 45f;
        }
        else if (PlayerMovement.Crouched)
        {
            vol = 0.2f;
            range = 15f;
        }

        AudioManager.Play(GetSound(foot), footSource.position, null, range, AudioCategory.SFX, vol);
    }

    private void PlayerMovement_OnLand(float airtime)
    {
        if (airtime > MIN_AIRTIME) AudioManager.Play(AudioArray.Drop, footSource.position, null, 35, AudioCategory.SFX, Mathf.Clamp01(airtime * 0.6f));
    }

    private AudioArray GetSound(Foot foot)
    {
        if (PlayerMovement.Sliding) return AudioArray.Slide;

        return foot == Foot.Right ? AudioArray.RightFoot : AudioArray.LeftFoot;
    }


    //private void Update()
    //{
    //    UpdateFootsteps();
    //}

    //private void UpdateFootsteps()
    //{
    //    Vector3 actualHorizontalVelocity = PlayerMovement.LocalVelocity.Flattened();
    //
    //    float velocityMag = actualHorizontalVelocity.magnitude;
    //
    //    time += Time.deltaTime * stepSpeed * velocityMag;
    //
    //    float sinValue = Mathf.Sin(time);
    //
    //    CalculateFootstep(sinValue, velocityMag);
    //}

    public static void Calculate(float sinValue, float magnitude, ref float time)
        => instance.CalculateFootstep(sinValue, magnitude, ref time);

    private void CalculateFootstep(float sinValue, float magnitude, ref float time)
    {
        if (magnitude < 1f || !PlayerMovement.Grounded)// || PlayerMovement.Sliding)
        {
            time = 0;
            foot = Foot.Right;
        }

        if (sinValue > 0.5f && foot == Foot.Right && PlayerMovement.Grounded)
        {
            Footstep(Foot.Right, magnitude);
            foot = Foot.Left;
        }
        else if (sinValue < -0.5f && foot == Foot.Left && PlayerMovement.Grounded)
        {
            Footstep(Foot.Left, magnitude);
            foot = Foot.Right;
        }
    }
}
