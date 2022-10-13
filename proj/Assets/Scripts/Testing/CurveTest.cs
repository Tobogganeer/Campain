using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTest : MonoBehaviour
{
    public float amplitude = 1;
    public float period = 1;
    public float timeScale = 1;

    [Space]
    public float movementScale = 1;

    Vector3 startPos;
    float time;

    void Start()
    {
        startPos = transform.position;
    }


    void Update()
    {
        time += Time.deltaTime * timeScale;

        transform.position = startPos + Functions.Lissajous(time, amplitude, period) * movementScale;
    }
}
