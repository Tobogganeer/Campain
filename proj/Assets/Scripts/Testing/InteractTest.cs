using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTest : MonoBehaviour, IInteractable
{
    public float interactTime = 0;

    public void StartInteract(PlayerPawn player)
    {
        Debug.Log("Interact started");
    }

    public float GetInteractTime()
    {
        return interactTime;
    }

    public void FinishInteract(PlayerPawn player)
    {
        Debug.Log("Interact finished");
    }

    public void EndInteract(PlayerPawn player)
    {
        Debug.Log("Interact ended");
    }

    public bool CanInteract(PlayerPawn player)
    {
        return true;
    }
}
