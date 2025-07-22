using UnityEngine;

public class Flashlight : MonoBehaviour
{
    
    public float damagePerSecond = 1f;         // 毎秒ダメージ量
    public float rangeOffset = 0.9f;          // LightのrangeとspotAngleに掛けるオフセット値
    public bool isEnabled = true;              // 懐中電灯の有効/無効
    public GameObject owner;                   // 懐中電灯の所有者（プレイヤー）
    public bool isDebug = false;               // デバッグ用のフラグ
    public Light flashlightLight;              // 懐中電灯のLightコンポーネント

    private float range;                       // 実際の判定用レンジ
    private float spotAngle;                   // 実際の判定用スポット角度
    private void Start()
    {
        GameEvents.Light.OnFlashlightCreated?.Invoke(this); // 懐中電灯が作成されたことを通知
        if (flashlightLight == null)
        {
            flashlightLight = GetComponent<Light>();
        }
        if (flashlightLight != null)
        {
            // Light の range と spotAngle を自動で取得し、0.9 を掛ける
            range = flashlightLight.range * rangeOffset;
            spotAngle = flashlightLight.spotAngle * rangeOffset;

            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} loaded settings from Light component. range={range}, spotAngle={spotAngle}");
            
        }
        else
        {
            // 万が一 Light がない場合、デフォルト値を使う
            range = 10f * rangeOffset;
            spotAngle = 25f * rangeOffset;
            if (isDebug) Debug.LogWarning($"[FLASHLIGHT] {name} Light component not found. Using default range and spotAngle.");
        }
    }
    private void OnDestroy()
    {
        GameEvents.Light.OnFlashlightDestroyed?.Invoke(this); // 懐中電灯が破壊されたことを通知
    }

    /// <summary>
    /// プレイヤーが懐中電灯に照らされているか判定する処理
    /// </summary>
    /// <param name="player">判定するプレイヤー</param>
    public void CheckPlayer(GameObject player,LayerMask obstacleMask)
    {
        if (!isEnabled) return; // 懐中電灯が無効なら何もしない
        if (player == null || player == owner) return; // 所有者自身は無視

        Vector3 lightPos = transform.position;                       // 懐中電灯の位置
        Vector3 lightForward = transform.forward;                    // 懐中電灯の向き（Forwardベクトル）
        Vector3 dirToPlayer = player.transform.position - lightPos;  // 懐中電灯→プレイヤー方向ベクトル

        float distance = dirToPlayer.magnitude;                      // 懐中電灯→プレイヤーまでの距離

        // 照射距離外なら判定しない
        if (distance > range)
            return;

        Vector3 dirNormalized = dirToPlayer.normalized;              // 単位ベクトルを取得

        // 手電筒のForward方向とプレイヤー方向との内積を計算
        float dot = Vector3.Dot(lightForward, dirNormalized);

        // スポット角度の半分をラジアンに変換
        float halfAngleRad = Mathf.Deg2Rad * (spotAngle / 2f);
        float cosHalfAngle = Mathf.Cos(halfAngleRad);

        // プレイヤーが懐中電灯の照射角内にいるか判定
        if (dot >= cosHalfAngle)
        {
            // Raycast を使用して障害物をチェックする
            Ray ray = new Ray(lightPos, dirNormalized);
            RaycastHit hit;

            // デバッグ用にSceneビューにRayを描画（黄色）
            if (isDebug) Debug.DrawRay(lightPos, dirNormalized * distance, Color.red, 0.1f);

            // Rayが障害物に当たった場合
            if (Physics.Raycast(ray, out hit, distance, obstacleMask))
            {
                Debug.Log($"[FLASHLIGHT] Raycast from flashlight {name} to player {player.name} hit obstacle: {hit.collider.name} at distance {hit.distance}.");
                if (isDebug) Debug.DrawRay(lightPos, dirNormalized * hit.distance, Color.green, 0.1f);
            }
            else
            {
                // 障害物がなければプレイヤーが光に当たっていると判定
                if (isDebug) Debug.Log($"[FLASHLIGHT] Raycast from flashlight {name} to player {player.name} hit nothing. Player exposed to light!");
                float damage = damagePerSecond * Time.deltaTime;

                DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = owner,      // 攻撃者設定
                        damage = damage,       // ダメージ量を設定
                        hitPoint = player.transform.position // ヒットポイントをプレイヤーの位置に設定
                    };

                // イベントを通じてダメージを通知する
                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damageInfo);
            }
        }
    }
    
    public void ToggleFlashlight()
    {
        isEnabled = !isEnabled; // 懐中電灯の有効/無効を切り替え
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isEnabled;
        }
        if(isDebug) Debug.Log($"[FLASHLIGHT] {name} flashlight is now {(isEnabled ? "enabled" : "disabled")}.");
    }
}