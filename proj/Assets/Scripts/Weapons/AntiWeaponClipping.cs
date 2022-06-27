using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiWeaponClipping : MonoBehaviour
{
    public Transform eyes;
    public Transform barrel;

    [Space]
    public LayerMask envMask;

    [Space]
    public float speed = 5;
    public float amount = 0.01f;

    [Space]
    public Transform movingTransform;

    // Theory:
    /*
    
    Cast from eyes to barrel.
    -If nothing hit, barrel is not in wall. Perfect.

    else...
    -Do a backwards cast from the barrel. If hit, barrel is not in wall
    --Raycast from barrel to eyes and eyes to barrel to get 2 hits on obj.
    --Add normals together to get offset (offset mult by thickness)
    --Raycast from eye to predicted new pos. If still hitting obj, just retract gun


    */

    private void Update()
    {
        Check();
    }

    private void Check()
    {
        Vector3 offset = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        Vector3 dir = eyes.position.DirectionTo_NoNormalize(barrel.position);
        float dist = dir.magnitude;
        dir.Normalize();

        bool hitWall = Physics.Raycast(new Ray(eyes.position, dir), out RaycastHit eyeHit, dist, envMask);

        if (hitWall)
        {
            bool barrelInWall = !Physics.Raycast(new Ray(barrel.position, -dir), out RaycastHit barrelHit, dist, envMask);

            if (barrelInWall)
            {
                //Vector3 normals = eyeHit.normal + barrelHit.normal;
                //normals.Normalize();
                Vector3 normal = eyeHit.normal;
                // dist like .65 far and .5 close
                float factor = Remap.Float(eyeHit.distance, 0.45f, 0.7f, 2.5f, 0.2f);
                offset += transform.InverseTransformDirection(normal) * factor;
            }
            else
            {
                Vector3 normal = barrelHit.normal;
                offset += transform.InverseTransformDirection(normal);

                Vector3 pos = barrelHit.point + barrelHit.normal;
                Vector3 barrelPos = barrel.position + barrel.forward * 100;
                Vector3 newDir = barrelPos - pos;
                newDir.Normalize();
                rotation = Quaternion.LookRotation(transform.InverseTransformDirection(newDir));
            }
        }
        else
        {
            
        }

        movingTransform.localPosition = Vector3.Lerp(movingTransform.localPosition, offset * amount, Time.deltaTime * speed);
        movingTransform.localRotation = Quaternion.Slerp(movingTransform.localRotation, rotation, Time.deltaTime * speed);
    }

    private void WeaponPeek()
    {

    }


    private void OnDrawGizmos()
    {
        //if (barrel != null)
        //{
        //    Gizmos.color = Color.gray;
        //    Gizmos.DrawSphere(barrel.position, barrelInWallCheckSize);
        //}
    }
}
