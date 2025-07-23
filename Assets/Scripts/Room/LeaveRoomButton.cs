using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LeaveRoomButton : MonoBehaviour
{
    [Header("返回的场景名称，例如 LobbyScene")]
    public string returnSceneName = "LobbyScene";
    public RoomExitManager roomExitManager;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnLeaveRoom);
        }
        else
        {
            Debug.LogError("LeaveRoomButton 需要挂在带 Button 的对象上");
        }
    }

    void OnLeaveRoom()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager 未找到，直接加载场景");
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("主机退出...");
            roomExitManager.ForceClientToExitClientRpc(returnSceneName);
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("客户端断开连接...");
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
        else
        {
            Debug.Log("未连接任何网络，直接返回场景");
            SceneTransitionManager.Instance.LoadScene(returnSceneName);
        }
    }
}
