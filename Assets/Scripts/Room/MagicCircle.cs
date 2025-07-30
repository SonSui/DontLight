using Unity.Netcode;
using UnityEngine;

public class MagicCircle : MonoBehaviour
{
    public float triggerRadius = 1f;
    public string playerTag = "Player";
    public RoomExitManager roomExitManager;

    private void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= triggerRadius)
            {
                OnLeaveRoom();
            }
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
