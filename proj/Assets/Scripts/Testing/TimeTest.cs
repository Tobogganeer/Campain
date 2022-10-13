using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeTest : MonoBehaviour
{
    public float slowmoTime = 0.2f;
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
            if (Time.timeScale < 0.5f) Time.timeScale = 1f;
            else Time.timeScale = slowmoTime;
    }
}
