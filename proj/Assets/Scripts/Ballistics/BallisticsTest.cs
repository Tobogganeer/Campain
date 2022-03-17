using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallisticsTest : MonoBehaviour
{
    public BallisticsSettings settings;
    public LayerMask layerMask;
    public float maxDistance;

    private BallisticsResult currentResult;

    [ContextMenu("Cast Ray")]
    public void CastRay()
    {
        currentResult = Ballistics.Cast(transform.position, transform.forward, maxDistance, layerMask, settings);

        for (int i = 0; i < currentResult.penetrations.Length; i++)
        {
            Debug.Log(currentResult.penetrations[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentResult.empty) return;

        Color start = Color.green;
        Color air = Color.yellow;
        Color material = Color.red;

        if (currentResult.penetrations != null && currentResult.penetrations.Length > 0)
        {
            Gizmos.color = start;
            Gizmos.DrawLine(transform.position, currentResult.penetrations[0].entryPoint);

            for (int i = 0; i < currentResult.penetrations.Length; i++)
            {
                if (currentResult.penetrations[i].exited)
                {
                    Gizmos.color = material;
                    Gizmos.DrawLine(currentResult.penetrations[i].entryPoint, currentResult.penetrations[i].exitPoint);
                }

                if (i > 0)
                {
                    Gizmos.color = air;
                    Gizmos.DrawLine(currentResult.penetrations[i - 1].exitPoint, currentResult.penetrations[i].entryPoint);
                }
            }
        }
    }
}
