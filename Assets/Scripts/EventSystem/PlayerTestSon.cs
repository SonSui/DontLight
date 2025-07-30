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
    public float chargeDampingMax = 6f; // �[�d�̌����ʂ̍ő�l
    public float chargeDampingAmount = 1f; // �[�d�̌����ʂ̑�����
    public float chargeDampingRecoverAmount = 1f; // �[�d�̌����ʂ̉񕜗�
    private float currentBattery = 10f; // �����d�r�c��
    public Flashlight flashlight; // �t���b�V�����C�g�̎Q��
    public bool isFlashlightOn = false; // �t���b�V�����C�g�̏��

    [Header("�d���ݒ�")]
    private float bulbCooldown = 5f; // �����d���N�[���_�E������

    [Header("�v���C���[�̐ݒ�")]
    public bool isTest = false;
    public PlayerData playerData = new PlayerData();
    public Material playerMaterial;
    private Material usingMaterial;
    public Renderer playerRenderer;
    

    [Header("�A�j���[�V�����ݒ�")]
    public float deathDelay = 0.3f; // ���S�A�j���[�V�����̒x������
    public float reductionTime = 0.5f; // ���S�A�j���[�V�����̏k������
    public float reductionScale = 0.05f; // ���S�A�j���[�V�����̏k����
    public Animator animatorMesh;
    public AudioSource audioSource; // �v���C���[�̉����\�[�X
    private Sequence currentSequence;
    private bool IsPlaying => currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying();


    public bool isWinner = false; // �����t���O
    public bool isDying = false; // ���S���t���O


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
        bulbCooldown = 0f; // �����d���N�[���_�E�����Ԃ�ݒ�
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
        if (lastDamageTime > HPRecoverCooldown && !isDying)
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
            chargeDamping -= Time.deltaTime * chargeDampingRecoverAmount; // �[�d�̌����ʂ���
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
        if (isWinner||isDying) return; // �����҂܂��͊��Ɏ��S���̃v���C���[�͎��S�������s��Ȃ�
        if (IsPlaying)
        {
            Debug.Log("Already Dying, skipping.");
            return;
        }

        isDying = true; // ���S���t���O�𗧂Ă�

        damageEffect.SetActive(false); // �_���[�W�G�t�F�N�g���\���ɂ���
        healEffect.SetActive(false); // �񕜃G�t�F�N�g���\���ɂ���

        animatorMesh.SetTrigger("Dead"); // ���S�A�j���[�V�������Đ�
        animatorMesh.SetBool("IsAlive", false); // ���S���̃t���O�𗧂Ă�

        audioSource.Play(); // ���S�����Đ�

        currentSequence = DOTween.Sequence();

        currentSequence.AppendInterval(deathDelay);
        currentSequence.Append(transform.DOScale(Vector3.one * reductionScale, reductionTime).SetEase(Ease.InBack));
        currentSequence.OnComplete(() =>
        {
            GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(0, 0, true));
            GameEvents.PlayerEvents.OnPlayerDied?.Invoke(this.gameObject);
            Destroy(gameObject);
        });

    }
    private void OnDestroy()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
    }
    /// <summary>
    /// �����Ȃǂɂ��_���[�W���󂯂�
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isWinner || isDying) return; // �����҂܂��͎��S���̃v���C���[�̓_���[�W���󂯂Ȃ�
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // ���݂�HP�ɉ����čő�HP�𒲐�

        lastDamageTime = 0f; // �_���[�W���󂯂��̂ŃN�[���_�E�������Z�b�g
        damageEffectTimer = damageEffectDuration; // �_���[�W�G�t�F�N�g�̃^�C�}�[�����Z�b�g
        if (damageEffect != null)
        {
            damageEffect.SetActive(true); // �_���[�W�G�t�F�N�g��\��
        }
        if(healEffect != null)
        {
            healEffect.SetActive(false); // �񕜃G�t�F�N�g���\��
        }

        // UI�X�V�C�x���g�𑗐M
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, true));

        // �f�o�b�O���O
        Debug.Log($"[Player{playerData.playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

        // ���S����
        if (currentHP <= 0f && !isDying)
        {
            Die();
        }
    }


    public void ToggleFlashlight()
    {
        if (isDying) return;
        isFlashlightOn = !isFlashlightOn;
        flashlight.ToggleFlashlight(isFlashlightOn);
    }

    public bool BatteryCharge(float amount = 1f)
    {
        if (isDying) return false; // ���S���̃v���C���[�͏[�d�ł��Ȃ�
        if (currentBattery >= playerData.battery) return false; // �d�r�����^���Ȃ牽�����Ȃ�
        currentBattery += (amount/chargeDamping);
        chargeDamping += chargeDampingAmount; // �[�d�̌����ʂ𑝉�
        chargeDamping = Mathf.Min(chargeDamping, chargeDampingMax); // �ő�[�d�����ʂ𒴂��Ȃ��悤�ɂ���
        currentBattery = Mathf.Min(currentBattery, playerData.battery); // �ő�d�r�c�ʂ𒴂��Ȃ��悤�ɂ���
        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // �d�r�X�V�C�x���g�𑗐M
        return true; // �[�d�ɐ��������ꍇ��true��Ԃ�
    }

    public bool ThrowBulb()
    {
        if (isDying) return false; // ���S���̃v���C���[�͓d���𓊂����Ȃ�
        if (bulbCooldown > 0) return false; // �d�����N�[���_�E�����Ȃ瓊�����Ȃ�
        bulbCooldown = playerData.bulbCooldown; // �N�[���_�E�����Ԃ����Z�b�g
        GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 0); // 0 = �d���Ȃ�
        
        animatorMesh.SetTrigger("Attack"); // �d���𓊂���A�j���[�V�������Đ�
        return true; // �d���𓊂��邱�Ƃ��ł���
    }

    public bool CanThrowBulb()
    {
        return !isDying && bulbCooldown <= 0; // ���S���łȂ��A���d�����N�[���_�E�����łȂ��ꍇ�ɓ�������
    }
    public void SetWinner()
    {
        isWinner = true;
        GameEvents.PlayerEvents.OnWinnerSet?.Invoke(playerData);
    }

}

