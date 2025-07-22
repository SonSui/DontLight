using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button backButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(BackToLobby);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BackToLobby()
    {
        SceneTransitionManager.Instance.LoadScene("LobbyScene");
    }
}
