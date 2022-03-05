using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputProfile
{
    public KeyCode forward = KeyCode.W;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode backwards = KeyCode.S;

    public KeyCode sprint = KeyCode.LeftShift;
    public KeyCode jump = KeyCode.Space;
    public KeyCode crouch = KeyCode.C;

    public KeyCode interact = KeyCode.F;
    public KeyCode ads = KeyCode.Mouse1;
    public KeyCode fire = KeyCode.Mouse0;

    public KeyCode leanLeft = KeyCode.Q;
    public KeyCode leanRight = KeyCode.E;
}
