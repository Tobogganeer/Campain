using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseOnClick : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            Debug.Break();
    }
}
