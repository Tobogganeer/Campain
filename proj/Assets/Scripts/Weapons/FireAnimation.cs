using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    public Settings settings;

    //[Space]
    //public bool useSpring = true;

    [Space]
    public float speed = 18f;
    public float smoothing = 7f;

    [Space]
    public bool useOldLerp = true;

    [Space]
    public float dampTime = 0.03f;
    public float dampTimeAmount = 0.07f;
    public float dampTimeForward = 0.1f;
    public float forwardRotSpeed = 10;

    [Space]
    public float positionInfluence = 0.03f;

    [Space]
    public KeyCode debugApplyKey = KeyCode.H;

    Vector3 amount;
    Vector3 targetAmount;
    Vector3 forwardAmount;

    Vector3 rot;
    Vector3 targetRot;
    Vector3 forwardRot;

    Vector3 refCurrent;
    Vector3 refCurrentAmount;
    Vector3 refCurrentForward;

    //float time;

    private void Update()
    {
        if (Input.GetKeyDown(debugApplyKey))
            Apply(settings);

        #region OLD SPRING
        /*
        if (useSpring)
            targetAmount = Spring(targetAmount, Vector3.zero, Time.deltaTime);
        else if (useSmoothDamp)
            targetAmount = Vector3.SmoothDamp(targetAmount, Vector3.zero, ref refCurrent, dampTime);
        else
            targetAmount = Vector3.Lerp(targetAmount, Vector3.zero, Time.deltaTime * speed);

        if (useSmoothDamp)
            amount = Vector3.SmoothDamp(amount, targetAmount, ref refCurrentAmount, dampTimeAmount);
        else
            amount = Vector3.Lerp(amount, targetAmount, Time.deltaTime * smoothing);
        */
        #endregion

        if (useOldLerp)
        {
            targetAmount = Vector3.Lerp(targetAmount, Vector3.zero, Time.deltaTime * speed);
            forwardAmount = Vector3.Lerp(forwardAmount, Vector3.zero, Time.deltaTime * speed * 0.5f);
            amount = Vector3.Lerp(amount, targetAmount, Time.deltaTime * smoothing);
        }
        else
        {
            targetAmount = Vector3.SmoothDamp(targetAmount, Vector3.zero, ref refCurrent, dampTime);
            forwardAmount = Vector3.SmoothDamp(forwardAmount, Vector3.zero, ref refCurrentForward, dampTimeForward);
            //amount = Vector3.SmoothDamp(amount, targetAmount, ref refCurrentAmount, dampTimeAmount);
            Vector3 forward = forwardAmount;
            forward /= Mathf.Clamp(targetAmount.sqrMagnitude, 1, 10);
            amount = Vector3.SmoothDamp(amount, targetAmount + forward, ref refCurrentAmount, dampTimeAmount);
        }

        targetRot = Vector3.Lerp(targetRot, Vector3.zero, Time.deltaTime * speed);
        forwardRot = Vector3.Lerp(forwardRot, Vector3.zero, Time.deltaTime * forwardRotSpeed);

        Vector3 forRot = forwardRot;
        forRot /= Mathf.Clamp(targetRot.sqrMagnitude, 1, 10);
        rot = Vector3.Lerp(rot, targetRot + forRot, Time.deltaTime * smoothing);

        //transform.localPosition = (amount + forwardAmount) * positionInfluence;
        transform.localPosition = amount * positionInfluence;
        //Vector3 forward = forwardAmount * positionInfluence;
        //forward /= Mathf.Clamp(amount.sqrMagnitude, 1, 10);
        //transform.localPosition = amount * positionInfluence + forward;
        transform.localRotation = Quaternion.Euler(rot);

        //time += Time.deltaTime;
    }

    public void Apply(Settings settings)
    {
        targetAmount.z = Random.Range(-settings.posMin, -settings.posMax);
        forwardAmount.z = Random.Range(settings.posMin, settings.posMax) * settings.forwardMult;

        float rot = Random.Range(-settings.rotMin, -settings.rotMax);
        targetRot.x = rot;
        targetAmount.y = rot * settings.rotVertDipMult;
        targetRot.y = Random.Range(-settings.yRot, settings.yRot);
        targetRot.z = Random.Range(-settings.zRot, settings.zRot);

        //forwardRot.x = Random.Range(settings.rotMin * settings.forwardRot, settings.rotMax * settings.forwardRot) * settings.forwardRot;
        forwardRot.x = -targetRot.x * settings.forwardRot;
        forwardRot.y = -targetRot.y * settings.forwardRot;
        forwardRot.z = -targetRot.z * settings.forwardRot;
        //time = 0;
    }

    /*
    public static float Spring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }

    public static Vector3 Spring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
    }
    */

    [System.Serializable]
    public class Settings
    {
        [Space]
        public float posMin = 7f;
        public float posMax = 11f;

        [Space]
        public float forwardMult = 0.3f;
        public float forwardRot = 0.3f;

        [Space]
        public float rotMin = 0.3f;
        public float rotMax = 1.5f;
        public float rotVertDipMult = 1f;

        [Space]
        public float zRot = 1f;
        public float yRot = 2f;
    }
}
