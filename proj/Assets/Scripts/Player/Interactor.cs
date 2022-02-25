using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public PlayerPawn player;

    public float interactRange = 4;
    public LayerMask interactLayers;
    public Transform interactFrom;
    public float interactCooldown = 1;
    float cooldown;

    private IInteractable currentInteractable;

    float interactTime;
    bool interacting;

    public bool Interacting => interacting;

    private void Update()
    {
        cooldown -= Time.deltaTime;

        FetchInteractables();

        if (Input.GetKeyDown(Inputs.Interact) && cooldown <= 0)
        {
            bool setCooldown = true;

            if (currentInteractable != null)
            {
                if (currentInteractable.CanInteract(player))
                {
                    currentInteractable.StartInteract(player);
                    float time = currentInteractable.GetInteractTime();

                    if (time > 0)
                    {
                        interacting = true;
                        interactTime = time;
                        HUD.SetInteractCooldown(interactTime);
                        setCooldown = false;
                    }
                }
            }

            if (setCooldown)
            {
                HUD.SetInteractCooldown(interactCooldown);
                cooldown = interactCooldown;
            }
            player.animator.UseArmAction();
        }

        if (Input.GetKey(Inputs.Interact) && interacting)
        {
            interactTime -= Time.deltaTime;

            if (interactTime <= 0)
            {
                interacting = false;
                interactTime = 0;
                currentInteractable?.FinishInteract(player);

                HUD.SetInteractCooldown(interactCooldown);
                cooldown = interactCooldown;
            }
        }

        if (Input.GetKeyUp(Inputs.Interact) && interacting)
        {
            if (interactTime > 0)
            {
                currentInteractable?.EndInteract(player);

                HUD.SetInteractCooldown(interactCooldown);
                cooldown = interactCooldown;
            }

            interacting = false;
            interactTime = 0;
        }
    }

    private void FetchInteractables()
    {
        if (Physics.Raycast(interactFrom.position, interactFrom.forward, out RaycastHit hit, interactRange, interactLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.transform.TryGetComponent(out currentInteractable))
                HUD.SetInteract(currentInteractable.CanInteract(player));
            else
            {
                HUD.SetInteract(false);
                if (interacting)
                {
                    currentInteractable?.EndInteract(player);
                    interactTime = 0;
                    HUD.SetInteractCooldown(interactCooldown);
                    cooldown = interactCooldown;
                }
                currentInteractable = null;
            }
        }
        else
        {
            HUD.SetInteract(false);

            if (interacting)
            {
                currentInteractable?.EndInteract(player);
                interactTime = 0;
                HUD.SetInteractCooldown(interactCooldown);
                cooldown = interactCooldown;
            }
            currentInteractable = null;
        }
    }
}
