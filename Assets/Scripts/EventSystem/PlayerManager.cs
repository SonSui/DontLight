using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    Dictionary<GameObject, PlayerData> playerMap;
    private int registeredPlayerCount = 0;
    public int maxPlayers = 4;
    public Color[] playerColors = new Color[]
    {
        Color.white,
        Color.blue,
        Color.green,
        Color.yellow
    };
    private void Awake()
    {
        if (playerMap == null) playerMap = new Dictionary<GameObject, PlayerData>();
    }
    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnPlayerSpawned += AddPlayer;
        GameEvents.PlayerEvents.OnPlayerDied += RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage += TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = GetAllPlayers;

    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnPlayerSpawned -= AddPlayer;
        GameEvents.PlayerEvents.OnPlayerDied -= RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage -= TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = null; 
    }

    private void AddPlayer(GameObject player)
    {
        if (!playerMap.ContainsKey(player))
        {
            int index = registeredPlayerCount;
            registeredPlayerCount++;

            var testScript = player.GetComponent<PlayerTestSon>();
            testScript.SetIndex(index);

            Debug.Log("Player added: " + player.name);

            // UI用データを作成
            PlayerData data = new PlayerData();
            data.playerIndex = index;
            data.playerName = $"P{index + 1}";
            data.playerColor = playerColors[index];
            data.bulbCooldown = 5f;
            data.maxHP = 100f;
            data.battery = 10f;

            playerMap[player] = data;
            // UI生成
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[player]);
            

        }
    }

    private void RemovePlayer(GameObject player)
    {
        Debug.Log("Player removed: " + player.name);
        playerMap.Remove(player);
    }

    public Dictionary<GameObject, PlayerData> GetAllPlayers()
    {
        return playerMap;
    }
    public void TakeDamage(GameObject player, DamageInfo damageInfo)
    {
        if (playerMap.ContainsKey(player))
        {
            player.GetComponent<PlayerTestSon>().TakeDamage(damageInfo);
        }
        else
        {
            Debug.LogWarning($"Player {player.name} not found in the list.");
        }
    }
}