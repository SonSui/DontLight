using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    void Update()
    {
        if (IsOwner)
        {
            // 本地控制逻辑
            if (IsServer) position.Value = transform.position;
            else SubmitPositionServerRpc(transform.position);
        }
        else
        {
            // 同步其他玩家位置
            transform.position = position.Value;
        }
    }

    [ServerRpc]
    void SubmitPositionServerRpc(Vector3 newPosition)
    {
        position.Value = newPosition;
    }
}