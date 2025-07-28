using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LanRoomDiscovery : MonoBehaviour
{
    [Header("Room List Settings")]
    public GameObject listPanelPrefab;
    public GameObject NoPeople;
    public Transform contentParent;
    
    private long nowTimer;
    private float roomRefresh = 1000f;
    private float roomTimeout = 1800f;
    private bool isListening = true;
    private UdpClient udpReceiver;
    private Dictionary<string, Dictionary<string, string>> roomDetail = new Dictionary<string, Dictionary<string, string>>();

    void Start()
    {
        try
        {
            udpReceiver = new UdpClient(8888);
            udpReceiver.EnableBroadcast = true;
            udpReceiver.BeginReceive(OnReceive, null);
            isListening = true;
            Debug.Log("🎧 ロビーはルームから放送情報の監視を開始…");
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ 監視の開始失敗: " + ex.Message);
        }
        StartCoroutine(RepeatFillinList());
    }

    void Update()
    {
        if (roomDetail.Count == 0) return;
        nowTimer = GetCurrentUnixTimestampMilliseconds();
        List<string> keysToRemove = new List<string>();
        foreach (KeyValuePair<string, Dictionary<string, string>> room in roomDetail)
        {
            string roomIP = room.Key;
            Dictionary<string, string> details = room.Value;
            foreach (KeyValuePair<string, string> detail in details)
            {
                if (detail.Key == "receiveTime")
                {
                    long receiveTime = long.Parse(detail.Value);
                    if (nowTimer - receiveTime > roomTimeout) keysToRemove.Add(roomIP);
                }
                if (detail.Key == "playerNum")
                {
                    int playerNum = int.Parse(detail.Value);
                    if (playerNum == 0) keysToRemove.Add(roomIP);
                }
            }
        }
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
            string messageIP = remoteEP.Address.ToString();
            Dictionary<string, string> room = ParseToDictionary(message);
            room.Add("receiveTime", GetCurrentUnixTimestampMilliseconds().ToString());
            UpdateRoomDetail(messageIP, room);
        } catch (Exception e)
        {
        } finally
        {
            if (isListening && udpReceiver != null)
            {
                try
                {
                    udpReceiver.BeginReceive(OnReceive, null);
                } catch (Exception e)
                {
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
            Debug.Log("🛑 ルームから放送情報の監視停止");
        }
    }

    IEnumerator RepeatFillinList()
    {
        while (true)
        {
            FillinList();
            yield return new WaitForSeconds(roomRefresh / 1000f);
        }
    }

    private void FillinList()
    {
        int eleNum = 0;
        foreach (Transform child in contentParent)
            GameObject.Destroy(child.gameObject);
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
                bool canJoin = true;
                int activeCount = 0;
                foreach (KeyValuePair<string, string> detail in details)
                {
                    if (detail.Key == "roomName")
                    {
                        Transform roomName = firstChild.GetChild(0);
                        Text roomNameText = roomName.GetComponent<Text>();
                        if (roomNameText != null) roomNameText.text = detail.Value;
                    }
                    if (detail.Key == "roomStat")
                    {
                        Transform roomStat = firstChild.GetChild(3);
                        Text roomStatText = roomStat.GetComponent<Text>();
                        if (roomStatText != null)
                        {
                            if (detail.Value == "preparation")
                            {
                                roomStatText.color = new Color(0.3216f, 0.4784f, 0.1569f, 1f);
                                roomStatText.text = "墓を掘ってる";
                            }
                            else
                            {
                                roomStatText.color = new Color(0.8f, 0.1412f, 0.1608f, 1f);
                                roomStatText.text = "埋葬されてる";
                            }
                        }
                    }
                    if (detail.Key == "playerNum")
                    {
                        activeCount = int.Parse(detail.Value);
                        for (int i = 0; i < 4; i++)
                        {
                            Transform roomCheck = firstChild.GetChild(i + 9);
                            roomCheck.gameObject.SetActive(i < activeCount);
                        }
                        if (activeCount == 4) canJoin = false;
                    }
                }
                Transform join = firstChild.GetChild(13);
                JoinRoomButton joinButton = join.GetComponent<JoinRoomButton>();
                if (joinButton != null)
                {
                    Image buttonImage = joinButton.GetComponent<Image>();

                    //if (canJoin) buttonImage.sprite = greenSprite;
                    //else buttonImage.sprite = redSprite;
                    //joinButton.GetComponent<Button>()

                    joinButton.SetInformation(roomIP, activeCount);
                }
                firstChild.gameObject.SetActive(true);
            }
            eleNum++;
        }
        if (roomDetail.Count == 0) NoPeople.SetActive(true);
        else NoPeople.SetActive(false);
    }

    long GetCurrentUnixTimestampMilliseconds()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return now.ToUnixTimeMilliseconds();
    }
}
