using System.Collections.Generic;
using UnityEngine;

public class SpotLightManager : MonoBehaviour
{
    public LayerMask obstacleMask;//障害物のレイヤーマスク

    private List<Bulb> bulbs = new List<Bulb>();

    private void OnEnable()
    {
        GameEvents.Light.OnPointLightCreated += AddLight;
        GameEvents.Light.OnPointLightDestroyed += RemoveLight;
    }

    private void OnDisable()
    {
        GameEvents.Light.OnPointLightCreated -= AddLight;
        GameEvents.Light.OnPointLightDestroyed -= RemoveLight;
    }

    private void AddLight(Bulb light)
    {
        if (!bulbs.Contains(light))
        {
            bulbs.Add(light);
            // 電球が作成されたことを通知
            Debug.Log("Spotlight created: " + light.name);
        }
    }

    private void RemoveLight(Bulb light)
    {
        bulbs.Remove(light);
        // 電球が破壊されたことを通知
        Debug.Log("Spotlight destroyed: " + light.name);
    }

    private void Update()
    {
        var players = GameEvents.PlayerEvents.OnQueryAllPlayers?.Invoke();
        if (players == null) return;

        foreach (var light in bulbs)
        {
            foreach (var player in players)
            {
                light.CheckPlayer(player, obstacleMask);
                //Debug.Log($"Checking player {player.name} with light {light.name}.");
            }
        }
    }
}
