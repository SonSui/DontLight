using UnityEngine;

public class KillPlayerWhenColliderEnter : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player")
        {
            // �v���C���[�̃^�O�� "Player" �̏ꍇ�A�v���C���[���폜
            collision.gameObject.GetComponent<PlayerTestSon>().Die();
        }
    }

}
