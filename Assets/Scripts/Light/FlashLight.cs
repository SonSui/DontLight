using UnityEngine;
using DG.Tweening;

public class Flashlight : MonoBehaviour
{
    
    public float damagePerSecond = 1f;         // ���b�_���[�W��
    public float rangeOffset = 0.9f;          // Light��range��spotAngle�Ɋ|����I�t�Z�b�g�l
    public bool isEnabled = false;              // �����d���̗L��/����
    public GameObject owner;                   // �����d���̏��L�ҁi�v���C���[)
    private PlayerTestSon playerParameter;       // ���L�҂�PlayerTestSon�R���|�[�l���g�Q��
    public bool isDebug = false;               // �f�o�b�O�p�̃t���O
    public Light flashlightLight;              // �����d����Light�R���|�[�l���g
    public float intensity = 10f;            // �����d���̌��̋����i�����l�j

    private float range;                       // ���ۂ̔���p�����W
    private float spotAngle;                   // ���ۂ̔���p�X�|�b�g�p�x

    private Tween shutdownTween;       
    private bool isShuttingDown = false;
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
            intensity = flashlightLight.intensity; // �����̌��̋������擾
            if (intensity <= 0f)
            {
                intensity = 10f; // ���̋�����0�ȉ��Ȃ�f�t�H���g�l��ݒ�
            }

            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} loaded settings from Light component. range={range}, spotAngle={spotAngle}");
            
        }
        else
        {
            // ������ Light ���Ȃ��ꍇ�A�f�t�H���g�l���g��
            range = 10f * rangeOffset;
            spotAngle = 25f * rangeOffset;
            if (isDebug) Debug.LogWarning($"[FLASHLIGHT] {name} Light component not found. Using default range and spotAngle.");
        }

        // �����d���̏�����Ԃ�ݒ�
        flashlightLight.enabled = isEnabled; // �����d���̗L��/������ݒ�

        if(owner!=null)playerParameter = owner.GetComponent<PlayerTestSon>();
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
        if (!isEnabled || isShuttingDown) return; // �������Ȃ疳�� // �����d���������Ȃ牽�����Ȃ�
        if (player == null || player == owner) return; // ���L�Ҏ��g�͖���
        if (playerParameter != null && playerParameter.CurrentBattery <= 0f) return; // �d�r�؂�Ȃ疳��

        Vector3 lightPos = transform.position;                       // �����d���̈ʒu
        Vector3 lightForward = transform.forward;                    // �����d���̌����iForward�x�N�g���j
        Vector3 dirToPlayer = player.transform.position - lightPos;  // �����d�����v���C���[�����x�N�g��

        float distance = dirToPlayer.magnitude;                      // �����d�����v���C���[�܂ł̋���

        // �Ǝˋ����O�Ȃ画�肵�Ȃ�
        if (distance > range)
            return;

        Vector3 dirNormalized = dirToPlayer.normalized;              // �P�ʃx�N�g�����擾

        // Forward�����ƃv���C���[�����Ƃ̓��ς��v�Z
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

    public void ToggleFlashlight(bool toggle)
    {
        // �ʏ��ON/OFF�ؑցA�����������Ȃ璆�~
        if (isShuttingDown && toggle)
        {
            CancelShutdown();
        }

        isEnabled = toggle;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isEnabled;
            flashlightLight.intensity = isEnabled ? intensity : 0f; // ON�Ȃ���̋�����ݒ�AOFF�Ȃ�0�ɂ���
        }
        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} flashlight is now {(isEnabled ? "enabled" : "disabled")}.");
        GetComponent<AudioSource>()?.Play(); // ON/OFF���ɉ���炷�iAudioSource������΁j
    }
    /// <summary>
    /// �����I�ɉ����d����1.5�b�����ď����i�o�b�e���[�؂ꎞ�Ȃǁj
    /// </summary>
    public void ForceShutdown()
    {
        if (!isEnabled) return;
        if (isShuttingDown || flashlightLight == null) return;

        isShuttingDown = true;
        isEnabled = false;

        float originalIntensity = flashlightLight.intensity;

        // ������Tween������Ύ~�߂�
        shutdownTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // �_��2��i1�b�j
        seq.Append(flashlightLight.DOIntensity(0, 0.25f));
        seq.Append(flashlightLight.DOIntensity(originalIntensity, 0.25f));
        seq.Append(flashlightLight.DOIntensity(0, 0.25f));
        seq.Append(flashlightLight.DOIntensity(originalIntensity, 0.25f));

        // 0.5�b�ŏ���
        seq.Append(flashlightLight.DOIntensity(0, 0.5f));

        // �������Ƀ��C�g�𖳌���
        seq.OnComplete(() =>
        {
            flashlightLight.enabled = false;
            isEnabled = false; // �����d���𖳌���
            shutdownTween = null;
            isShuttingDown = false;
            if (isDebug) Debug.Log($"[FLASHLIGHT] {name} forced shutdown complete.");
        });

        shutdownTween = seq;

        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} forced shutdown started.");
    }

    /// <summary>
    /// ������������ON�w�߂������Ƃ��ATween���L�����Z�����čē_��
    /// </summary>
    public void CancelShutdown()
    {
        if (!isShuttingDown || flashlightLight == null) return;

        shutdownTween?.Kill();
        flashlightLight.intensity = intensity; // ���邳���ő�ɖ߂��i�K�v�ɉ����ďC���j
        flashlightLight.enabled = true;
        isEnabled = true;
        isShuttingDown = false;

        if (isDebug) Debug.Log($"[FLASHLIGHT] {name} shutdown canceled and flashlight re-enabled.");
    }
}