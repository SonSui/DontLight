using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StopNetWorkManager : MonoBehaviour
{
    public string returnSceneName;
    public RoomExitManager roomExitManager;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnLeaveRoom);
        }
    }

    void OnLeaveRoom()
    {
        if (NetworkManager.Singleton == null)
        {
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("¥Û¥¹¥ÈÇÐ¶Ï...");
            roomExitManager.ForceClientToExitClientRpc(returnSceneName);
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("¥¯¥é¥¤¥¢¥ó¥ÈÇÐ¶Ï...");
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
        else
        {
            Debug.Log("¥Í¥Ã¥È¥ï©`¥¯¤Ë½Ó¾A¤µ¤ì¤Æ¤¤¤Ê¤¤");
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
    }
}
