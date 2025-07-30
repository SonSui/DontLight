using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [Header("UI要素")]
    public HPUI hpUI;
    public BulbUI bulbUI;
    public BatteryUI batteryUI;
    public TextMeshProUGUI playerNameText;

    private int playerIndex = -1;

    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnHPChanged += OnHPChanged;
        GameEvents.PlayerEvents.OnBulbStateChanged += OnBulbChanged;
        GameEvents.PlayerEvents.OnBatteryChanged += OnBatteryChanged;
    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnHPChanged -= OnHPChanged;
        GameEvents.PlayerEvents.OnBulbStateChanged -= OnBulbChanged;
        GameEvents.PlayerEvents.OnBatteryChanged -= OnBatteryChanged;
    }

    /// <summary>
    /// プレイヤーデータを元にUI初期化を行う
    /// </summary>
    public void Initialize(PlayerData data)
    {
        this.playerIndex = data.playerIndex;

        if (playerNameText != null)
            playerNameText.text = data.playerName;

        // HP装飾色を設定
        hpUI?.SetUIColor(data.playerColor);
        OnHPChanged(data.playerIndex, new HPInfo(data.maxHP, data.maxHP, false, data.maxHP));

        // 電池残量初期値を設定
        StartCoroutine(DelayedBatteryUpdate(data));

        // 電球クールダウン初期値を設定
        if (bulbUI != null)
        {
            bulbUI.SetCooldown(data.bulbCooldown); // ※この関数はbulbUI側に実装が必要
        }
    }

    private void OnHPChanged(int index, HPInfo info)
    {
        if (index != playerIndex) return;
        hpUI?.UpdateHP(info); // HPUI内部でアニメ処理を維持
    }

    private void OnBulbChanged(int index, int state)
    {
        if (index != playerIndex) return;

        switch (state)
        {
            case 0:
                bulbUI?.SetBulbState(false); // 無
                break;
            case 1:
                bulbUI?.SetBulbState(true);  // 持っている
                break;
        }
    }

    private void OnBatteryChanged(int index, float num,bool isCharge)
    {
        if (index != playerIndex) return;
        batteryUI?.UpdateBattery(num,isCharge); // BatteryUI内部でアニメ処理を維持
    }
    private IEnumerator DelayedBatteryUpdate(PlayerData data)
    {
        yield return null;
        OnBatteryChanged(data.playerIndex, data.battery, false);
    }

}
