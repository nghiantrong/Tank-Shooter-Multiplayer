using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    private Allocation allocation;

    private string joinCode;

    private string lobbyId;

    private NetworkServer networkServer;

    private const int MAX_CONNECTIONS = 20;

    private const string GAME_SCENE_NAME = "Game";

    public async Task StartHostAsync()
    {
        try
        {
            //create allocation with maxConnections
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

        //create lobby
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject (
                        //if they are members of the lobby, allowed to read
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MAX_CONNECTIONS, lobbyOptions);

            lobbyId = lobby.Id;

            //the ugs requires to send the ping after a certain amount of time so that it keeps enlisting the lobby
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "MissingName")
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        //only server needs to handle change scene because server changes scene for the client
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while(true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}
