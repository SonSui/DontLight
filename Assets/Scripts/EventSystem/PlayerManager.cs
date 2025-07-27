using System;
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

    // プレイヤーインデックスと生存状況のマッピング
    private Dictionary<int, bool> playerAliveTable = new();

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
            inputPlayer.GetComponent<PlayerTestSon>().SetPlayerData(data);

            // 生存状態をtrueに設定
            playerAliveTable[data.playerIndex] = true;

            // 生成 UI
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[inputPlayer.gameObject]);
        }

        GameManager.Instance.ClearJoinedPlayers();
    }
    private void RemovePlayer(GameObject player)
    {
        Debug.Log("Player removed: " + player.name);

        if (!playerMap.TryGetValue(player, out var data))
        {
            Debug.LogWarning("死亡プレイヤーが登録されていません。");
            return;
        }

        int index = data.playerIndex;
        playerAliveTable[index] = false;  // 生存フラグをfalseに設定
        playerMap.Remove(player);         // プレイヤーデータ削除

        // 生存しているプレイヤーを数える
        int aliveCount = 0;
        int lastAliveIndex = -1;

        foreach (var kvp in playerAliveTable)
        {
            if (kvp.Value)
            {
                aliveCount++;
                lastAliveIndex = kvp.Key;
            }
        }

        // 最後の1人になった場合、勝利イベントを通知
        if (aliveCount == 1)
        {
            foreach (var kvp in playerMap)
            {
                if(kvp.Value.playerIndex == lastAliveIndex)
                {
                    Debug.Log($"勝者：プレイヤー{lastAliveIndex}");
                    GameEvents.PlayerEvents.OnWinnerSet?.Invoke(kvp.Value);
                    kvp.Key.GetComponent<PlayerTestSon>().SetWinnder();// 勝利フラグを設定
                    break;
                }
            }
        }
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

    // GameScene、PlayerInput未登録状態でテスト用
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

            playerAliveTable[index] = true;
        }
    }
}