using System;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;

public class GameEvents
{
    public static class PlayerEvents
    {
        public static Action<GameObject> OnPlayerSpawned; //プレイヤーがスポーンしたときに呼ばれるイベント
        public static Action<GameObject> OnPlayerDied;//プレイヤーが死亡したときに呼ばれるイベント
        public static Func<List<GameObject>> OnQueryAllPlayers;//全てのプレイヤーを取得するためのイベント
        public static Action<GameObject, float> OnTakeLightDamage;//プレイヤーがライトからダメージを受けたときに呼ばれるイベント
    }
    public static class Light
    {
        public static Action<Bulb> OnPointLightCreated;//電球が作成されたときに呼ばれるイベント
        public static Action<Bulb> OnPointLightDestroyed;//電球が破壊されたときに呼ばれるイベント
    }

}
