using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;

public class RoomExitManager : NetworkBehaviour
{
    [ClientRpc]
    public void ForceClientToExitClientRpc(string sceneName)
    {
        if (!IsHost && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("收到主机命令：返回大厅");
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
    }
}
