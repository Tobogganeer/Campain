using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyEvents
{
    public static event Action<Vector3> OnEnemyDie;

    public static void EnemyDied(Vector3 position)
    {
        OnEnemyDie?.Invoke(position);
    }
}