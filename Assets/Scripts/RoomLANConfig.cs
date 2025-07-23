using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class RoomLANConfig : NetworkBehaviour
{
    [Header("Port")]
    public ushort port = 7777;

    [Header("Spawn Positions")]
    public List<Vector3> spawnPositions = new List<Vector3>()
    {
        new Vector3(-5, 0, 0),  // λ��1
        new Vector3(5, 0, 0),   // λ��2
        new Vector3(0, 0, 5),   // λ��3
        new Vector3(0, 0, -5)   // λ��4
    };

    private string playerStat;

    private void Start()
    {
        playerStat = StaticEvents.playerStat;
        Debug.Log("RoomLANConfig playerStat : " + playerStat);
        if (playerStat == "Host") StartHost();
        else StartClient();

        // ע��ͻ��������¼�
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // ȡ��ע���¼�
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
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
        Debug.Log("Host started on 0.0.0.0:" + port + " " + success);

        // �����Զ���ȡ��һ��λ��
        if (success)
        {
            MovePlayerToSpawnPosition(NetworkManager.Singleton.LocalClientId, 0);
        }
    }

    public void StartClient()
    {
        string ip = StaticEvents.hostIP;
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("Please enter a valid IP address.");
            return;
        }

        ConfigureTransportForClient(ip);
        bool success = NetworkManager.Singleton.StartClient();
        Debug.Log($"Connecting to {ip}:{port} " + success);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // ������Ϊÿ�����ӵĿͻ��˷���λ��
            int spawnIndex = NetworkManager.Singleton.ConnectedClients.Count - 1;

            // ȷ��������Ԥ���λ������
            if (spawnIndex < spawnPositions.Count)
            {
                // �ڷ��������ƶ����
                MovePlayerToSpawnPosition(clientId, spawnIndex);

                // ֪ͨ�ͻ����ƶ�
                MovePlayerClientRpc(clientId, spawnPositions[spawnIndex]);
            }
            else
            {
                Debug.LogWarning("No more spawn positions available!");
            }
        }
    }

    [ClientRpc]
    private void MovePlayerClientRpc(ulong clientId, Vector3 position)
    {
        // ֻ�ж�Ӧ�Ŀͻ��˲Ż�ִ���ƶ�
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
}