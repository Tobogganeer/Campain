using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public abstract class PlayerAnimation : MonoBehaviour
public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public Transform lookAtBone;
    public PlayerPawn player;

    public int updatesPerSecond = 10;
    float updateTime;
    float timer;

    public bool crouching; // for other players
    public float x;
    public float y;

    PlayerAnimationMessage lastData;

    private void Start()
    {
        //player = GetComponent<PlayerPawn>();
        updateTime = 1f / updatesPerSecond;
        timer = updateTime;

        animator.SetLayerWeight(1, 0);
    }

    //public void Set(float x, float y, bool grounded, bool crouching, Vector3 camForward)
    private void Update()
    {
        if (player.player.IsLocalPlayer)
        {
            Set();

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = updateTime;
                PlayerAnimationMessage data = GetMessage();

                if (data != lastData)
                {
                    //ClientSend.SendPlayerAnimation(data);
                    lastData = data;
                }
            }
        }
    }

    public void Set()
    {
        // Called only for local client
        float x = GetFactor(player.movement.LocalActualVelocity.x);
        float y = GetFactor(player.movement.LocalActualVelocity.z);

        animator.SetBool("grounded", player.movement.grounded);
        animator.SetBool("crouching", player.movement.crouching);
        animator.SetFloat("x", x, 0.1f, Time.deltaTime);
        animator.SetFloat("y", y, 0.1f, Time.deltaTime);
    }

    private float GetFactor(float xy)
    {
        float sign = xy < 0 ? -1 : 1;
        xy = Mathf.Abs(xy);

        float factor;
        float walkSpeed = player.movement.GetWalkingSpeed();

        if (xy < walkSpeed) factor = Mathf.Lerp(0, 0.5f, xy / walkSpeed);
        else factor = Mathf.Lerp(0.5f, 1f, xy / player.movement.GetRunningSpeed());

        return factor * sign;
    }

    private PlayerAnimationMessage GetMessage()
    {
        float x = GetFactor(player.movement.LocalActualVelocity.x);
        float y = GetFactor(player.movement.LocalActualVelocity.z);

        return new PlayerAnimationMessage(player.movement.crouching, player.movement.grounded, x, y, player.cam.transform.forward);
    }


    public void OnData(PlayerAnimationMessage data)
    {
        crouching = data.crouching;
        x = data.x;
        y = data.y;
        animator.SetBool("grounded", data.grounded);
        animator.SetBool("crouching", data.crouching);
        animator.SetFloat("x", data.x, 0.1f, updateTime);
        animator.SetFloat("y", data.y, 0.1f, updateTime);
        LookAt(data.lookDirection);
    }

    //public abstract void LookAt(Vector3 camForward);

    private Quaternion lastRot;
    private Vector3 forward;
    public void LookAt(Vector3 camForward)
    {
        forward = camForward;
    }

    private void LateUpdate()
    {
        if (!player.player.IsLocalPlayer)
        {
            //Quaternion rot = lookAtBone.rotation;
            //lookAtBone.LookAt(lookAtBone.position + forward);
            lookAtBone.forward = forward;
            Quaternion target = lookAtBone.rotation;
            //lookAtBone.rotation = rot;

            lookAtBone.rotation = Quaternion.Slerp(lastRot, target, Time.deltaTime * 5);
            //lookAtBone.rotation = target;

            lastRot = lookAtBone.rotation;
        }
    }



    public void UseArmAction()
    {
        StopAllCoroutines();

        StartCoroutine(ArmAction());
    }


    const float AstronautInteractTime = 0.66f;
    const float ScourgeInteractTime = 1.75f;
    // ^^^ Based on animation times

    readonly WaitForSeconds astroWait = new WaitForSeconds(AstronautInteractTime);
    readonly WaitForSeconds scourgeWait = new WaitForSeconds(ScourgeInteractTime);
    const int LayerFadeSteps = 20;
    const float LayerFadeTime = 0.5f;

    readonly WaitForSeconds fadeWait = new WaitForSeconds(LayerFadeTime / LayerFadeSteps);

    private IEnumerator ArmAction()
    {
        //if (player.IsLocalPlayer)
        //    ClientSend.SendPlayerArmAction();
        //
        //if (player.IsScourge)
        //{
        //    animator.SetLayerWeight(1, 1);
        //    animator.CrossFadeInFixedTime("Attack", 0.1f, 1);
        //
        //    yield return scourgeWait;
        //
        //    //animator.SetLayerWeight(1, 0);
        //}
        //else
        //{
        //    animator.SetLayerWeight(1, 1);
        //    animator.CrossFadeInFixedTime("Interact", 0.1f, 1);
        //
        //    yield return astroWait;
        //
        //    //animator.SetLayerWeight(1, 0);
        //}

        for (int i = LayerFadeSteps; i > 1; i--)
        {
            float percent = Remap.Float(i, 1, LayerFadeSteps, 0, 1);
            animator.SetLayerWeight(1, percent);

            yield return fadeWait;
        }
    }
}
