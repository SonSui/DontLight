using UnityEngine;

public class Flashlight : MonoBehaviour
{
    
    public float damagePerSecond = 1f;         // ���b�_���[�W��
    public float rangeOffset = 0.9f;          // Light��range��spotAngle�Ɋ|����I�t�Z�b�g�l
    public bool isEnabled = true;              // �����d���̗L��/����
    public GameObject owner;                   // �����d���̏��L�ҁi�v���C���[�j
    public bool isDebug = false;               // �f�o�b�O�p�̃t���O
    public Light flashlightLight;              // �����d����Light�R���|�[�l���g

    private float range;                       // ���ۂ̔���p�����W
    private float spotAngle;                   // ���ۂ̔���p�X�|�b�g�p�x
    private void Start()
    {
        GameEvents.Light.OnFlashlightCreated?.Invoke(this); // �����d�����쐬���ꂽ���Ƃ�ʒm
        if (flashlightLight == null)
        {
            flashlightLight = GetComponent<Light>();
        }
        if (flashlightLight != null)
        {
            // Light �� range �� spotAngle �������Ŏ擾���A0.9 ���|����
            range = flashlightLight.range * rangeOffset;
            spotAngle = flashlightLight.spotAngle * rangeOffset;

            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} loaded settings from Light component. range={range}, spotAngle={spotAngle}");
            
        }
        else
        {
            // ������ Light ���Ȃ��ꍇ�A�f�t�H���g�l���g��
            range = 10f * rangeOffset;
            spotAngle = 25f * rangeOffset;
            if (isDebug) Debug.LogWarning($"[FLASHLIGHT] {name} Light component not found. Using default range and spotAngle.");
        }
    }
    private void OnDestroy()
    {
        GameEvents.Light.OnFlashlightDestroyed?.Invoke(this); // �����d�����j�󂳂ꂽ���Ƃ�ʒm
    }

    /// <summary>
    /// �v���C���[�������d���ɏƂ炳��Ă��邩���肷�鏈��
    /// </summary>
    /// <param name="player">���肷��v���C���[</param>
    public void CheckPlayer(GameObject player,LayerMask obstacleMask)
    {
        if (!isEnabled) return; // �����d���������Ȃ牽�����Ȃ�
        if (player == null || player == owner) return; // ���L�Ҏ��g�͖���

        Vector3 lightPos = transform.position;                       // �����d���̈ʒu
        Vector3 lightForward = transform.forward;                    // �����d���̌����iForward�x�N�g���j
        Vector3 dirToPlayer = player.transform.position - lightPos;  // �����d�����v���C���[�����x�N�g��

        float distance = dirToPlayer.magnitude;                      // �����d�����v���C���[�܂ł̋���

        // �Ǝˋ����O�Ȃ画�肵�Ȃ�
        if (distance > range)
            return;

        Vector3 dirNormalized = dirToPlayer.normalized;              // �P�ʃx�N�g�����擾

        // ��d����Forward�����ƃv���C���[�����Ƃ̓��ς��v�Z
        float dot = Vector3.Dot(lightForward, dirNormalized);

        // �X�|�b�g�p�x�̔��������W�A���ɕϊ�
        float halfAngleRad = Mathf.Deg2Rad * (spotAngle / 2f);
        float cosHalfAngle = Mathf.Cos(halfAngleRad);

        // �v���C���[�������d���̏Ǝˊp���ɂ��邩����
        if (dot >= cosHalfAngle)
        {
            // Raycast ���g�p���ď�Q�����`�F�b�N����
            Ray ray = new Ray(lightPos, dirNormalized);
            RaycastHit hit;

            // �f�o�b�O�p��Scene�r���[��Ray��`��i���F�j
            if (isDebug) Debug.DrawRay(lightPos, dirNormalized * distance, Color.red, 0.1f);

            // Ray����Q���ɓ��������ꍇ
            if (Physics.Raycast(ray, out hit, distance, obstacleMask))
            {
                Debug.Log($"[FLASHLIGHT] Raycast from flashlight {name} to player {player.name} hit obstacle: {hit.collider.name} at distance {hit.distance}.");
                if (isDebug) Debug.DrawRay(lightPos, dirNormalized * hit.distance, Color.green, 0.1f);
            }
            else
            {
                // ��Q�����Ȃ���΃v���C���[�����ɓ������Ă���Ɣ���
                if (isDebug) Debug.Log($"[FLASHLIGHT] Raycast from flashlight {name} to player {player.name} hit nothing. Player exposed to light!");
                float damage = damagePerSecond * Time.deltaTime;

                DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = owner,      // �U���Ґݒ�
                        damage = damage,       // �_���[�W�ʂ�ݒ�
                        hitPoint = player.transform.position // �q�b�g�|�C���g���v���C���[�̈ʒu�ɐݒ�
                    };

                // �C�x���g��ʂ��ă_���[�W��ʒm����
                GameEvents.PlayerEvents.OnTakeLightDamage?.Invoke(player, damageInfo);
            }
        }
    }
    
    public void ToggleFlashlight()
    {
        isEnabled = !isEnabled; // �����d���̗L��/������؂�ւ�
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isEnabled;
        }
        if(isDebug) Debug.Log($"[FLASHLIGHT] {name} flashlight is now {(isEnabled ? "enabled" : "disabled")}.");
    }
}