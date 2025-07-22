using System;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;

public struct DamageInfo
{
    public GameObject attacker; // 攻撃者のGameObject
    public float damage;        // 受けたダメージ量
    public Vector3 hitPoint;    // ダメージを受けた位置
}
[System.Serializable]
public class PlayerData
{
    /// <summary>プレイヤー番号（0～3）</summary>
    public int playerIndex;

    /// <summary>プレイヤー名（任意）</summary>
    public string playerName;

    /// <summary>プレイヤーの色（HPUI装飾などに使用）</summary>
    public Color playerColor;

    /// <summary>灯泡のクールダウン初期値</summary>
    public float bulbCooldown;

    /// <summary>初期HP（最大値としても使用）</summary>
    public float maxHP;

    /// <summary>初期電池残量</summary>
    public float battery;
}

public class GameEvents
{
    public static class PlayerEvents
    {
        public static Action<GameObject> OnPlayerSpawned; //プレイヤーがスポーンしたときに呼ばれるイベント
        public static Action<GameObject> OnPlayerDied;//プレイヤーが死亡したときに呼ばれるイベント
        public static Func<Dictionary<GameObject, PlayerData>> OnQueryAllPlayers;//全てのプレイヤーを取得するためのイベント
        public static Action<GameObject, DamageInfo> OnTakeLightDamage;//プレイヤーがライトからダメージを受けたときに呼ばれるイベント
        /// <summary>
        /// プレイヤーが登録された時、個別にUIを生成
        /// </summary>
        public static Action<PlayerData> OnPlayerUIAdd;

        // HP変化
        public static Action<int, HPInfo> OnHPChanged;

        // 電池変化
        public static Action<int, float,bool> OnBatteryChanged;

        // 灯泡状態変化（0 = 無, 1 = 持ってる, 2 = CD中）
        public static Action<int, int> OnBulbStateChanged;
    }
    public static class Light
    {
        public static Action<Bulb> OnPointLightCreated;//電球が作成されたときに呼ばれるイベント
        public static Action<Bulb> OnPointLightDestroyed;//電球が破壊されたときに呼ばれるイベント
        public static Action<Flashlight> OnFlashlightCreated; // 懐中電灯が作成されたときに呼ばれるイベント
        public static Action<Flashlight> OnFlashlightDestroyed; // 懐中電灯が破壊されたときに呼ばれるイベント
    }

}
