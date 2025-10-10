using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public class PlayerTestSon : MonoBehaviour
{
    [Header("�v���C���[�̊�{�ݒ�")]

    [Header("�̗͐ݒ�")]
    private float currentHP = 100f;
    private float originMaxHP = 100f; // �����ő�HP
    public float maxHP = 100f;
    public float HPRecoverCooldown = 1f; // HP�񕜂̃N�[���_�E������
    public int playerDisplayHP = 6;
    private float HPSlotAmount = 10f;
    private List<bool> HpDcreaseEff = new List<bool>();
    private float lastDamageTime = 0f; // �Ō�Ƀ_���[�W���󂯂�����
    public float HPRecoverAmount = 1f; // HP�񕜗�
    public GameObject damageEffect; // �_���[�W�G�t�F�N�g�̎Q��
    public float damageEffectDuration = 0.5f; // �_���[�W�G�t�F�N�g�̎�������
    private float damageEffectTimer = 0f; // �_���[�W�G�t�F�N�g�̃^�C�}�[
    public GameObject healEffect; // �񕜃G�t�F�N�g�̎Q��

    public GameObject damageEffectExplo;
    public float damageEffectExploDuration = 0.6f;
    private float damageEffectExploTimer = 0f;

    [Header("�d�r�ݒ�")]
    private float chargeDamping = 1f; // �[�d�̌�����
    public float chargeDampingMax = 6f; // �[�d�̌����ʂ̍ő�l
    public float chargeDampingAmount = 1f; // �[�d�̌����ʂ̑�����
    public float chargeDampingRecoverAmount = 1f; // �[�d�̌����ʂ̉񕜗�
    private float currentBattery = 10f; // �����d�r�c��
    public Flashlight flashlight; // �t���b�V�����C�g�̎Q��
    public bool isFlashlightOn = false; // �t���b�V�����C�g�̏��
    public GameObject batteryFlashEffect; // �d�r�c�ʂ����Ȃ��Ƃ��̃G�t�F�N�g
    public float CurrentBattery => currentBattery;
    [SerializeField]private BatteryWorldBar_CenterPivot batteryWordUI;


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

    private Sequence colorSequence;
    private Color originalColor = Color.white;
    [Header("�_���[�W�F���o")]
    public float flashOneWayDuration = 0.15f;
    public Ease flashEase = Ease.InOutSine;

    private Gamepad gamepad;
    [Header("��_�����̐U��")]
    [SerializeField] private float lightRumbleLow = 0.10f;   // �������ł̎�U��(����g)
    [SerializeField] private float lightRumbleHigh = 0.20f;  // �������ł̎�U��(�����g)
    [SerializeField] private float strongRumbleLow = 0.40f;  // �G�t�F�N�g���o���̋��U��(����g)
    [SerializeField] private float strongRumbleHigh = 0.80f; // �G�t�F�N�g���o���̋��U��(�����g)
    [SerializeField] private float strongRumbleDuration = 0.25f; // ���U���̒���(�b)
    [SerializeField] private float rumbleStopDelay = 0.15f;       // �u��e�����Ȃ��Ȃ��Ă���v��U�����~�߂�P�\

    private bool isLightRumbling = false;
    private float noDamageTimer = 0f;

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

        originalColor = playerData.playerColor;
        maxHP = playerData.maxHP;
        currentHP = maxHP; // ����HP���ő�HP�ɐݒ�
        originMaxHP = playerData.maxHP; // �����ő�HP��ۑ�
        currentBattery = playerData.battery; // �����d�r�c�ʂ�ݒ�
        bulbCooldown = 0f; // �����d���N�[���_�E�����Ԃ�ݒ�
        HPSlotAmount = maxHP / playerDisplayHP; // 1�X���b�g�������HP���v�Z
        HpDcreaseEff.Clear();
        for (int i = 0; i < playerDisplayHP; i++)
        {
            HpDcreaseEff.Add(false); // ������
        }
        InitGamepadFromPlayerData();
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

        SetPlayerData(playerData); // �v���C���[�f�[�^��ݒ�
    }
    private void OnDisable()
    {
        // �V�[���J�ڂȂǂŖ��������ꂽ�Ƃ����~�߂Ă����ƈ��S
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            gamepad.ResetHaptics();
        }
    }
    private void Update()
    {
        if (isWinner) return; // �����҂͍X�V�������s��Ȃ�
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

                for (int i = 0; i < playerDisplayHP; i++)
                {
                    if (currentHP >= HPSlotAmount * (i + 1) && HpDcreaseEff[i])
                    {
                        HpDcreaseEff[i] = false;
                        Debug.Log($"[Player{playerData.playerIndex}] HP recovered to {currentHP}. Slot {i} restored.");
                        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false));
                        if(i >= playerDisplayHP/3) ResetMaterialColor();
                    }
                }
                
            }
            else if (healEffect != null && healEffect.activeSelf)
            {
                healEffect.SetActive(false); // �񕜃G�t�F�N�g���\��
                GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false));
            }
        }
        else
        {
            lastDamageTime += Time.deltaTime; // �N�[���_�E�����Ԃ��X�V
        }
        if (damageEffectTimer > 0f)
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

        if (damageEffectExploTimer > 0f)
        {
            damageEffectExploTimer -= Time.deltaTime; // �_���[�W�G�t�F�N�g�̃^�C�}�[���X�V
            if (damageEffectExploTimer <= 0f)
            {
                // �_���[�W�G�t�F�N�g�̃^�C�}�[��0�ɂȂ�����G�t�F�N�g���\���ɂ���
                if (damageEffectExplo != null)
                {
                    damageEffectExplo.SetActive(false);
                }
            }
        }
        if (chargeDamping > 1f)
        {
            chargeDamping -= Time.deltaTime * chargeDampingRecoverAmount; // �[�d�̌����ʂ���
            chargeDamping = Mathf.Max(1f, chargeDamping); // 1f�����ɂȂ�Ȃ��悤�ɂ���
        }
        if (bulbCooldown > 0)
        {
            bulbCooldown -= Time.deltaTime; // �d���N�[���_�E�����Ԃ��X�V
            bulbCooldown = Mathf.Max(0, bulbCooldown); // ���̒l�ɂȂ�Ȃ��悤�ɂ���
            if (bulbCooldown <= 0f)
            {
                // �d���N�[���_�E�����I��������C�x���g�𑗐M
                GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 1); // 1 = �d���g�p�\

            }
        }
        if (isFlashlightOn && flashlight != null)
        {
            // �t���b�V�����C�g���I���̏ꍇ�A�d�r�c�ʂ�����������
            currentBattery -= Time.deltaTime; // �d�r�c�ʂ�����
            currentBattery = Mathf.Max(0, currentBattery); // ���̒l�ɂȂ�Ȃ��悤�ɂ���
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false); // �d�r�X�V�C�x���g�𑗐M
            batteryWordUI?.UpdateBatteryImmediately(currentBattery); // ���EUI�X�V
            if (currentBattery <= 0f)
            {
                // �d�r���Ȃ��Ȃ����ꍇ�A�t���b�V�����C�g���I�t�ɂ���
                flashlight.ForceShutdown();
                isFlashlightOn = false;
                batteryFlashEffect.SetActive(true); // �d�r�c�ʂ����Ȃ��G�t�F�N�g��\��
            }
        }
        noDamageTimer += Time.deltaTime;
        if (isLightRumbling && noDamageTimer > rumbleStopDelay)
        {
            StopLightRumble();
        }
    }

    public void Die()
    {
        if (isWinner || isDying) return; // �����҂܂��͊��Ɏ��S���̃v���C���[�͎��S�������s��Ȃ�
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
        if (colorSequence != null && colorSequence.IsActive())
            colorSequence.Kill();

        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            gamepad.ResetHaptics();
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
        maxHP = Mathf.Min((originMaxHP - currentHP) / 2 + currentHP, maxHP); // ���݂�HP�ɉ����čő�HP�𒲐�

        if (!isLightRumbling) StartLightRumble();
        noDamageTimer = 0f;

        lastDamageTime = 0f; // �_���[�W���󂯂��̂ŃN�[���_�E�������Z�b�g
        damageEffectTimer = damageEffectDuration; // �_���[�W�G�t�F�N�g�̃^�C�}�[�����Z�b�g
        if (damageEffect != null)
        {
            damageEffect.SetActive(true); // �_���[�W�G�t�F�N�g��\��
        }
        if (healEffect != null)
        {
            healEffect.SetActive(false); // �񕜃G�t�F�N�g���\��
        }

        // UI�X�V�C�x���g�𑗐M
        for (int i = 0; i < playerDisplayHP; i++)
        {
            float slotThreshold = HPSlotAmount * (i + 1);
            if (currentHP < slotThreshold && !HpDcreaseEff[i])
            {
                HpDcreaseEff[i] = true;
                GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, true));
                if(i>playerDisplayHP/3)SetMaterialRedForSec(0.5f);
                else SetMaterialRedForSec(-1);

                if (gamepad != null)
                    StartCoroutine(StrongRumblePulse(strongRumbleLow, strongRumbleHigh, strongRumbleDuration));
            }
        }


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
        currentBattery += (amount / chargeDamping);
        chargeDamping += chargeDampingAmount; // �[�d�̌����ʂ𑝉�
        chargeDamping = Mathf.Min(chargeDamping, chargeDampingMax); // �ő�[�d�����ʂ𒴂��Ȃ��悤�ɂ���
        currentBattery = Mathf.Min(currentBattery, playerData.battery); // �ő�d�r�c�ʂ𒴂��Ȃ��悤�ɂ���
        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // �d�r�X�V�C�x���g�𑗐M
        batteryWordUI?.UpdateBattery(currentBattery, true, playerData.battery); // ���EUI�X�V
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

    private void SetMaterialRedForSec(float t)
    {
        if (usingMaterial == null) return;

        if (colorSequence != null && colorSequence.IsActive())
            colorSequence.Kill();

        if(damageEffectExplo != null)
        {
            damageEffectExplo.SetActive(true);
            damageEffectExploTimer = damageEffectExploDuration; // �_���[�W�G�t�F�N�g�̃^�C�}�[�����Z�b�g
        }

        Color hurtRed = Color.red;

        if (t > 0f)
        {
            float oneWay = Mathf.Max(0.01f, t * 0.5f);
            colorSequence = DOTween.Sequence();
            colorSequence
                .Join(usingMaterial.DOColor(hurtRed, "_MainColor", oneWay).SetEase(flashEase))
                .Join(usingMaterial.DOColor(hurtRed, "_DissolveColor", oneWay).SetEase(flashEase))
                .AppendInterval(0f)
                .Append(usingMaterial.DOColor(originalColor, "_MainColor", oneWay).SetEase(flashEase))
                .Join(usingMaterial.DOColor(originalColor, "_DissolveColor", oneWay).SetEase(flashEase))
                .OnComplete(() => colorSequence = null);
        }
        else
        {

            usingMaterial.SetColor("_MainColor", playerData.playerColor);
            usingMaterial.SetColor("_DissolveColor", playerData.playerColor);
            colorSequence = DOTween.Sequence();
            colorSequence
                .Join(usingMaterial.DOColor(hurtRed, "_MainColor", flashOneWayDuration).SetEase(flashEase))
                .Join(usingMaterial.DOColor(hurtRed, "_DissolveColor", flashOneWayDuration).SetEase(flashEase))
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    private void ResetMaterialColor()
    {
        if (usingMaterial == null) return;

        if (colorSequence != null && colorSequence.IsActive())
            colorSequence.Kill();

        float back = Mathf.Max(0.05f, flashOneWayDuration * 0.75f);
        usingMaterial.DOColor(originalColor, "_MainColor", back).SetEase(flashEase);
        usingMaterial.DOColor(originalColor, "_DissolveColor", back).SetEase(flashEase);
    }
    private void InitGamepadFromPlayerData()
    {
        // PlayerData.devices �D��
        if (playerData != null)
        {
            if (playerData.devices != null)
            {
                foreach (var d in playerData.devices)
                {
                    if (d is Gamepad gp) { gamepad = gp; break; }
                }
            }
            // PlayerInput.devices ������
            if (gamepad == null && playerData.input != null && playerData.input.devices.Count > 0)
            {
                foreach (var d in playerData.input.devices)
                {
                    if (d is Gamepad gp)
                    {
                        gamepad = gp;
                        break;
                    }
                }
            }
        }
    }

private void StartLightRumble()
    {
        if (gamepad == null) return;
        gamepad.SetMotorSpeeds(lightRumbleLow, lightRumbleHigh);
        isLightRumbling = true;
    }

    private void StopLightRumble()
    {
        if (gamepad == null) return;
        gamepad.SetMotorSpeeds(0f, 0f);
        isLightRumbling = false;
    }

    /// <summary>���U������莞�ԁB�I��������U���Ɏ��R���A�i�K�v�Ȃ�j</summary>
    private System.Collections.IEnumerator StrongRumblePulse(float low, float high, float duration)
    {
        if (gamepad == null) yield break;

        // ���̏�ԂɊւ�炸���U����
        gamepad.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);

        // �_���[�W���Ȃ��U�����p���A�����łȂ���Β�~
        if (isLightRumbling)
            gamepad.SetMotorSpeeds(lightRumbleLow, lightRumbleHigh);
        else
            gamepad.SetMotorSpeeds(0f, 0f);
    }
}

