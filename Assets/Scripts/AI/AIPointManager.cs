using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIPointManager : MonoBehaviour
{
    public static AIPointManager instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform[] pointsOfInterest;
    public static Transform[] PointsOfInterest => instance.pointsOfInterest;

    private const int RANDOM_ATTEMPTS = 5;

    public bool drawGizmos = true;
    public float gizmoSize = 0.3f;

    public static Vector3 GetPointOfInterest(Vector3 currentPosition) // Maybe pass in min distance?
    {
        if (PointsOfInterest == null || PointsOfInterest.Length == 0) return currentPosition;

        // VVV Only try to find a far point 5 times, otherwise just go with any random point
        for (int i = 0; i < RANDOM_ATTEMPTS; i++)
        {
            // More than ~3 meters away
            Vector3 pos = PointsOfInterest[Random.Range(0, PointsOfInterest.Length)].position;
            if (pos.SqrDistance(currentPosition) > 10)
            {
                return pos;
            }
        }

        return PointsOfInterest[Random.Range(0, PointsOfInterest.Length)].position;
    }

    private void OnDrawGizmos()
    {
        if (pointsOfInterest == null || !drawGizmos) return;

        Gizmos.color = Color.yellow;
        foreach (Transform transform in pointsOfInterest)
        {
            Gizmos.DrawSphere(transform.position, gizmoSize);
        }
    }
}
