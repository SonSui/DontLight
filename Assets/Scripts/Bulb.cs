using UnityEngine;

public class Bulb : MonoBehaviour
{
    public float radius = 2f;
    public int damagePerSecond = 1;

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // Minimum distance to avoid division by zero
    private void Start()
    {
        sqrRadius = radius * radius;
        GameEvents.SpotLight.OnSpotLightCreated?.Invoke(this);
    }

    public void CheckPlayer(GameObject player, LayerMask obstacleMask)
    {
        Vector3 diff = player.transform.position - transform.position;
        float sqrDist = diff.sqrMagnitude;

        if (sqrDist < sqrRadius)
        {
            float damage = damagePerSecond * Time.deltaTime;

            if (sqrDist < minDmgDistance * minDmgDistance)
            {
                Debug.Log($"[BULB] Player {player.name} is VERY CLOSE to bulb {gameObject.name}. Direct damage applied.");
                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damage);
            }
            else
            {
                Vector3 dir = diff.normalized;
                float distance = Mathf.Sqrt(sqrDist);

                Ray ray = new Ray(transform.position, dir);
                RaycastHit hit;

                Debug.DrawRay(transform.position, dir * distance, Color.red, 0.1f);

                if (Physics.Raycast(ray, out hit, distance, obstacleMask))
                {
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
