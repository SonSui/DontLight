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
    private bool isListening = true;

    void Start()
    {
        try
        {
            udpReceiver = new UdpClient(8888);
            udpReceiver.EnableBroadcast = true;
            udpReceiver.BeginReceive(OnReceive, null);

            isListening = true;

            Debug.Log("🎧 大厅开始监听房间广播...");
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ 启动监听失败: " + ex.Message);
        }
    }

    void OnReceive(IAsyncResult result)
    {
        if (!isListening || udpReceiver == null)
            return;

        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpReceiver.EndReceive(result, ref remoteEP);
            string message = Encoding.UTF8.GetString(data);

            Debug.Log($"📨 收到房间广播：{message} 来自：{remoteEP.Address}");

            string ip = remoteEP.Address.ToString();
            roomMap[ip] = message;
            roomLastSeen[ip] = Time.time;

            // TODO: 刷新 UI
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("⚠ UdpClient 已关闭，停止接收。");
            return;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("接收出错：" + ex.Message);
        }
        finally
        {
            // 继续监听（只有在 still active 时）
            if (isListening && udpReceiver != null)
            {
                try
                {
                    udpReceiver.BeginReceive(OnReceive, null);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("BeginReceive 失败：" + e.Message);
                }
            }
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
        isListening = false;

        if (udpReceiver != null)
        {
            udpReceiver.Close();
            udpReceiver = null;
            Debug.Log("🛑 停止监听房间广播。");
        }
    }

    // 提供当前房间信息
    public List<string> GetRoomList()
    {
        return new List<string>(roomMap.Values);
    }
}
