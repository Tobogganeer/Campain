using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualVoid.Net;

public class PlayerPawn : MonoBehaviour
{
    [Header("Both")]
    public Player player;
    public FPSCamera cam;
    public VoiceOutput voiceOutput;
    public PlayerAnimation animator;
    public ClientNetworkTransform netTransform;

    public bool IsLocalPlayer => player.IsLocalPlayer;

    public bool Crouching => IsLocalPlayer ? PlayerMovement.Crouched : animator.crouching;

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
