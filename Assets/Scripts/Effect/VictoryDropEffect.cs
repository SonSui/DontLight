
using UnityEngine;

public class VictoryDropEffect : MonoBehaviour
{
    [Header("�����G���A�̐ݒ�")]
    public Vector3 areaCenter = new Vector3(0, 5, 0); // ��������钆�S�ʒu
    public Vector2 areaSize = new Vector2(10, 10);    // XZ�����͈̔�

    [Header("��������Prefab")]
    public GameObject[] prefabsToDrop;  // �����Ă���v���n�u�̃��X�g
    public float spawnInterval = 0.2f;  // ���b���Ƃɐ������邩

    [Header("�I�u�W�F�N�g�̎���")]
    public float autoDestroyTime = 10f; // �������ꂽ�I�u�W�F�N�g�������I�ɏ�����܂ł̎���

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // �C���^�[�o�����Ԃ��o�߂����琶��
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPrefab();
        }
    }

    // �����_���Ȉʒu��Prefab�𐶐�����
    void SpawnPrefab()
    {
        // XZ���ʂŃ����_���Ȉʒu������
        Vector3 spawnPos = areaCenter + new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            0,
            Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );

        // �����_���ȃv���n�u��I��
        GameObject prefab = prefabsToDrop[Random.Range(0, prefabsToDrop.Length)];

        // ����
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // ��莞�Ԍ�Ɏ����ō폜
        Destroy(obj, autoDestroyTime);
    }

    // �V�[���r���[�ŃG���A�����o�I�ɕ\��
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}