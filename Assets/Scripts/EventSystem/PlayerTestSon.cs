using UnityEngine;
using DG.Tweening;

public class PlayerTestSon : MonoBehaviour
{
    [Header("�v���C���[�̊�{�ݒ�")]

    [Header("�̗͐ݒ�")]
    private float currentHP =100f;
    private float originMaxHP = 100f; // �����ő�HP
    public float maxHP = 100f;
    public float HPRecoverCooldown = 1f; // HP�񕜂̃N�[���_�E������
    private float lastDamageTime = 0f; // �Ō�Ƀ_���[�W���󂯂�����
    public float HPRecoverAmount = 1f; // HP�񕜗�
    public GameObject damageEffect; // �_���[�W�G�t�F�N�g�̎Q��
    public float damageEffectDuration = 0.5f; // �_���[�W�G�t�F�N�g�̎�������
    private float damageEffectTimer = 0f; // �_���[�W�G�t�F�N�g�̃^�C�}�[
    public GameObject healEffect; // �񕜃G�t�F�N�g�̎Q��

    [Header("�d�r�ݒ�")]
    private float chargeDamping = 1f; // �[�d�̌�����
    public float chargeDampingAmount = 1f; // �[�d�̌����ʂ̑�����
    private float currentBattery = 10f; // �����d�r�c��
    public Flashlight flashlight; // �t���b�V�����C�g�̎Q��
    public bool isFlashlightOn = true; // �t���b�V�����C�g�̏��

    private float bulbCooldown = 5f; // �����d���N�[���_�E������

    [Header("�v���C���[�̐ݒ�")]
    public bool isTest = false;
    public PlayerData playerData = new PlayerData();
    public Material playerMaterial;
    private Material usingMaterial;
    public Renderer playerRenderer;
    

    public bool isWinner = false; // �����t���O


    public void SetIndex(int index)
    {
        playerData.playerIndex = index;
    }
    public void SetPlayerData(PlayerData data)
    {
        playerData = data;

        usingMaterial = new Material(playerMaterial);
        usingMaterial.SetColor("_MainColor", playerData.playerColor);
        usingMaterial.SetColor("_DissolveColor", playerData.playerColor);
        if (playerRenderer != null && playerRenderer.material != null)
        {
            playerRenderer.material = usingMaterial;
        }
        else
        {
            Debug.LogWarning("PlayerRenderer or Material is not set.");
        }

        maxHP = playerData.maxHP;
        currentHP = maxHP; // ����HP���ő�HP�ɐݒ�
        originMaxHP = playerData.maxHP; // �����ő�HP��ۑ�
        currentBattery = playerData.battery; // �����d�r�c�ʂ�ݒ�
        bulbCooldown = playerData.bulbCooldown; // �����d���N�[���_�E�����Ԃ�ݒ�
    }
    private void Start()
    {
        flashlight.ToggleFlashlight(isFlashlightOn); // �t���b�V�����C�g�̏�Ԃ�ݒ�
        if (isTest)
        {
            GameEvents.PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
        }
        damageEffect.SetActive(false); // ������Ԃł̓_���[�W�G�t�F�N�g���\���ɂ���
        healEffect.SetActive(false); // ������Ԃł͉񕜃G�t�F�N�g���\���ɂ���
    }
    private void Update()
    {
        if(isWinner) return; // �����҂͍X�V�������s��Ȃ�
        if (lastDamageTime > HPRecoverCooldown)
        {
            if (currentHP < maxHP)
            {
                if (healEffect != null && !healEffect.activeSelf)
                {
                    healEffect.SetActive(true); // �񕜃G�t�F�N�g��\��
                }
                float newHP = currentHP + HPRecoverAmount * Time.deltaTime;
                currentHP = Mathf.Min(newHP, maxHP); // �ő�HP�𒴂��Ȃ��悤�ɂ���
                GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false)); // HP�X�V�C�x���g�𑗐M
            }
            else if (healEffect != null && healEffect.activeSelf)
            {
                healEffect.SetActive(false); // �񕜃G�t�F�N�g���\��
            }
        }
        else
        {
            lastDamageTime += Time.deltaTime; // �N�[���_�E�����Ԃ��X�V
        }
        if(damageEffectTimer > 0f)
        {
            damageEffectTimer -= Time.deltaTime; // �_���[�W�G�t�F�N�g�̃^�C�}�[���X�V
            if (damageEffectTimer <= 0f)
            {
                // �_���[�W�G�t�F�N�g�̃^�C�}�[��0�ɂȂ�����G�t�F�N�g���\���ɂ���
                if (damageEffect != null)
                {
                    damageEffect.SetActive(false);
                }
            }
        }
        if(chargeDamping > 1f)
        {
            chargeDamping -= chargeDampingAmount * Time.deltaTime; // �[�d�̌���
            chargeDamping = Mathf.Max(1f, chargeDamping); // 1f�����ɂȂ�Ȃ��悤�ɂ���
        }
        if(bulbCooldown > 0)
        {
            bulbCooldown -= Time.deltaTime; // �d���N�[���_�E�����Ԃ��X�V
            bulbCooldown = Mathf.Max(0, bulbCooldown); // ���̒l�ɂȂ�Ȃ��悤�ɂ���
            if(bulbCooldown <= 0f)
            {
                // �d���N�[���_�E�����I��������C�x���g�𑗐M
                GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 1); // 1 = �d���g�p�\
            }
        }
        if(isFlashlightOn && flashlight != null)
        {
            // �t���b�V�����C�g���I���̏ꍇ�A�d�r�c�ʂ�����������
            currentBattery -= Time.deltaTime; // �d�r�c�ʂ�����
            currentBattery = Mathf.Max(0, currentBattery); // ���̒l�ɂȂ�Ȃ��悤�ɂ���
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false); // �d�r�X�V�C�x���g�𑗐M
            if(currentBattery <= 0f)
            {
                // �d�r���Ȃ��Ȃ����ꍇ�A�t���b�V�����C�g���I�t�ɂ���
                flashlight.ForceShutdown();
                isFlashlightOn = false;
            }
        }
    }

    public void Die()
    {
        if(isWinner) return; // �����҂͎��S���Ȃ�
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(0, 0, true));
        GameEvents.PlayerEvents.OnPlayerDied?.Invoke(this.gameObject);
        Destroy(gameObject);
    }
    /// <summary>
    /// �����Ȃǂɂ��_���[�W���󂯂�
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isWinner) return; // �����҂̓_���[�W���󂯂Ȃ�
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // ���݂�HP�ɉ����čő�HP�𒲐�

        lastDamageTime = 0f; // �_���[�W���󂯂��̂ŃN�[���_�E�������Z�b�g
        damageEffectTimer = damageEffectDuration; // �_���[�W�G�t�F�N�g�̃^�C�}�[�����Z�b�g
        if (damageEffect != null)
        {
            damageEffect.SetActive(true); // �_���[�W�G�t�F�N�g��\��
        }

        // UI�X�V�C�x���g�𑗐M
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, true));

        // �f�o�b�O���O
        Debug.Log($"[Player{playerData.playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

        // ���S����
        if (currentHP <= 0f)
        {
            Die();
        }
    }


    public void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        flashlight.ToggleFlashlight(isFlashlightOn);
    }

    public void BatteryCharge(float amount = 1f)
    {
        if(currentBattery >= playerData.battery) return; // �d�r�����^���Ȃ牽�����Ȃ�
        currentBattery += (amount/chargeDamping);
        chargeDamping += chargeDampingAmount; // �[�d�̌����ʂ𑝉�
        currentBattery = Mathf.Min(currentBattery, playerData.battery); // �ő�d�r�c�ʂ𒴂��Ȃ��悤�ɂ���
        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // �d�r�X�V�C�x���g�𑗐M
    }

    public bool ThrowBulb()
    {
        if (bulbCooldown > 0) return false; // �d�����N�[���_�E�����Ȃ瓊�����Ȃ�
        bulbCooldown = playerData.bulbCooldown; // �N�[���_�E�����Ԃ����Z�b�g
        GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 0); // 0 = �d���Ȃ�
        return true; // �d���𓊂��邱�Ƃ��ł���
    }
    public void SetWinner()
    {
        isWinner = true;
        GameEvents.PlayerEvents.OnWinnerSet?.Invoke(playerData);
    }

}

