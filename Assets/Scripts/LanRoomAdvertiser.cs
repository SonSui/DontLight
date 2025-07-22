using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LanRoomAdvertiser : MonoBehaviour
{
    private UdpClient udpSender;
    private float broadcastInterval = 1f;
    private float timer = 0f;
    private int num = 0;

    void Start()
    {
        udpSender = new UdpClient();
        udpSender.EnableBroadcast = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= broadcastInterval)
        {
            timer = 0f;
            BroadcastRoomInfo();
        }
    }

    void BroadcastRoomInfo()
    {
        num++;
        string roomInfo = "ROOM_INFO|房间A|人数:" + num + "/4|IP:" + GetLocalIPAddress();
        byte[] data = Encoding.UTF8.GetBytes(roomInfo);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 8888);
        udpSender.Send(data, data.Length, endPoint);

        Debug.Log("📡 已广播房间信息：" + roomInfo);
    }

    string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "Unknown";
    }

    void OnDestroy()
    {
        udpSender?.Close();
    }
}
