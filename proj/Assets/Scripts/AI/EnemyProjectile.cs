using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyProjectile : MonoBehaviour
{
    public abstract void Init(Vector3 targetPos, Vector3 origin);
}
