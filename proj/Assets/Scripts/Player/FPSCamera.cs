using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;

public class FPSCamera : MonoBehaviour
{
    public static FPSCamera instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform playerBody;
    public Transform verticalTransform;

    private float yRotation;

    //public float sensitivity = 3;
    public float maxVerticalRotation = 90;

    private const float SENSITIVITY_MULT = 0.1f;//3 / 50;
    //                           Default sens / Good cam sens

    private float sensitivity => CurrentSensFromSettings * SENSITIVITY_MULT;

    public static float CurrentSensFromSettings = 50;

    //[Space]
    //public float recoilDecaySpeed = 10;
    ////public float recoilSmoothSpeed = 5;
    //private static float currentRecoil;
    //private static float desiredRecoil;

    // ^^^ May be useful when getting hit by scourge


    int warningCounter = 0; // To not spam warnings in the console every frame

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //desiredRecoil = Mathf.Lerp(desiredRecoil, 0, Time.deltaTime * recoilDecaySpeed);
        //yRotation -= desiredRecoil * Time.deltaTime;

        //if (desiredRecoil < 0) desiredRecoil = 0;

        if (playerBody == null || verticalTransform == null)
        {
            warningCounter++;
            if (warningCounter > 60)
            {
                warningCounter = 0;
                Debug.LogWarning("FPSCamera transforms are null!");
            }
        }
        // Basic null checking

        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");

        playerBody.Rotate(Vector3.up * x * sensitivity);
        // Rotates the body horizontally

        yRotation = Mathf.Clamp(yRotation - y * sensitivity, -maxVerticalRotation, maxVerticalRotation);
        float clampedRotWithRecoil = yRotation;
        //float clampedRotWithRecoil = Mathf.Clamp(yRotation - currentRecoil, -maxVerticalRotation, maxVerticalRotation);

        // Clamps the Y rotation so you can only look straight up or down, not backwards
        verticalTransform.localRotation = Quaternion.Euler(new Vector3(clampedRotWithRecoil, 0));
        // Sets the verticalTransforms rotation
        // Cannot call the Rotate() method on verticalTransform because trying to clamp the y value
        // makes Euler roll in his grave and messes it up.
        // Storing the rotation, clamping it and then applying it fixed the problem.
    }

    public void LookAt_Local(Vector3 point)
    {
        Vector3 eulers = Quaternion.LookRotation(transform.position.DirectionTo(point), Vector3.up).eulerAngles;
        playerBody.eulerAngles = new Vector3(0, eulers.y, 0);
        yRotation = Mathf.Clamp(eulers.x, -maxVerticalRotation, maxVerticalRotation);
        verticalTransform.localRotation = Quaternion.Euler(new Vector3(yRotation, 0));
    }

    public static void LookAt(Vector3 point)
    {
        Vector3 eulers = Quaternion.LookRotation(instance.transform.position.DirectionTo(point), Vector3.up).eulerAngles;
        instance.playerBody.eulerAngles = new Vector3(0, eulers.y, 0);
        instance.yRotation = Mathf.Clamp(eulers.x, -instance.maxVerticalRotation, instance.maxVerticalRotation);
        instance.verticalTransform.localRotation = Quaternion.Euler(new Vector3(instance.yRotation, 0));
    }

    public static void Shake(ShakePreset preset)
    {
        Shaker.ShakeAllSeparate(preset);
    }

    //public static void AddRecoil(float amount)
    //{
    //    desiredRecoil += amount;
    //}
}
