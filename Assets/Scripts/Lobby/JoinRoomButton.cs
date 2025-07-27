using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    public string roomIP;

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
        StaticEvents.playerStat = "Client";
        StaticEvents.hostIP = roomIP;
        SceneTransitionManager.Instance.LoadScene("RoomScene");
    }
}
