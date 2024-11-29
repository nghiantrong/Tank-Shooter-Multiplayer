using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    private Allocation allocation;

    private string joinCode;

    private const int MAX_CONNECTIONS = 20;

    private const string GAME_SCENE_NAME = "Game";

    public async Task StartHostAsync()
    {
        try
        {
            //give allocation with maxConnections
            allocation = await Relay.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }

        try
        {
            //if succeed, get the code for it
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("JoinCode: " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //use dtls instead of udp for secure purposes
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        //only server needs to handle change scene because server changes scene for the client
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
    }
}
