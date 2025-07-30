using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [Header("UI�v�f")]
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
    /// �v���C���[�f�[�^������UI���������s��
    /// </summary>
    public void Initialize(PlayerData data)
    {
        this.playerIndex = data.playerIndex;

        if (playerNameText != null)
            playerNameText.text = data.playerName;

        // HP�����F��ݒ�
        hpUI?.SetUIColor(data.playerColor);
        OnHPChanged(data.playerIndex, new HPInfo(data.maxHP, data.maxHP, false, data.maxHP));

        // �d�r�c�ʏ����l��ݒ�
        StartCoroutine(DelayedBatteryUpdate(data));

        // �d���N�[���_�E�������l��ݒ�
        if (bulbUI != null)
        {
            bulbUI.SetCooldown(data.bulbCooldown); // �����̊֐���bulbUI���Ɏ������K�v
        }
    }

    private void OnHPChanged(int index, HPInfo info)
    {
        if (index != playerIndex) return;
        hpUI?.UpdateHP(info); // HPUI�����ŃA�j���������ێ�
    }

    private void OnBulbChanged(int index, int state)
    {
        if (index != playerIndex) return;

        switch (state)
        {
            case 0:
                bulbUI?.SetBulbState(false); // ��
                break;
            case 1:
                bulbUI?.SetBulbState(true);  // �����Ă���
                break;
        }
    }

    private void OnBatteryChanged(int index, float num,bool isCharge)
    {
        if (index != playerIndex) return;
        batteryUI?.UpdateBattery(num,isCharge); // BatteryUI�����ŃA�j���������ێ�
    }
    private IEnumerator DelayedBatteryUpdate(PlayerData data)
    {
        yield return null;
        OnBatteryChanged(data.playerIndex, data.battery, false);
    }

}
