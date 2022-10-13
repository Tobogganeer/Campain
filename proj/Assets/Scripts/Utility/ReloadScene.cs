using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ReloadScene : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            SceneManager.ReloadCurrentLevel();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
    }
}
