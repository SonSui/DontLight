using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button createButton;
    public Button backButton;

    void Start()
    {
        createButton.onClick.AddListener(CreateRoom);
        backButton.onClick.AddListener(BackToTitle);
    }

    public void CreateRoom()
    {
        StaticEvents.playerStat = "Host";
        StaticEvents.hostIP = StaticEvents.GetLocalIPAddress();
        SceneTransitionManager.Instance.LoadScene("RoomScene");
    }

    public void BackToTitle()
    {
        SceneTransitionManager.Instance.LoadScene("Title");
    }
}
