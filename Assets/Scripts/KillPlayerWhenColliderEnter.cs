using UnityEngine;

public class KillPlayerWhenColliderEnter : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player")
        {
            // プレイヤーのタグが "Player" の場合、プレイヤーを削除
            collision.gameObject.GetComponent<PlayerTestSon>().Die();
        }
    }

}
