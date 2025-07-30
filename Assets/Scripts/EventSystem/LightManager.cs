using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public LayerMask obstacleMask;//障害物のレイヤーマスク

    private List<Bulb> bulbs = new List<Bulb>();
    private List<Flashlight> flashlights = new List<Flashlight>();

    public int bulbMaxCount = 16; // 最大電球数

    private void OnEnable()
    {
        GameEvents.Light.OnPointLightCreated += AddLight;
        GameEvents.Light.OnPointLightDestroyed += RemoveLight;
        GameEvents.Light.OnFlashlightCreated += AddFlashlight;
        GameEvents.Light.OnFlashlightDestroyed += RemoveFlashlight;
    }

    private void OnDisable()
    {
        GameEvents.Light.OnPointLightCreated -= AddLight;
        GameEvents.Light.OnPointLightDestroyed -= RemoveLight;
        GameEvents.Light.OnFlashlightCreated -= AddFlashlight;
        GameEvents.Light.OnFlashlightDestroyed -= RemoveFlashlight;
    }

    private void Start()
    {
        bulbMaxCount = GameManager.Instance?.GetMaxBulbCount() ?? bulbMaxCount; // GameManagerから最大電球数を取得
    }
    private void AddLight(Bulb light)
    {
        if (!bulbs.Contains(light))
        {
            bulbs.Add(light);
            // 電球が作成されたことを通知
            Debug.Log("Spotlight created: " + light.name);
            // 電球の数が最大数を超えた場合、最も古い電球を削除
            if (bulbs.Count > bulbMaxCount)
            {
                int count = bulbs.Count - bulbMaxCount;
                for (int i = 0; i < count; i++)
                {
                    if(bulbs.Count <= i) break; // 安全チェック
                    bulbs[i].Extinguish(); // 電球を消灯
                    Debug.Log("Removed oldest spotlight: " + bulbs[i].name);
                }
            }
        }
    }

    private void RemoveLight(Bulb light)
    {
        bulbs.Remove(light);
        // 電球が破壊されたことを通知
        Debug.Log("Spotlight destroyed: " + light.name);
    }

    private void AddFlashlight(Flashlight flashlight)
    {
        if (!flashlights.Contains(flashlight))
        {
            flashlights.Add(flashlight);
            // 懐中電灯が作成されたことを通知
            Debug.Log("Flashlight created: " + flashlight.name);
        }
    }
    private void RemoveFlashlight(Flashlight flashlight)
    {
        flashlights.Remove(flashlight);
        // 懐中電灯が破壊されたことを通知
        Debug.Log("Flashlight destroyed: " + flashlight.name);
    }


    private void Update()
    {
        var players = GameEvents.PlayerEvents.OnQueryAllPlayers?.Invoke();
        if (players == null) return;

        // Dictionary を List に変換して列挙を安全にする
        var playerList = new List<KeyValuePair<GameObject, PlayerData>>(players);

        foreach (var light in bulbs)
        {
            foreach (var player in playerList)
            {
                if (player.Key == null) continue;
                light.CheckPlayer(player.Key, obstacleMask);
            }
        }

        foreach (var flashlight in flashlights)
        {
            foreach (var player in playerList)
            {
                if (player.Key == null) continue;
                flashlight.CheckPlayer(player.Key, obstacleMask);
            }
        }
    }
}
