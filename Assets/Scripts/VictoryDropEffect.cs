
using UnityEngine;

public class VictoryDropEffect : MonoBehaviour
{
    [Header("生成エリアの設定")]
    public Vector3 areaCenter = new Vector3(0, 5, 0); // 生成される中心位置
    public Vector2 areaSize = new Vector2(10, 10);    // XZ方向の範囲

    [Header("生成するPrefab")]
    public GameObject[] prefabsToDrop;  // 落ちてくるプレハブのリスト
    public float spawnInterval = 0.2f;  // 何秒ごとに生成するか

    [Header("オブジェクトの寿命")]
    public float autoDestroyTime = 10f; // 生成されたオブジェクトが自動的に消えるまでの時間

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // インターバル時間が経過したら生成
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPrefab();
        }
    }

    // ランダムな位置にPrefabを生成する
    void SpawnPrefab()
    {
        // XZ平面でランダムな位置を決定
        Vector3 spawnPos = areaCenter + new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            0,
            Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );

        // ランダムなプレハブを選択
        GameObject prefab = prefabsToDrop[Random.Range(0, prefabsToDrop.Length)];

        // 生成
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 一定時間後に自動で削除
        Destroy(obj, autoDestroyTime);
    }

    // シーンビューでエリアを視覚的に表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}