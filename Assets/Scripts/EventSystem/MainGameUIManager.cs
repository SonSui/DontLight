using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameUIManager : MonoBehaviour
{
    [Header("プレイヤーUIプレハブ")]
    public GameObject playerUIPrefab;

    [Header("UIの生成ポイント（最大4）")]
    public Transform[] uiSpawnPoints = new Transform[4];

    [Header("UIを左右反転するか（trueで反転）")]
    public bool[] flipUI = new bool[4];

    

    // プレイヤー番号とUIの紐付け
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
    /// プレイヤー1人分のUIを生成し、設定する
    /// </summary>
    /// <param name="data">プレイヤーデータ</param>
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

        // UIを生成して配置
        GameObject uiObj = Instantiate(playerUIPrefab, uiSpawnPoints[index]);
        if (flipUI.Length > index && flipUI[index])
        {
            // X方向に反転
            uiObj.GetComponent<RectTransform>().localScale = new Vector3(-1f,1f,1f);
        }

        // UIの初期化
        PlayerUI playerUI = uiObj.GetComponent<PlayerUI>();
        playerUI.Initialize(data);
        playerUIs[index] = playerUI;
    }
}