using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakkaSword : MonoBehaviour
{
    public Transform sword;
    public Vector3 idleOffset;
    public Vector3 swingOffset;

    Vector3 swingAmount;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height);
    }
}
