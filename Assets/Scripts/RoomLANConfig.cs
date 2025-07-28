using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class RoomLANConfig : NetworkBehaviour
{
    [Header("Host Port")]
    public ushort port = 7777;

    [Header("Spawn Positions")]
    public List<Vector3> spawnPositions = new List<Vector3>()
    {
        new Vector3(-5, 0, 0),
        new Vector3(5, 0, 0),
        new Vector3(0, 0, 5),
        new Vector3(0, 0, -5)
    };

    private string playerStat;

    private void Start()
    {
        playerStat = StaticEvents.playerStat;
        if (playerStat == "Host") StartHost();
        else StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    string GetLocalIPAddress()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            ?.ToString() ?? "Unavailable";
    }

    void ConfigureTransportForHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");
        }
    }

    void ConfigureTransportForClient(string serverIp)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(serverIp, port);
        }
    }

    public void StartHost()
    {
        ConfigureTransportForHost();
        bool success = NetworkManager.Singleton.StartHost();
        Debug.Log("StartHost" + port + " " + success);
        if (success)
        {
            MovePlayerToSpawnPosition(NetworkManager.Singleton.LocalClientId, 0);
        }
    }

    public void StartClient()
    {
        string ip = StaticEvents.hostIP;
        if (string.IsNullOrEmpty(ip))return;
        ConfigureTransportForClient(ip);
        bool success = NetworkManager.Singleton.StartClient();
        Debug.Log($"{ip}:{port}" + success);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int spawnIndex = NetworkManager.Singleton.ConnectedClients.Count - 1;
            if (spawnIndex < spawnPositions.Count)
            {
                MovePlayerToSpawnPosition(clientId, spawnIndex);
                MovePlayerClientRpc(clientId, spawnPositions[spawnIndex]);
            }
            else
            {
                Debug.LogWarning("Spawn index out of range. Not moving player.");
                SceneTransitionManager.Instance.LoadScene("LobbyScene");
            }
        }
    }

    [ClientRpc]
    private void MovePlayerClientRpc(ulong clientId, Vector3 position)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).transform.position = position;
        }
    }

    private void MovePlayerToSpawnPosition(ulong clientId, int spawnIndex)
    {
        if (spawnIndex >= 0 && spawnIndex < spawnPositions.Count)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject != null)
            {
                playerObject.transform.position = spawnPositions[spawnIndex];
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}