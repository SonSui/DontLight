using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class LanRoomDiscovery : MonoBehaviour
{
    private UdpClient udpReceiver;
    private Dictionary<string, string> roomMap = new Dictionary<string, string>();
    private Dictionary<string, float> roomLastSeen = new Dictionary<string, float>();
    private float roomTimeout = 10f;

    void Start()
    {
        udpReceiver = new UdpClient(8888);
        udpReceiver.EnableBroadcast = true;
        udpReceiver.BeginReceive(OnReceive, null);

        Debug.Log("🎧 大厅开始监听房间广播...");
    }

    void OnReceive(IAsyncResult result)
    {
        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpReceiver.EndReceive(result, ref remoteEP);
            string message = Encoding.UTF8.GetString(data);

            Debug.Log($"📨 收到房间广播：{message} 来自：{remoteEP.Address}");

            string ip = remoteEP.Address.ToString();
            roomMap[ip] = message;
            roomLastSeen[ip] = Time.time;

            // TODO: 在这里更新 UI（比如刷新房间列表）
        }
        catch (Exception ex)
        {
            Debug.LogWarning("接收出错：" + ex.Message);
        }
        finally
        {
            // ✅ 确保持续监听
            udpReceiver.BeginReceive(OnReceive, null);
        }
    }

    void Update()
    {
        // 清理超时未响应的房间
        float now = Time.time;
        List<string> toRemove = new List<string>();

        foreach (var kv in roomLastSeen)
        {
            if (now - kv.Value > roomTimeout)
                toRemove.Add(kv.Key);
        }

        foreach (string ip in toRemove)
        {
            Debug.Log($"⚠ 移除超时房间：{ip} | {roomMap[ip]}");
            roomMap.Remove(ip);
            roomLastSeen.Remove(ip);
            // TODO: 更新 UI
        }
    }

    void OnDestroy()
    {
        udpReceiver?.Close();
    }

    // 可选：暴露给 UI 的方法，获取当前所有房间广播信息
    public List<string> GetRoomList()
    {
        return new List<string>(roomMap.Values);
    }
}
