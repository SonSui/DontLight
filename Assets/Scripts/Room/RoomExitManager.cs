using Unity.Netcode;
using UnityEngine;

public class RoomExitManager : NetworkBehaviour
{
    [ClientRpc]
    public void ForceClientToExitClientRpc()
    {
        if (!IsHost && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("ホストコマンドを受信: ロビーに戻る");
            StaticEvents.Dissolution = true;
            NetworkManager.Singleton.Shutdown();
            OnReturnToTitleSelected();
        }
    }

    public void OnReturnToTitleSelected()
    {
        GameEvents.UIEvents.OnOnlineStart?.Invoke();
    }
}
