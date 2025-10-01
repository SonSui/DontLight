using UnityEngine;
using DG.Tweening;

public class Flashlight : MonoBehaviour
{
    
    public float damagePerSecond = 1f;         // 毎秒ダメージ量
    public float rangeOffset = 0.9f;          // LightのrangeとspotAngleに掛けるオフセット値
    public bool isEnabled = false;              // 懐中電灯の有効/無効
    public GameObject owner;                   // 懐中電灯の所有者（プレイヤー)
    private PlayerTestSon playerParameter;       // 所有者のPlayerTestSonコンポーネント参照
    public bool isDebug = false;               // デバッグ用のフラグ
    public Light flashlightLight;              // 懐中電灯のLightコンポーネント
    public float intensity = 10f;            // 懐中電灯の光の強さ（初期値）

    private float range;                       // 実際の判定用レンジ
    private float spotAngle;                   // 実際の判定用スポット角度

    private Tween shutdownTween;       
    private bool isShuttingDown = false;
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
            intensity = flashlightLight.intensity; // 初期の光の強さを取得
            if (intensity <= 0f)
            {
                intensity = 10f; // 光の強さが0以下ならデフォルト値を設定
            }

            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} loaded settings from Light component. range={range}, spotAngle={spotAngle}");
            
        }
        else
        {
            // 万が一 Light がない場合、デフォルト値を使う
            range = 10f * rangeOffset;
            spotAngle = 25f * rangeOffset;
            if (isDebug) Debug.LogWarning($"[FLASHLIGHT] {name} Light component not found. Using default range and spotAngle.");
        }

        // 懐中電灯の初期状態を設定
        flashlightLight.enabled = isEnabled; // 懐中電灯の有効/無効を設定

        if(owner!=null)playerParameter = owner.GetComponent<PlayerTestSon>();
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
        if (!isEnabled || isShuttingDown) return; // 消灯中なら無視 // 懐中電灯が無効なら何もしない
        if (player == null || player == owner) return; // 所有者自身は無視
        if (playerParameter != null && playerParameter.CurrentBattery <= 0f) return; // 電池切れなら無視

        Vector3 lightPos = transform.position;                       // 懐中電灯の位置
        Vector3 lightForward = transform.forward;                    // 懐中電灯の向き（Forwardベクトル）
        Vector3 dirToPlayer = player.transform.position - lightPos;  // 懐中電灯→プレイヤー方向ベクトル

        float distance = dirToPlayer.magnitude;                      // 懐中電灯→プレイヤーまでの距離

        // 照射距離外なら判定しない
        if (distance > range)
            return;

        Vector3 dirNormalized = dirToPlayer.normalized;              // 単位ベクトルを取得

        // Forward方向とプレイヤー方向との内積を計算
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

    public void ToggleFlashlight(bool toggle)
    {
        // 通常のON/OFF切替、強制消灯中なら中止
        if (isShuttingDown && toggle)
        {
            CancelShutdown();
        }

        isEnabled = toggle;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isEnabled;
            flashlightLight.intensity = isEnabled ? intensity : 0f; // ONなら光の強さを設定、OFFなら0にする
        }
        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} flashlight is now {(isEnabled ? "enabled" : "disabled")}.");
        GetComponent<AudioSource>()?.Play(); // ON/OFF時に音を鳴らす（AudioSourceがあれば）
    }
    /// <summary>
    /// 強制的に懐中電灯を1.5秒かけて消灯（バッテリー切れ時など）
    /// </summary>
    public void ForceShutdown()
    {
        if (!isEnabled) return;
        if (isShuttingDown || flashlightLight == null) return;

        isShuttingDown = true;
        isEnabled = false;

        float originalIntensity = flashlightLight.intensity;

        // 既存のTweenがあれば止める
        shutdownTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // 点滅2回（1秒）
        seq.Append(flashlightLight.DOIntensity(0, 0.25f));
        seq.Append(flashlightLight.DOIntensity(originalIntensity, 0.25f));
        seq.Append(flashlightLight.DOIntensity(0, 0.25f));
        seq.Append(flashlightLight.DOIntensity(originalIntensity, 0.25f));

        // 0.5秒で消灯
        seq.Append(flashlightLight.DOIntensity(0, 0.5f));

        // 完了時にライトを無効化
        seq.OnComplete(() =>
        {
            flashlightLight.enabled = false;
            isEnabled = false; // 懐中電灯を無効化
            shutdownTween = null;
            isShuttingDown = false;
            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} forced shutdown complete.");
        });

        shutdownTween = seq;

        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} forced shutdown started.");
    }

    /// <summary>
    /// 強制消灯中にON指令が来たとき、Tweenをキャンセルして再点灯
    /// </summary>
    public void CancelShutdown()
    {
        if (!isShuttingDown || flashlightLight == null) return;

        shutdownTween?.Kill();
        flashlightLight.intensity = intensity; // 明るさを最大に戻す（必要に応じて修正）
        flashlightLight.enabled = true;
        isEnabled = true;
        isShuttingDown = false;

        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} shutdown canceled and flashlight re-enabled.");
    }
}