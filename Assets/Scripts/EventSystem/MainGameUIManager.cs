using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameUIManager : MonoBehaviour
{
    [Header("�v���C���[UI�v���n�u")]
    public GameObject playerUIPrefab;

    [Header("UI�̐����|�C���g�i�ő�4�j")]
    public Transform[] uiSpawnPoints = new Transform[4];

    [Header("UI�����E���]���邩�itrue�Ŕ��]�j")]
    public bool[] flipUI = new bool[4];

    

    // �v���C���[�ԍ���UI�̕R�t��
    private Dictionary<int, PlayerUI> playerUIs = new Dictionary<int, PlayerUI>();

    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnPlayerUIAdd += CreatePlayerUI;
    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnPlayerUIAdd -= CreatePlayerUI;
    }

    /// <summary>
    /// �v���C���[1�l����UI�𐶐����A�ݒ肷��
    /// </summary>
    /// <param name="data">�v���C���[�f�[�^</param>
    private void CreatePlayerUI(PlayerData data)
    {
        int index = data.playerIndex;
        if (playerUIs.ContainsKey(index))
        {
            Debug.LogWarning($"UI for Player {index} already exists.");
            return;
        }

        if (index >= uiSpawnPoints.Length)
        {
            Debug.LogError($"No spawn point defined for Player {index}.");
            return;
        }

        // UI�𐶐����Ĕz�u
        GameObject uiObj = Instantiate(playerUIPrefab, uiSpawnPoints[index]);
        if (flipUI.Length > index && flipUI[index])
        {
            // X�����ɔ��]
            uiObj.GetComponent<RectTransform>().localScale = new Vector3(-1f,1f,1f);
        }

        // UI�̏�����
        PlayerUI playerUI = uiObj.GetComponent<PlayerUI>();
        playerUI.Initialize(data);
        playerUIs[index] = playerUI;
    }
}