using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoints : MonoBehaviour
{
    [Header("Just used for inspector")]
    [SerializeField] private new string name;
    [SerializeField] private float maxHP = 100;
    [SerializeField] private float startingHP = 100;
    private float currentHP;

    public float CurrentHP => currentHP;

    private void Start()
    {
        currentHP = Mathf.Clamp(startingHP, 0, maxHP);
    }

    public void TakeDamage(float damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    public void SetHP(float amount)
    {
        currentHP = Mathf.Clamp(amount, 0, maxHP);
    }
}
