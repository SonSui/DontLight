using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class LanRoomAdvertiser : MonoBehaviour
{
    private float timer = 0f;
    private float broadcastInterval = 1f;
    private string playerStat;
    private UdpClient udpSender;

    void Start()
    {
        playerStat = StaticEvents.playerStat;
        udpSender = new UdpClient();
        udpSender.EnableBroadcast = true;
    }

    void Update()
    {
        if (playerStat != "Host") return;
        timer += Time.deltaTime;
        if (timer >= broadcastInterval)
        {
            timer = 0f;
            BroadcastRoomInfo();
        }
    }

    void BroadcastRoomInfo()
    {
        int currentPlayerNum = NetworkManager.Singleton.ConnectedClients.Count;
        string roomInfo = "roomName=墓地" + StaticEvents.hostIP + ";roomStat=preparation;gameMap=1;playerNum=" + currentPlayerNum;
        byte[] data = Encoding.UTF8.GetBytes(roomInfo);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 8888);
        udpSender.Send(data, data.Length, endPoint);

        Debug.Log("📡 ルーム情報を放送開始：" + roomInfo);
    }

    void OnDestroy()
    {
        udpSender?.Close();
    }
}
