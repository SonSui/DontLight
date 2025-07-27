using Unity.Netcode;
using UnityEngine;

public class RoomExitManager : NetworkBehaviour
{
    [ClientRpc]
    public void ForceClientToExitClientRpc(string sceneName)
    {
        if (!IsHost && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("ホストコマンドを受信: ロビーに戻る");
            NetworkManager.Singleton.Shutdown();
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
    }
}
