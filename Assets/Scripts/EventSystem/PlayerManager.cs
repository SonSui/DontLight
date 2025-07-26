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

    private int registeredPlayerCount = 0; // �i�e�X�g�p�j�o�^���ꂽ�v���C���[�̐�

    // �v���C���[�C���f�b�N�X�Ɛ����󋵂̃}�b�s���O
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

            // ������Ԃ�true�ɐݒ�
            playerAliveTable[data.playerIndex] = true;

            // ���� UI
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[inputPlayer.gameObject]);
        }

        GameManager.Instance.ClearJoinedPlayers();
    }
    private void RemovePlayer(GameObject player)
    {
        Debug.Log("Player removed: " + player.name);

        if (!playerMap.TryGetValue(player, out var data))
        {
            Debug.LogWarning("���S�v���C���[���o�^����Ă��܂���B");
            return;
        }

        int index = data.playerIndex;
        playerAliveTable[index] = false;  // �����t���O��false�ɐݒ�
        playerMap.Remove(player);         // �v���C���[�f�[�^�폜

        // �������Ă���v���C���[�𐔂���
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

        // �Ō��1�l�ɂȂ����ꍇ�A�����C�x���g��ʒm
        if (aliveCount == 1)
        {
            foreach (var kvp in playerMap)
            {
                if(kvp.Value.playerIndex == lastAliveIndex)
                {
                    Debug.Log($"���ҁF�v���C���[{lastAliveIndex}");
                    GameEvents.PlayerEvents.OnWinnerSet?.Invoke(kvp.Value);
                    kvp.Key.GetComponent<PlayerTestSon>().SetWinnder();// �����t���O��ݒ�
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

    // GameScene�APlayerInput���o�^��ԂŃe�X�g�p
    private void AddPlayer(GameObject player)
    {
        if (!playerMap.ContainsKey(player))
        {
            int index = registeredPlayerCount;
            registeredPlayerCount++;

            var testScript = player.GetComponent<PlayerTestSon>();
            testScript.SetIndex(index);

            Debug.Log("Player added: " + player.name);

            // UI�p�f�[�^���쐬
            PlayerData data = new PlayerData();
            data.playerIndex = index;
            data.playerName = $"P{index + 1}";
            data.playerColor = GameManager.Instance.playerColors[index % GameManager.Instance.playerColors.Count];
            data.bulbCooldown = 5f;
            data.maxHP = 100f;
            data.battery = 10f;

            playerMap[player] = data;
            // UI����
            GameEvents.PlayerEvents.OnPlayerUIAdd?.Invoke(playerMap[player]);

            playerAliveTable[index] = true;
        }
    }
}