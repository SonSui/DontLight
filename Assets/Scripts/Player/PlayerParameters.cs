using UnityEngine;
using System.Collections;

public class PlayerParameters : MonoBehaviour
{
    // 懐中電灯オブジェクト（ON/OFF制御用）
    // Flashlight GameObject to be toggled on/off and UI
    // 手电筒的游戏对象，用于开关控制
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private BatteryUI batteryUI;

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

    private PlayerData playerData = new PlayerData();
    public Material playerMaterial;
    private Material usingMaterial;
    public Renderer playerRenderer;

    public void SetIndex(int index)
    {
        playerData.playerIndex = index;
    }
    public void SetPlayerData(PlayerData data)
    {
        playerData = data;

        usingMaterial = new Material(playerMaterial);
        usingMaterial.SetColor("_MainColor", playerData.playerColor);
        usingMaterial.SetColor("_DissolveColor", playerData.playerColor);
        if (playerRenderer != null && playerRenderer.material != null)
        {
            playerRenderer.material = usingMaterial;
        }
        else
        {
            Debug.LogWarning("PlayerRenderer or Material is not set.");
        }
    }

    private void Start()
    {
        currentBattery = flashlightBattery;

        flashlight.ToggleFlashlight(true);
        flashlightIsOn = true;

        batteryUI?.UpdateBattery(currentBattery, false, flashlightBattery);

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
            currentBattery = Mathf.Min(currentBattery, flashlightBattery); // Maximize limit to flashlightBattery 最大值制限
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // Update battery UI if available 更新 UI（充電中）

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
                flashlight.ToggleFlashlight(true);

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
        float duration = flashlightBattery;
        float interval = 1f;
        int steps = Mathf.FloorToInt(duration / interval);
        float batteryPerStep = flashlightBattery / steps;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(interval);

            currentBattery -= batteryPerStep;
            currentBattery = Mathf.Max(currentBattery, 0f);

            // 实时更新 UI + 广播事件
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false);
            batteryUI?.UpdateBattery(currentBattery, false, flashlightBattery);
        }

        // 电量耗尽，关灯
        flashlight.ToggleFlashlight(false);
        flashlightIsOn = false;
        currentBattery = 0f;

        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false);
        batteryUI?.UpdateBattery(currentBattery, false, flashlightBattery);
    }
}
