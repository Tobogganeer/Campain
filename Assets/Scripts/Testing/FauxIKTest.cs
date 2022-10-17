using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxIKTest : MonoBehaviour
{
    public Transform hip;
    public Transform knee;
    public Transform pole;
    public float legLength = 2f;

    void Update()
    {
        // Doesn't really work in play mode, just use in edit mode and watch the gizmos
        //knee.position = GetKneePos();
    }

    private void OnDrawGizmos()
    {
        Vector3 middle = Vector3.Lerp(transform.position, hip.position, 0.5f);
        Vector3 poleDir = middle.Flattened().DirectionTo(pole.position.Flattened());
        Vector3 footToHipDir = transform.position.DirectionTo(hip.position);

        Vector3 sidewaysVector = Vector3.Cross(footToHipDir, poleDir).normalized;
        Vector3 forwardsVector = Vector3.Cross(sidewaysVector, footToHipDir).normalized;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetKneePos(), 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(middle, middle + sidewaysVector);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(middle, middle + forwardsVector);
    }

    Vector3 GetKneePos()
    {
        float dist = Mathf.Clamp(Vector3.Distance(transform.position, hip.position), 0, legLength);

        Vector3 middle = Vector3.Lerp(transform.position, hip.position, 0.5f);
        Vector3 poleDir = middle.Flattened().DirectionTo(pole.position.Flattened());
        Vector3 footToHipDir = transform.position.DirectionTo(hip.position);

        Vector3 sidewaysVector = Vector3.Cross(footToHipDir, poleDir).normalized;
        Vector3 forwardsVector = Vector3.Cross(sidewaysVector, footToHipDir).normalized;

        float kneeDist = Pythagorean(dist / 2, legLength / 2);
        return middle + forwardsVector * kneeDist;
    }

    Vector3 GetKneePos_NoExtensionMethods()
    {
        float dist = Mathf.Clamp(Vector3.Distance(transform.position, hip.position), 0, legLength); // dont worry about this either
        // ALL YOU NEED IS TO REPLACE YOUR ONE DIRECTION WITH forwardsVector

        Vector3 middle = Vector3.Lerp(transform.position, hip.position, 0.5f); // Halfway up the leg, you have "(hip.position + transform.position) / 2" which is fine, same thing

        Vector3 middle_noY = middle;
        middle_noY.y = 0;
        Vector3 polePos_noY = pole.position;
        polePos_noY.y = 0;

        Vector3 poleDir = (polePos_noY - middle_noY).normalized; // Just the direction towards the pole from the knee, without any Y
        Vector3 footToHipDir = (hip.position - transform.position).normalized; // From the foot to the hip, pretty self explanatory

        Vector3 sidewaysVector = Vector3.Cross(footToHipDir, poleDir).normalized; // This will point from the middle to the right
        Vector3 forwardsVector = Vector3.Cross(sidewaysVector, footToHipDir).normalized; // This will point from the middle to the knee, but with the correct Y / elevation value

        float kneeDist = Pythagorean(dist / 2, legLength / 2); // not really correct, dont worry about this line, you only need the direction
        return middle + forwardsVector * kneeDist;
    }

    float Pythagorean(float a, float b)
    {
        return Mathf.Sqrt(a * a + b * b);
    }
}

/*

public static class VectorExtensions
{
    public static Vector3 Flattened(this Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

    public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
    {
		return Vector3.Normalize(target - origin);
    }
}

*/