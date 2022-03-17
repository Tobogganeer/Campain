using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccuracyProfile
{
    public float standingInnaccuracy;
    
    public float walkingInnaccuracy;
    public float runningInnaccuracy;

    public float adsMult;
    public float crouchingMult;
    public float airborneMult;
}
