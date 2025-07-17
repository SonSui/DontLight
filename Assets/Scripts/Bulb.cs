using UnityEngine;

public class Bulb : MonoBehaviour
{
    private float radius = 2f;
    public float rangeOffset = 0.9f;
    public int damagePerSecond = 1;
    public bool isDebug = false; // デバッグ用のフラグ
    public Light bulbLight; // 電球のLightコンポーネント

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // 最小ダメージ距離
    private void Start()
    {
        bulbLight = GetComponent<Light>();
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
        GameEvents.Light.OnPointLightCreated?.Invoke(this);
    }
    private void OnDestroy()
    {
        GameEvents.Light.OnPointLightDestroyed?.Invoke(this);
    }

    public void CheckPlayer(GameObject player, LayerMask obstacleMask)
    {
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
}
