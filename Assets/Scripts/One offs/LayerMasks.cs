using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMasks : MonoBehaviour
{
    private static LayerMasks instance;
    private void Awake() => instance = this;


    [SerializeField] private LayerMask bulletLayerMask;
    public static LayerMask BulletLayerMask => instance.bulletLayerMask;
}
