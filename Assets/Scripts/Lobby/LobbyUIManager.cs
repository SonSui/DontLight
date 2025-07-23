using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button createButton;
    public Button backButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createButton.onClick.AddListener(CreateRoom);
        //backButton.onClick.AddListener(BackToMenu);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateRoom()
    {
        StaticEvents.playerStat = "Host";
        StaticEvents.hostIP = StaticEvents.GetLocalIPAddress();
        SceneTransitionManager.Instance.LoadScene("RoomScene");
    }
}
