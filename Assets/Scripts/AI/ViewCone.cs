using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewCone : MonoBehaviour
{
    public float radius = 5;
    public float fov = 75;

    //public LayerMask visionBlockingLayers;
    public Transform visionSource;

    public LayerMask testedLayers;
    public LayerMask targetLayers;

    public bool CanSee(Vector3 point)
    {
        Vector3 dir = visionSource.position.DirectionTo(point);

        #region Old
        //Debug.Log(Vector3.Angle(dir, visionSource.forward));

        //Debug.Log(Vector3.Dot(visionSource.forward, dir));
        //
        //float dot = Vector3.Dot(visionSource.forward, dir);
        //if (dot < Mathf.Cos(fov)) return false;
        #endregion

        if (Vector3.Angle(dir, visionSource.forward) > fov / 2) return false;
        // Point is out of field of view

        Ray ray = new Ray(visionSource.position, dir);
        //Ray fromPoint = new Ray(point, -dir);

        //if (Physics.Raycast(fromThis, radius, visionBlockingLayers)) return false; // Raycast from this hit a vision blocking object
        //if (Physics.Raycast(fromPoint, radius, visionBlockingLayers)) return false; // Raycast from point hit a vision blocking object

        if (Physics.Raycast(ray, out RaycastHit hit, radius, testedLayers))
        {
            int layer = hit.collider.gameObject.layer; // GameObject layer is stored by index rather than binary
            //LayerMask hitMask = LayerMask.GetMask(LayerMask.LayerToName(layer));
            layer = (int)Mathf.Pow(2, layer); // Convert index to binary by using 2 ^ layer

            if ((targetLayers.value & layer) != 0) // Check if targetLayers contains layer at all
            {
                return true; // If the layer hit is a target layer, return true
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (visionSource == null) return;

        float halfFov = fov / 2f;

        int numPointsToSample = 8; // For making the circle thing
        Vector3[] points = new Vector3[numPointsToSample + 1];

        //Quaternion right = Quaternion.AngleAxis(halfFov, Vector3.up);
        //Quaternion left = Quaternion.AngleAxis(-halfFov, Vector3.up);
        //// Gets the directions going at the limits of the fov
        //
        //points[points.Length - 1] = right * transform.forward;
        //points[0] = left * transform.forward;
        // Transforms the quaternions into vector directions

        float factor = 1f / numPointsToSample;

        for (int i = 0; i < numPointsToSample + 1; i++)
        {
            Quaternion dir = Quaternion.AngleAxis(Mathf.Lerp(-halfFov, halfFov, factor * i), Vector3.up);
            points[i] = dir * transform.forward;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(visionSource.position, visionSource.position + points[0] * radius);
        Gizmos.DrawLine(visionSource.position, visionSource.position + points[points.Length - 1] * radius);

        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(visionSource.position + points[i] * radius, visionSource.position + points[i + 1] * radius);
        }
        //Gizmos.DrawLine(visionSource.position + rightVec * radius, visionSource.position + leftVec * radius);

        // Draws the triangle
    }
}
