using System;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;

public struct DamageInfo
{
    public GameObject attacker; // �U���҂�GameObject
    public float damage;        // �󂯂��_���[�W��
    public Vector3 hitPoint;    // �_���[�W���󂯂��ʒu
}
[System.Serializable]
public class PlayerData
{
    /// <summary>�v���C���[�ԍ��i0�`3�j</summary>
    public int playerIndex = -1;

    /// <summary>�v���C���[���i�C�Ӂj</summary>
    public string playerName;

    /// <summary>�v���C���[�̐F�iHPUI�����ȂǂɎg�p�j</summary>
    public Color playerColor;

    /// <summary>���A�̃N�[���_�E�������l</summary>
    public float bulbCooldown = 5f;

    /// <summary>����HP�i�ő�l�Ƃ��Ă��g�p�j</summary>
    public float maxHP = 100f;

    /// <summary>�����d�r�c��</summary>
    public float battery = 10f;

    public PlayerInput input;
    public string controlScheme;
    public List<InputDevice> devices = new List<InputDevice>();
}

public class GameEvents
{
    public static class PlayerEvents
    {
        public static Action<InputOnlyPlayer> OnPlayerRegistered; //�v���C���[���o�^���ꂽ�Ƃ��ɌĂ΂��C�x���g
        public static Action<GameObject> OnPlayerSpawned; //�v���C���[���X�|�[�������Ƃ��ɌĂ΂��C�x���g
        public static Action<GameObject> OnPlayerDied;//�v���C���[�����S�����Ƃ��ɌĂ΂��C�x���g
        public static Func<Dictionary<GameObject, PlayerData>> OnQueryAllPlayers;//�S�Ẵv���C���[���擾���邽�߂̃C�x���g
        public static Action<GameObject, DamageInfo> OnTakeLightDamage;//�v���C���[�����C�g����_���[�W���󂯂��Ƃ��ɌĂ΂��C�x���g
        /// <summary>
        /// �v���C���[���o�^���ꂽ���A�ʂ�UI�𐶐�
        /// </summary>
        public static Action<PlayerData> OnPlayerUIAdd;

        // HP�ω�
        public static Action<int, HPInfo> OnHPChanged;

        // �d�r�ω�
        public static Action<int, float,bool> OnBatteryChanged;

        // �d����ԕω�<�v���[���[�ԍ�,�d�����>�i0 = ��, 1 = �����Ă�j
        public static Action<int, int> OnBulbStateChanged;

        public static Action<PlayerData> OnWinnerSet; // ���҂����肵���Ƃ��ɌĂ΂��C�x���g
    }
    public static class Light
    {
        public static Action<Bulb> OnPointLightCreated;//�d�����쐬���ꂽ�Ƃ��ɌĂ΂��C�x���g
        public static Action<Bulb> OnPointLightDestroyed;//�d�����j�󂳂ꂽ�Ƃ��ɌĂ΂��C�x���g
        public static Action<Flashlight> OnFlashlightCreated; // �����d�����쐬���ꂽ�Ƃ��ɌĂ΂��C�x���g
        public static Action<Flashlight> OnFlashlightDestroyed; // �����d�����j�󂳂ꂽ�Ƃ��ɌĂ΂��C�x���g
    }
    public static class UIEvents
    {
        public static Action OnLocalStart;
        public static Action OnOnlineStart;
        public static Action OnGameClose;
        public static Action OnReturnToTitleScene;
        public static Action OnLocalGameStart;
        public static Action OnOnlineGameStart;

        public static Action OnOnlineRoomEnter;
    }
    public static class PrepareUIEvents
    {
        public static Action<PlayerData> OnPlayerDataCreated;
        public static Action<int> OnSetBulbCount;
    }
    public static class GameSceneEvents
    {
        public static Action<GameState> OnBeforeSceneChange;
    }
}
