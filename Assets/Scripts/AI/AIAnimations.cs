using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimations : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    public HumanoidAnimations animations;

    private void Update()
    {
        Vector3 localSpeed = agent.transform.InverseTransformVector(agent.velocity);
        //Vector3 localSpeed = agent.velocity;
        if (agent.velocity.sqrMagnitude < 0.1f)
            localSpeed = Vector3.zero;

        //float xSpeed = Mathf.InverseLerp(-agent.speed, agent.speed, localSpeed.x);
        //float ySpeed = Mathf.InverseLerp(-agent.speed, agent.speed, localSpeed.z);
        //xSpeed = xSpeed * 2 - 1;
        //ySpeed = ySpeed * 2 - 1;
        //
        //animator.SetFloat("x", xSpeed);
        //animator.SetFloat("y", ySpeed);
        animations.SetVelocity(localSpeed.InverseLerp(-agent.speed, agent.speed));

        //Debug.Log($"Agent Speed: {agent.speed} - x: {xSpeed} - y: {ySpeed}");
    }

    //private void OnDrawGizmosSelected()
    //{
    //    if (agent == null) return;
    //
    //    Vector3 localSpeed = agent.transform.InverseTransformVector(agent.velocity);
    //    if (agent.velocity.sqrMagnitude < 0.1f)
    //        localSpeed = Vector3.zero;
    //
    //    Vector3 worldSpeed = agent.velocity;
    //    if (agent.velocity.sqrMagnitude < 0.1f)
    //        worldSpeed = Vector3.zero;
    //
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.position, transform.position + localSpeed);
    //
    //    Gizmos.color = new Color(0, 1, 1);
    //    Gizmos.DrawLine(transform.position, transform.position + worldSpeed);
    //}
}
