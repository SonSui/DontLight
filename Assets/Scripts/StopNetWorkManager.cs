using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StopNetWorkManager : MonoBehaviour
{
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
            OnReturnToTitleSelected();
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("ホスト切断...");
            roomExitManager.ForceClientToExitClientRpc();
            NetworkManager.Singleton.Shutdown();
            OnReturnToTitleSelected();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("クライアント終了...");
            NetworkManager.Singleton.Shutdown();
            OnReturnToTitleSelected();
        }
        else
        {
            OnReturnToTitleSelected();
        }
    }

    public void OnReturnToTitleSelected()
    {
        GameEvents.UIEvents.OnOnlineStart?.Invoke();
    }
}
