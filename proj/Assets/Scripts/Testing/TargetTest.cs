using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTest : MonoBehaviour, IBulletDamagable
{
    public void TakeBulletDamage(DamageDetails details)
    {
        AudioManager.Play(new Audio("UIClick").Set2D());
    }
}
