using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    Dictionary<GameObject, PlayerData> playerMap;
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private int registeredPlayerCount = 0; // （テスト用）登録されたプレイヤーの数

    private void Awake()
    {
        if (playerMap == null) playerMap = new Dictionary<GameObject, PlayerData>();
    }
    void Start()
    {
        SpawnPlayersSafely();
    }
    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnPlayerDied += RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage += TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = GetAllPlayers;
        GameEvents.PlayerEvents.OnPlayerSpawned += AddPlayer;

    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnPlayerDied -= RemovePlayer;
        GameEvents.PlayerEvents.OnTakeLightDamage -= TakeDamage;
        GameEvents.PlayerEvents.OnQueryAllPlayers = null;
        GameEvents.PlayerEvents.OnPlayerSpawned -= AddPlayer;
    }
    private void SpawnPlayersSafely()
    {
        var joinedPlayers = GameManager.Instance.GetAllJoinedPlayers();
        foreach (var inputOnly in joinedPlayers)
        {
            PlayerData data = inputOnly.playerData;
            var inputPlayer = PlayerInput.Instantiate(
                playerPrefab,
                controlScheme: data.controlScheme,
                pairWithDevices: data.devices.ToArray(),
                playerIndex: data.playerIndex
            );
            

            inputPlayer.transform.position = spawnPoints[data.playerIndex].position;

            playerMap[inputPlayer.gameObject] = data;
            inputOnly.Delete();

            inputPlayer.GetComponent<PlayerTestSon>().SetIndex(data.playerIndex);

            // 生成 UI
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[inputPlayer.gameObject]);
        }

        GameManager.Instance.ClearJoinedPlayers();
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
            data.playerColor = GameManager.Instance.playerColors[index % GameManager.Instance.playerColors.Count];
            data.bulbCooldown = 5f;
            data.maxHP = 100f;
            data.battery = 10f;

            playerMap[player] = data;
            // UI生成
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[player]);


        }
    }
}