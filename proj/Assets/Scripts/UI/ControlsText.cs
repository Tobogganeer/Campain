using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsText : MonoBehaviour
{
    public TMPro.TMP_Text text;

    private void Update()
    {
        if (Keyboard.current.tabKey.isPressed)
        {
            text.text = $@"
WASD => Move
Shift => Sprint
C => Crouch
Q & E => Lean
LMB and RMB => Shoot and ADS
Up, Right, Down arrows => Barrel, Underbarrel, Sight
R => Reload
T => Slow Time
Backspace => Reload Level
Escape => Quit
K (Hold) => Direct bullet volley
L (Press) => Random bullet volley
Middle Mouse (Press) => Change Weapons
";
        }
        // WASD Shift C Q E LMB RMB Up Down R  . T
        else
        {
            text.text = "Hold TAB for controls";
        }
    }
}
