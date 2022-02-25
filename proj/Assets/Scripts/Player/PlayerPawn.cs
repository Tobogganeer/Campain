using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualVoid.Net;

public class PlayerPawn : MonoBehaviour
{
    [Header("Both")]
    public Player player;
    public PlayerMovementDSM movement;
    public FPSCamera cam;
    public VoiceOutput voiceOutput;
    public PlayerAnimation animator;
    public ClientNetworkTransform netTransform;

    public bool IsLocalPlayer => player.IsLocalPlayer;

    public bool Crouching
    {
        get
        {
            if (movement != null) return movement.crouching;
            else return animator.crouching;
        }
    }

    private void Start()
    {
        netTransform.ownerClient = player;

        if (IsLocalPlayer)
        {

        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            
        }
    }
}
