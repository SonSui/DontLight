using UnityEngine;
using System.Collections;

public class PlayerParameters : MonoBehaviour
{
    // 懐中電灯オブジェクト（ON/OFF制御用）
    // Flashlight GameObject to be toggled on/off
    // 手电筒的游戏对象，用于开关控制
    [SerializeField] private GameObject flashlight;

    // 懐中電灯の最大バッテリー容量（秒数として使用）
    // Maximum flashlight battery capacity (used as duration in seconds)
    // 手电筒最大电量（也作为持续时间使用）
    [SerializeField] private float flashlightBattery = 30f;

    // 毎回の充電増加量（RechargeFlashlight()で加算）
    // Battery charge increment per recharge tick
    // 每次充电增加的电量数值
    [SerializeField] private float chargingValue = 1.5f;

    // 懐中電灯が現在点灯しているかどうか
    // Whether the flashlight is currently on
    // 当前手电筒是否处于开启状态
    private bool flashlightIsOn = false;

    // 現在のバッテリー残量
    // Current battery level
    // 当前电池电量
    private float currentBattery = 0f;

    // 懐中電灯を一定時間後に消すためのコルーチン参照
    // Reference to coroutine for turning off the flashlight
    // 控制手电筒定时关闭的协程引用
    private Coroutine flashlightRoutine;

    private void Start()
    {
        flashlight.SetActive(true);
        flashlightIsOn = true;
        StartCoroutine(DisableAfterTime());
    }

    // 懐中電灯を充電する関数
    // Function to recharge the flashlight
    // 给手电筒充电的函数
    public void RechargeFlashlight()
    {
        // 既に点灯している場合は充電不可
        // Do nothing if flashlight is already on
        // 如果手电筒已开启，则不再充电
        if (!flashlightIsOn)
        {
            // 充電量を加算
            // Increase battery level by charging value
            // 增加当前电量
            currentBattery += chargingValue;

            // デバッグ用に現在のバッテリー残量を出力
            // Output current battery level for debugging
            // 调试用输出当前电量
            Debug.Log("Current Battery: " + currentBattery);

            // バッテリーが最大値に達したら点灯
            // If battery reaches full, turn on flashlight
            // 电量达到最大则开启手电筒
            if (currentBattery >= flashlightBattery)
            {
                currentBattery = flashlightBattery;
                flashlightIsOn = true;
                flashlight.SetActive(true);

                // 一定時間後に自動でOFFにする処理を開始
                // Start coroutine to auto-disable flashlight after time
                // 开启定时关闭手电筒的协程
                flashlightRoutine = StartCoroutine(DisableAfterTime());
            }
        }
    }

    // 一定時間経過後に懐中電灯をOFFにする処理
    // Coroutine to disable flashlight after a certain time
    // 在指定时间后自动关闭手电筒的协程
    private IEnumerator DisableAfterTime()
    {
        // 最大電池容量分だけ待機
        // Wait for the duration equal to the battery capacity
        // 等待与最大电量时间相同的秒数
        yield return new WaitForSeconds(flashlightBattery);

        // 消灯処理
        // Turn off flashlight and reset state
        // 关闭手电筒并重置状态
        flashlight.SetActive(false);
        flashlightIsOn = false;
        currentBattery = 0f;
    }
}
