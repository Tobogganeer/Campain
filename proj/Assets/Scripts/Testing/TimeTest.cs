using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTest : MonoBehaviour
{
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            if (Time.timeScale < 0.5f) Time.timeScale = 1f;
            else Time.timeScale = 0.35f;
    }
}
