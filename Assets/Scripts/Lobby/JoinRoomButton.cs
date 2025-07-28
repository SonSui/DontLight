using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    private string roomIP;
    private int playerNum;
    private Button button;


    //EventSystem.current.SetSelectedGameObject(firstSelectedButton);
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
            if(lobbyUIManager != null) lobbyUIManager.OpenFullRoomPop();
        }
    }

    public void SetInformation(string roomIP, int playerNum) {
        this.roomIP = roomIP;
        this.playerNum = playerNum;
    }
    
}
