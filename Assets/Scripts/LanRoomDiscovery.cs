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

    private Dictionary<string, Dictionary<string, string>> roomDetail = new Dictionary<string, Dictionary<string, string>>();
    private float roomTimeout = 3f;
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

    void Update()
    {
        // 清理超时未响应的房间
        float now = Time.time;
        foreach (KeyValuePair<string, Dictionary<string, string>> room in roomDetail)
        {
            string roomIP = room.Key;
            Dictionary<string, string> details = room.Value;
            Debug.Log($"房间IP: {roomIP}");
            foreach (KeyValuePair<string, string> detail in details)
            {
                Debug.Log($"  {detail.Key} : {detail.Value}");
                if (detail.Key == "receiveTime")
                {
                    float receiveTime = float.Parse(detail.Value);
                    if (now - receiveTime > roomTimeout)
                        roomDetail.Remove(roomIP);
                }
            }
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
            string messageIP = remoteEP.Address.ToString();
            Dictionary<string, string> room = ParseToDictionary(message);
            room.Add("receiveTime", Time.time.ToString());
            UpdateRoomDetail(messageIP, room);
        }
        catch (Exception ex)
        {
            //Debug.LogWarning("接收出错：" + ex.Message);
        }
        finally
        {
            if (isListening && udpReceiver != null)
            {
                try
                {
                    udpReceiver.BeginReceive(OnReceive, null);
                }
                catch (Exception e)
                {
                    //Debug.LogWarning("BeginReceive 失败：" + e.Message);
                }
            }
        }
    }

    Dictionary<string, string> ParseToDictionary(string input)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string[] pairs = input.Split(';');
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
            {
                dict[kv[0]] = kv[1];
            }
        }
        return dict;
    }

    void UpdateRoomDetail(String messageIP, Dictionary<string, string> room) {
        if (roomDetail.ContainsKey(messageIP)) roomDetail[messageIP] = room;
        else roomDetail.Add(messageIP, room);
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
}
