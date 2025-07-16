using UnityEngine;

public class Bulb : MonoBehaviour
{
    public float radius = 2f;
    public int damagePerSecond = 1;
    public bool isDebug = false; // デバッグ用のフラグ

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // 最小ダメージ距離
    private void Start()
    {
        sqrRadius = radius * radius;
        GameEvents.Light.OnPointLightCreated?.Invoke(this);
    }

    public void CheckPlayer(GameObject player, LayerMask obstacleMask)
    {
        Vector3 diff = player.transform.position - transform.position;
        float sqrDist = diff.sqrMagnitude;

        if (sqrDist < sqrRadius) // プレイヤーは電球の半径内にいるか
        {
            float damage = damagePerSecond * Time.deltaTime;

            if (sqrDist < minDmgDistance * minDmgDistance)
            {
                // プレイヤーが非常に近い場合、直接ダメージを適用
                Debug.Log($"[BULB] Player {player.name} is VERY CLOSE to bulb {gameObject.name}. Direct damage applied.");
                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damage);
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
                    Debug.Log($"[BULB] Raycast from bulb {gameObject.name} to player {player.name} hit obstacle: {hit.collider.name} at distance {hit.distance}.");
                }
                else
                { 
                    Debug.Log($"[BULB] Raycast from bulb {gameObject.name} to player {player.name} hit nothing. Player exposed to light!");
                    GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damage);
                }
            }
        }
    }
}
