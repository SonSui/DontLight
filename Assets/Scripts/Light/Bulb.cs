using UnityEngine;
using DG.Tweening;

public class Bulb : MonoBehaviour
{
    [Header("電球の設定")]
    private float radius = 2f;
    public float rangeOffset = 0.9f;
    public int damagePerSecond = 1;
    public bool isDebug = false; // デバッグ用のフラグ
    public Light bulbLight; // 電球のLightコンポーネント

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // 最小ダメージ距離
    private bool isExtinguished = false; // 電球が消灯中かどうか
    private float spawnTime = 0.5f; // 電球のスポーン時間
    private Sequence extinguishSequence;

    [Header("高速衝突の設定")]
    public float highSpeedThreshold = 3f; // 高速のしきい値
    public float highSpeedDamage = 5f;   // 高速時の追加ダメージ
    public GameObject impactEffect2D;     // 2Dヒットエフェクト
    public float highSpeedTime = 1.8f;
    private void Start()
    {
        if (bulbLight != null)
        {
            radius = bulbLight.range * rangeOffset; // Lightのrangeを0.9倍して使用
            if (isDebug) Debug.Log($"[BULB] {name} loaded radius from Light component. radius={radius}");
        }
        else
        {
            // 万が一 Light がない場合、デフォルト値を使う
            radius = 2f * rangeOffset; // デフォルトの半径
            if (isDebug) Debug.LogWarning($"[BULB] {name} Light component not found. Using default radius.");
        }
        sqrRadius = radius * radius;
        bulbLight.enabled = false; // 初期状態では光を消しておく
        GameEvents.Light.OnPointLightCreated?.Invoke(this);
    }
    private void Update()
    {
        // スポーン時間が経過したら、電球の光を有効にする
        if (spawnTime > 0)
        {
            spawnTime -= Time.deltaTime;
            if (spawnTime <= 0 && bulbLight != null)
            {
                bulbLight.enabled = true;
                if (isDebug) Debug.Log($"[BULB] {name} light enabled after spawn time.");
            }
        }
        if(highSpeedDamage > 0)
        {
            // 高速衝突後のダメージ処理
            highSpeedTime -= Time.deltaTime;
            
        }
    }
    private void OnDestroy()
    {
        if (extinguishSequence != null && extinguishSequence.IsActive())
        {
            extinguishSequence.Kill();
        }
        GameEvents.Light.OnPointLightDestroyed?.Invoke(this);
    }

    public void CheckPlayer(GameObject player, LayerMask obstacleMask)
    {

        if (isExtinguished) return;
        if (spawnTime > 0) return;
        Vector3 diff = player.transform.position - transform.position;
        float sqrDist = diff.sqrMagnitude;

        if (sqrDist < sqrRadius) // プレイヤーは電球の半径内にいるか
        {
            float damage = damagePerSecond * Time.deltaTime;
            DamageInfo damageInfo = new DamageInfo
            {
                attacker = gameObject,
                damage = damage,
                hitPoint = player.transform.position
            };

            if (sqrDist < minDmgDistance * minDmgDistance)
            {
                // プレイヤーが非常に近い場合、直接ダメージを適用
                if (isDebug) Debug.Log($"[BULB] Player {player.name} is VERY CLOSE to bulb {name}. Direct damage applied.");

                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damageInfo);
            }
            else
            {
                // プレイヤーが電球の半径内にいるが、非常に近くない場合
                Vector3 dir = diff.normalized;
                float distance = Mathf.Sqrt(sqrDist);

                Ray ray = new Ray(transform.position, dir);
                RaycastHit hit;

                if (isDebug) Debug.DrawRay(transform.position, dir * distance, Color.red, 0.1f);

                // レイキャストを使用して、電球からプレイヤーまでの間に障害物があるかチェック
                if (Physics.Raycast(ray, out hit, distance, obstacleMask))
                {
                    if (isDebug) Debug.DrawRay(transform.position, dir * hit.distance, Color.green, 0.1f);
                    if (isDebug) Debug.Log($"[BULB] Raycast from bulb {gameObject.name} to player {player.name} hit obstacle: {hit.collider.name} at distance {hit.distance}.");
                }
                else
                {
                    if (isDebug) Debug.Log($"[BULB] Raycast from bulb {gameObject.name} to player {player.name} hit nothing. Player exposed to light!");
                    GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damageInfo);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isExtinguished || highSpeedTime <= 0f) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude >= highSpeedThreshold)
            {
                // 高速衝突と判定
                Debug.Log($"[BULB] {name} high-speed collided with {collision.gameObject.name}");

                // ダメージ計算
                DamageInfo damageInfo = new DamageInfo
                {
                    attacker = gameObject,
                    damage = highSpeedDamage,
                    hitPoint = collision.contacts[0].point
                };
                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(collision.gameObject, damageInfo);

                // 2Dヒットエフェクト生成
                if (impactEffect2D != null)
                {
                    Vector3 spawnPos = collision.contacts[0].point;
                    Instantiate(impactEffect2D, spawnPos, Quaternion.identity);
                }
            }
        }
    }

    /// <summary>
    /// 電球を消灯させる処理（1秒間点滅しながら光が弱くなる）
    /// </summary>
    public void Extinguish()
    {
        if (isExtinguished) return;
        isExtinguished = true;

        if (bulbLight != null)
        {
            float originalIntensity = bulbLight.intensity;

            // シーケンス作成
            extinguishSequence = DOTween.Sequence();


            extinguishSequence.Append(bulbLight.DOIntensity(0, 0.25f));                    // 暗くなる
            extinguishSequence.Append(bulbLight.DOIntensity(originalIntensity, 0.25f));    // 明るくなる
            extinguishSequence.Append(bulbLight.DOIntensity(0, 0.25f));                    // 暗くなる
            extinguishSequence.Append(bulbLight.DOIntensity(originalIntensity, 0.25f));    // 明るくなる


            extinguishSequence.Append(bulbLight.DOIntensity(0, 0.5f));

            // 完了後に削除
            extinguishSequence.OnComplete(() =>
            {
                if (this == null || gameObject == null) return;
                if (isDebug) Debug.Log($"[BULB] {name} extinguished and destroyed.");
                Destroy(gameObject);
            });

            if (isDebug) Debug.Log($"[BULB] {name} extinguish sequence started.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}
