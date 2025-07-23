using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class LanRoomAdvertiser : MonoBehaviour
{
    private UdpClient udpSender;
    private float broadcastInterval = 1f;
    private float timer = 0f;
    private int num = 0;
    private string playerStat;

    void Start()
    {
        playerStat = StaticEvents.playerStat;
        Debug.Log("LanRoomAdvertiser playerStat : " + playerStat);
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
        string roomInfo = "roomName=Room" + StaticEvents.hostIP + ";playerNum=" + num + ";roomStat=preparation";
        byte[] data = Encoding.UTF8.GetBytes(roomInfo);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 8888);
        udpSender.Send(data, data.Length, endPoint);

        Debug.Log("📡 已广播房间信息：" + roomInfo);
    }

    void OnDestroy()
    {
        udpSender?.Close();
    }
}
