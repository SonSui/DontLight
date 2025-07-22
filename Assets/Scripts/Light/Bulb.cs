using UnityEngine;
using DG.Tweening;

public class Bulb : MonoBehaviour
{
    private float radius = 2f;
    public float rangeOffset = 0.9f;
    public int damagePerSecond = 1;
    public bool isDebug = false; // �f�o�b�O�p�̃t���O
    public Light bulbLight; // �d����Light�R���|�[�l���g

    private float sqrRadius;
    private float minDmgDistance = 0.01f; // �ŏ��_���[�W����
    private bool isExtinguished = false; // �d�������������ǂ���
    private float spawnTime = 0.5f; // �d���̃X�|�[������
    private void Start()
    {
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
        bulbLight.enabled = false; // ������Ԃł͌��������Ă���
        GameEvents.Light.OnPointLightCreated?.Invoke(this);
    }
    private void Update()
    {
        // �X�|�[�����Ԃ��o�߂�����A�d���̌���L���ɂ���
        if (spawnTime > 0)
        {
            spawnTime -= Time.deltaTime;
            if (spawnTime <= 0 && bulbLight != null)
            {
                bulbLight.enabled = true;
                if (isDebug) Debug.Log($"[BULB] {name} light enabled after spawn time.");
            }
        }
    }
    private void OnDestroy()
    {
        GameEvents.Light.OnPointLightDestroyed?.Invoke(this);
    }

    public void CheckPlayer(GameObject player, LayerMask obstacleMask)
    {

        if (isExtinguished) return;
        if (spawnTime > 0) return;
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
    /// <summary>
    /// �d�������������鏈���i1�b�ԓ_�ł��Ȃ�������キ�Ȃ�j
    /// </summary>
    public void Extinguish()
    {
        if (isExtinguished) return;
        isExtinguished = true;

        if (bulbLight != null)
        {
            float originalIntensity = bulbLight.intensity;

            // �V�[�P���X�쐬
            Sequence seq = DOTween.Sequence();

            
            seq.Append(bulbLight.DOIntensity(0, 0.25f));                    // �Â��Ȃ�
            seq.Append(bulbLight.DOIntensity(originalIntensity, 0.25f));    // ���邭�Ȃ�
            seq.Append(bulbLight.DOIntensity(0, 0.25f));                    // �Â��Ȃ�
            seq.Append(bulbLight.DOIntensity(originalIntensity, 0.25f));    // ���邭�Ȃ�

            
            seq.Append(bulbLight.DOIntensity(0, 0.5f));

            // ������ɍ폜
            seq.OnComplete(() =>
            {
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
