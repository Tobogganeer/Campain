using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimations : MonoBehaviour
{
    public Animator animator;
    
    public void SetVelocity(Vector3 velocity01)
    {
        animator.SetFloat("x", velocity01.x * 2 - 1, 0.1f, Time.deltaTime);
        animator.SetFloat("y", velocity01.z * 2 - 1, 0.1f, Time.deltaTime);
    }
}
