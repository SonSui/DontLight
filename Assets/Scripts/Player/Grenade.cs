using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    // グレネードの寿命（秒）
    // Grenade's lifespan in seconds
    // 手榴弹的存活时间（秒）
    [SerializeField] private float lifeTime = 15f;

    void Start()
    {
        // ゲーム開始時に寿命タイマーを開始する
        // Start the lifetime countdown when the game begins
        // 在游戏开始时启动寿命倒计时
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime()
    {
        // 指定時間待機してからオブジェクトを破壊する
        // Wait for the specified time and then destroy the object
        // 等待指定时间后销毁该物体
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
