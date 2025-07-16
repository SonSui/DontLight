using System.Collections.Generic;
using UnityEngine;

public class SpotLightManager : MonoBehaviour
{
    public LayerMask obstacleMask;//��Q���̃��C���[�}�X�N

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
            // �d�����쐬���ꂽ���Ƃ�ʒm
            Debug.Log("Spotlight created: " + light.name);
        }
    }

    private void RemoveLight(Bulb light)
    {
        bulbs.Remove(light);
        // �d�����j�󂳂ꂽ���Ƃ�ʒm
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
