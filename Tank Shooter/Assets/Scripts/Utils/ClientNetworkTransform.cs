using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CanCommitToTransform = IsOwner;
    }

    override protected void Update()
    {
        CanCommitToTransform = IsOwner;
        base.Update();
        //check if we connect to the server as a client
        if (!IsHost && NetworkManager != null && NetworkManager.IsConnectedClient && CanCommitToTransform)
        {
            //dirtyTime: how long it was since the last time we tried to sync the time
            TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
        }
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
