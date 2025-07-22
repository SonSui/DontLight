using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class LanRoomDiscovery : MonoBehaviour
{
    public GameObject listPanelPrefab;
    public Transform contentParent;
    private UdpClient udpReceiver;
    private long nowTimer;
    private float roomRefresh = 1f;
    private float roomTimeout = 1200f;
    private bool isListening = true;
    private Dictionary<string, Dictionary<string, string>> roomDetail = new Dictionary<string, Dictionary<string, string>>();

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
        StartCoroutine(RepeatFillinList());
    }

    void Update()
    {
        // 清理超时未响应的房间
        if (roomDetail.Count == 0) return;
        nowTimer = GetCurrentUnixTimestampMilliseconds();
        List<string> keysToRemove = new List<string>();
        foreach (KeyValuePair<string, Dictionary<string, string>> room in roomDetail)
        {
            string roomIP = room.Key;
            Dictionary<string, string> details = room.Value;
            //Debug.Log($"房间IP: {roomIP}");
            foreach (KeyValuePair<string, string> detail in details)
            {
                //Debug.Log($"  {detail.Key} : {detail.Value}");
                if (detail.Key == "receiveTime")
                {
                    long receiveTime = long.Parse(detail.Value);
                    //Debug.Log("nowTimer : " + nowTimer);
                    //Debug.Log("nowTimer - receiveTime : " + (nowTimer - receiveTime));
                    if (nowTimer - receiveTime > roomTimeout)
                    {
                        keysToRemove.Add(roomIP);
                    }
                        
                }
            }
        }
        // 统一删除超时房间
        foreach (string key in keysToRemove)
            roomDetail.Remove(key);
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
            //Debug.Log($"📨 收到房间广播：{message} 来自：{remoteEP.Address}");
            string messageIP = remoteEP.Address.ToString();
            Dictionary<string, string> room = ParseToDictionary(message);
            room.Add("receiveTime", GetCurrentUnixTimestampMilliseconds().ToString());
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

    IEnumerator RepeatFillinList()
    {
        while (true)
        {
            FillinList();
            yield return new WaitForSeconds(roomRefresh);
        }
    }

    private void FillinList()
    {
        int eleNum = 0;
        foreach (Transform child in contentParent)
            GameObject.Destroy(child.gameObject);
        Debug.Log("roomDetail :" + roomDetail.Count);
        GameObject newPanel = null;
        foreach (KeyValuePair<string, Dictionary<string, string>> room in roomDetail)
        {
            string roomIP = room.Key;
            Dictionary<string, string> details = room.Value;
            if (eleNum % 2 == 0)
                newPanel = Instantiate(listPanelPrefab, contentParent);

            if (newPanel != null)
            {
                Transform firstChild = newPanel.transform.GetChild(eleNum % 2);
                Transform secondChild = firstChild.GetChild(0);
                Text textComponent = secondChild.GetComponent<Text>();
                if (textComponent != null)
                {
                    foreach (KeyValuePair<string, string> detail in details)
                    {
                        if (detail.Key == "roomName")
                        {
                            textComponent.text = detail.Value;
                        }
                        firstChild.gameObject.SetActive(true);
                    }
                }
            }
            eleNum++;
        }
    }

    long GetCurrentUnixTimestampMilliseconds()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return now.ToUnixTimeMilliseconds();
    }
}
