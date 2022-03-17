using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static LocalPlayer instance;
    private void Awake()
    {
        instance = this;
    }
}
