using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using UnityEngine.InputSystem;

public class RoomLANConfig : NetworkBehaviour
{
    [Header("Host Port")]
    public ushort port = 7777;

    [Header("Spawn Positions")]
    public List<Transform> spawnPoints;

    [Header("Spawn Positions")]
    public GameObject InputOnlyPlayer;

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
        if (string.IsNullOrEmpty(ip)) return;
        ConfigureTransportForClient(ip);
        bool success = NetworkManager.Singleton.StartClient();
        Debug.Log($"{ip}:{port}" + success);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int spawnIndex = NetworkManager.Singleton.ConnectedClients.Count - 1;
            if (spawnIndex < spawnPoints.Count)
            {
                MovePlayerToSpawnPosition(clientId, spawnIndex);
                MovePlayerClientRpc(clientId, spawnIndex);
            }
            else
            {
                Debug.LogWarning("Spawn index out of range. Not moving player.");
                GameEvents.UIEvents.OnOnlineStart?.Invoke();
            }
        }
        Debug.Log("Owner ID = " + NetworkObject.OwnerClientId + ", IsOwner = " + IsOwner);
    }

    private void MovePlayerToSpawnPosition(ulong clientId, int spawnIndex)
    {
        if (spawnIndex >= 0 && spawnIndex < spawnPoints.Count)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject != null)
            {
                playerObject.transform.position = spawnPoints[spawnIndex].position;

                PlayerData playerData = new PlayerData();
                playerData.playerIndex = spawnIndex;
                playerData.playerName = IsServer ? StaticEvents.hostIP : StaticEvents.clientIP; // 统一由服务器决定
                playerData.playerColor = GameManager.Instance.playerColors[spawnIndex % GameManager.Instance.playerColors.Count];

                var input = playerObject.GetComponent<PlayerInput>();
                playerData.input = input;
                playerData.devices = new List<InputDevice>(input.devices);
                playerData.controlScheme = input.currentControlScheme;

                // 让服务器同步数据
                playerObject.GetComponent<PlayerTestSon>().SetPlayerData(playerData);
            }
        }
    }

    [ClientRpc]
    private void MovePlayerClientRpc(ulong clientId, int spawnIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject != null)
            {
                playerObject.transform.position = spawnPoints[spawnIndex].position;
                // 不再在客户端设置 PlayerData，而是等待服务器同步

                PlayerData playerData = new PlayerData();
                playerData.playerIndex = spawnIndex;
                playerData.playerName = IsServer ? StaticEvents.hostIP : StaticEvents.clientIP; // 统一由服务器决定
                playerData.playerColor = GameManager.Instance.playerColors[spawnIndex % GameManager.Instance.playerColors.Count];

                var input = playerObject.GetComponent<PlayerInput>();
                playerData.input = input;
                playerData.devices = new List<InputDevice>(input.devices);
                playerData.controlScheme = input.currentControlScheme;

                // 让服务器同步数据
                playerObject.GetComponent<PlayerTestSon>().SetPlayerData(playerData);
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