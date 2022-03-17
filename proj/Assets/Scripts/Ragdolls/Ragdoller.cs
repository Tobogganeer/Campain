using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoller : MonoBehaviour
{
    public RagdollBone[] ragdollBones;
    public Behaviour[] componentsToDisableOnRagdoll;
    public Collider[] collidersToDisableOnRagdoll;

    public RagdollItem[] items;

    private void Start()
    {
        DisableRagdoll();
    }

    public void EnableRagdoll()
    {
        SetAllComps(false);
        SetAllBones(true);

        foreach (RagdollItem item in items)
        {
            item.Drop(transform.forward * 5);
        }
    }

    public void DisableRagdoll()
    {
        SetAllComps(true);
        SetAllBones(false);
    }

    private void SetAllComps(bool active)
    {
        foreach (Behaviour comp in componentsToDisableOnRagdoll)
        {
            comp.enabled = active;
        }

        foreach (Collider collider in collidersToDisableOnRagdoll)
        {
            collider.enabled = active;
        }
    }

    private void SetAllBones(bool active)
    {
        foreach (RagdollBone bone in ragdollBones)
        {
            bone.SetState(active);
        }
    }

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        foreach (RagdollBone bone in ragdollBones)
        {
            bone.AddForce(force, forceMode);
        }
    }
}