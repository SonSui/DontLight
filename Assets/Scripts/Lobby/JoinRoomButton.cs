using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    private string roomIP;
    private int playerNum;
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
            OnOnlineRoomSelected();
        }
        else {
            LobbyUIManager lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
            if(lobbyUIManager != null) lobbyUIManager.OpenFullRoomPop();
        }
    }

    public void SetInformation(string roomIP, int playerNum) {
        this.roomIP = roomIP;
        this.playerNum = playerNum;
    }

    public void OnOnlineRoomSelected()
    {
        StaticEvents.playerStat = "Client";
        StaticEvents.hostIP = roomIP;
        StaticEvents.clientIP = "test";
        GameEvents.UIEvents.OnOnlineRoomEnter?.Invoke();
    }
}
