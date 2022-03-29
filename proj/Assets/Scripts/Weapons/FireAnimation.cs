using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    public Settings settings;

    [Space]
    public float speed = 18f;
    public float smoothing = 7f;

    public float positionInfluence = 0.03f;

    [Space]
    public KeyCode debugApplyKey = KeyCode.H;

    Vector3 amount;
    Vector3 targetAmount;

    Vector3 rot;
    Vector3 targetRot;

    private void Update()
    {
        if (Input.GetKeyDown(debugApplyKey))
            Apply(settings);

        targetAmount = Vector3.Lerp(targetAmount, Vector3.zero, Time.deltaTime * speed);
        amount = Vector3.Lerp(amount, targetAmount, Time.deltaTime * smoothing);

        targetRot = Vector3.Lerp(targetRot, Vector3.zero, Time.deltaTime * speed);
        rot = Vector3.Lerp(rot, targetRot, Time.deltaTime * smoothing);

        transform.localPosition = amount * positionInfluence;
        transform.localRotation = Quaternion.Euler(rot);
    }

    public void Apply(Settings settings)
    {
        targetAmount.z = Random.Range(-settings.posMin, -settings.posMax);
        float rot = Random.Range(-settings.rotMin, -settings.rotMax);
        targetRot.x = rot;
        targetAmount.y = rot * settings.rotVertDipMult;
        targetRot.y = Random.Range(-settings.yRot, settings.yRot);
        targetRot.z = Random.Range(-settings.zRot, settings.zRot);
    }

    [System.Serializable]
    public class Settings
    {
        [Space]
        public float posMin = 7f;
        public float posMax = 11f;

        [Space]
        public float rotMin = 0.3f;
        public float rotMax = 1.5f;
        public float rotVertDipMult = 1f;

        [Space]
        public float zRot = 1f;
        public float yRot = 2f;
    }
}
