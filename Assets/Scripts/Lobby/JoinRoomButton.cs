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
            button.onClick.AddListener(joinRoom);
        }
    }

    public void joinRoom()
    {
        StaticEvents.playerStat = "Client";
        StaticEvents.hostIP = roomIP;
        SceneTransitionManager.Instance.LoadScene("RoomScene");
    }
}
