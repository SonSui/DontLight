using UnityEngine;

public class Bulb : MonoBehaviour
{
    private float radius = 2f;
    public float rangeOffset = 0.9f;
    public int damagePerSecond = 1;
    public bool isDebug = false; // �f�o�b�O�p�̃t���O
    public Light bulbLight; // �d����Light�R���|�[�l���g

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // �ŏ��_���[�W����
    private void Start()
    {
        bulbLight = GetComponent<Light>();
        if (bulbLight != null)
        {
            radius = bulbLight.range * rangeOffset; // Light��range��0.9�{���Ďg�p
            if (isDebug) Debug.Log($"[BULB] {name} loaded radius from Light component. radius={radius}");
        }
        else
        {
            // ������ Light ���Ȃ��ꍇ�A�f�t�H���g�l���g��
            radius = 2f * rangeOffset; // �f�t�H���g�̔��a
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

        if (sqrDist < sqrRadius) // �v���C���[�͓d���̔��a���ɂ��邩
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
                // �v���C���[�����ɋ߂��ꍇ�A���ڃ_���[�W��K�p
                if (isDebug) Debug.Log($"[BULB] Player {player.name} is VERY CLOSE to bulb {name}. Direct damage applied.");

                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damageInfo);
            }
            else
            {
                // �v���C���[���d���̔��a���ɂ��邪�A���ɋ߂��Ȃ��ꍇ
                Vector3 dir = diff.normalized;
                float distance = Mathf.Sqrt(sqrDist);

                Ray ray = new Ray(transform.position, dir);
                RaycastHit hit;

                if (isDebug) Debug.DrawRay(transform.position, dir * distance, Color.red, 0.1f);

                // ���C�L���X�g���g�p���āA�d������v���C���[�܂ł̊Ԃɏ�Q�������邩�`�F�b�N
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
