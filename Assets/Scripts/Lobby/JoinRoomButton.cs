using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JoinRoomButton : MonoBehaviour
{
    public string roomIP;
    public int playerNum;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(JoinRoom);
        }
    }

    public void JoinRoom()
    {
        if (playerNum < 4)
        {
            StaticEvents.playerStat = "Client";
            StaticEvents.hostIP = roomIP;
            SceneTransitionManager.Instance.LoadScene("RoomScene");
        }
        else {
            LobbyUIManager lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
            if(lobbyUIManager != null) lobbyUIManager.OpenPop();
        }
    }
}
