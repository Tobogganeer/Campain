using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigIK : MonoBehaviour
{
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public float maxDistance = 0.35f;
    public float upwardsAngleLimit = 5;
    public Vector3 footOffset;
    public Vector3 rotOffsetHit = new Vector3(-140, 0, 0);
    public Vector3 rotOffsetDefault = new Vector3(-230, 0, 0);
    public LayerMask layerMask;

    [Space]
    public Transform followHeight;
    public float heightOffset;
    public float followAmount = 0.6f;

    void Update()
    {
        if (followHeight != null)
        {
            transform.localPosition = transform.localPosition.WithY(followHeight.localPosition.y * followAmount + heightOffset);
        }

        //Vector3 upOffset = footOffset + Vector3.up * maxDistance;
        Vector3 upOffset = Vector3.up * maxDistance;
        if (Physics.Raycast(new Ray(leftFoot.position + upOffset, Vector3.down), out RaycastHit hit, maxDistance, layerMask))
        {
            leftFootTarget.position = hit.point + footOffset;
            if (Vector3.Angle(hit.normal, Vector3.up) > upwardsAngleLimit)
            {
                leftFootTarget.rotation = Quaternion.LookRotation(hit.normal);
                leftFootTarget.Rotate(rotOffsetHit, Space.Self);
            }
            else
            {
                leftFootTarget.rotation = leftFoot.rotation;
                leftFootTarget.Rotate(rotOffsetDefault, Space.Self);
            }
        }
        else
        {
            leftFootTarget.position = leftFoot.position + footOffset;
            leftFootTarget.rotation = leftFoot.rotation;
            leftFootTarget.Rotate(rotOffsetDefault, Space.Self);
        }

        if (Physics.Raycast(new Ray(rightFoot.position + upOffset, Vector3.down), out hit, maxDistance, layerMask))
        {
            rightFootTarget.position = hit.point + footOffset;
            if (Vector3.Angle(hit.normal, Vector3.up) > upwardsAngleLimit)
            {
                rightFootTarget.rotation = Quaternion.LookRotation(hit.normal);
                rightFootTarget.Rotate(rotOffsetHit, Space.Self);
            }
            else
            {
                rightFootTarget.rotation = rightFoot.rotation;
                rightFootTarget.Rotate(rotOffsetDefault, Space.Self);
            }
        }
        else
        {
            rightFootTarget.position = rightFoot.position + footOffset;
            rightFootTarget.rotation = rightFoot.rotation;
            rightFootTarget.Rotate(rotOffsetDefault, Space.Self);
        }

        //leftFootTarget.Rotate(rotOffset, Space.Self);
        //rightFootTarget.Rotate(rotOffset, Space.Self);

        //Vector3 left = leftFootTarget.localEulerAngles;
        //left.z = Mathf.Clamp(left.z, -35f, 35f);
        //leftFootTarget.localRotation = Quaternion.Euler(left);
    }

    private void OnDrawGizmos()
    {
        if (leftFoot != null && rightFoot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(leftFoot.position, 0.1f);
            Gizmos.DrawWireSphere(rightFoot.position, 0.1f);
            Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.up * maxDistance);
            Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.up * maxDistance);

            //Vector3 upOffset = footOffset + Vector3.up * maxDistance;
            Vector3 upOffset = Vector3.up * maxDistance;
            Gizmos.color = Color.red;

            if (Physics.Raycast(new Ray(leftFoot.position + upOffset, Vector3.down), out RaycastHit hit, maxDistance, layerMask))
                Gizmos.DrawWireSphere(hit.point + footOffset, 0.2f);
            //leftFootTarget.position = hit.point + footOffset;
            else
                Gizmos.DrawWireSphere(leftFoot.position + footOffset, 0.2f);
            //leftFootTarget.position = leftFoot.position + footOffset;

            if (Physics.Raycast(new Ray(rightFoot.position + upOffset, Vector3.down), out hit, maxDistance, layerMask))
                Gizmos.DrawWireSphere(hit.point + footOffset, 0.2f);
            else
                Gizmos.DrawWireSphere(rightFoot.position + footOffset, 0.2f);

            //Gizmos.DrawWireSphere(leftFoot.position + upOffset, 0.15f);
            //Gizmos.DrawLine(leftFoot.position + upOffset, leftFoot.position + upOffset + Vector3.down * maxDistance);
        }
    }
}
