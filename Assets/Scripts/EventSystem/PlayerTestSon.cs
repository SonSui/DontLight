using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public class PlayerTestSon : MonoBehaviour
{
    [Header("プレイヤーの基本設定")]

    [Header("体力設定")]
    private float currentHP = 100f;
    private float originMaxHP = 100f; // 初期最大HP
    public float maxHP = 100f;
    public float HPRecoverCooldown = 1f; // HP回復のクールダウン時間
    public int playerDisplayHP = 6;
    private float HPSlotAmount = 10f;
    private List<bool> HpDcreaseEff = new List<bool>();
    private float lastDamageTime = 0f; // 最後にダメージを受けた時間
    public float HPRecoverAmount = 1f; // HP回復量
    public GameObject damageEffect; // ダメージエフェクトの参照
    public float damageEffectDuration = 0.5f; // ダメージエフェクトの持続時間
    private float damageEffectTimer = 0f; // ダメージエフェクトのタイマー
    public GameObject healEffect; // 回復エフェクトの参照

    public GameObject damageEffectExplo;
    public float damageEffectExploDuration = 0.6f;
    private float damageEffectExploTimer = 0f;

    [Header("電池設定")]
    private float chargeDamping = 1f; // 充電の減少量
    public float chargeDampingMax = 6f; // 充電の減少量の最大値
    public float chargeDampingAmount = 1f; // 充電の減少量の増加量
    public float chargeDampingRecoverAmount = 1f; // 充電の減少量の回復量
    private float currentBattery = 10f; // 初期電池残量
    public Flashlight flashlight; // フラッシュライトの参照
    public bool isFlashlightOn = false; // フラッシュライトの状態
    public GameObject batteryFlashEffect; // 電池残量が少ないときのエフェクト
    public float CurrentBattery => currentBattery;
    [SerializeField]private BatteryWorldBar_CenterPivot batteryWordUI;


    [Header("電球設定")]
    private float bulbCooldown = 5f; // 初期電球クールダウン時間

    [Header("プレイヤーの設定")]
    public bool isTest = false;
    public PlayerData playerData = new PlayerData();
    public Material playerMaterial;
    private Material usingMaterial;
    public Renderer playerRenderer;


    [Header("アニメーション設定")]
    public float deathDelay = 0.3f; // 死亡アニメーションの遅延時間
    public float reductionTime = 0.5f; // 死亡アニメーションの縮小時間
    public float reductionScale = 0.05f; // 死亡アニメーションの縮小率
    public Animator animatorMesh;
    public AudioSource audioSource; // プレイヤーの音声ソース
    private Sequence currentSequence;
    private bool IsPlaying => currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying();


    public bool isWinner = false; // 勝利フラグ
    public bool isDying = false; // 死亡中フラグ

    private Sequence colorSequence;
    private Color originalColor = Color.white;
    [Header("ダメージ色演出")]
    public float flashOneWayDuration = 0.15f;
    public Ease flashEase = Ease.InOutSine;

    private Gamepad gamepad;
    [Header("被ダメ時の振動")]
    [SerializeField] private float lightRumbleLow = 0.10f;   // 光源内での弱振動(低周波)
    [SerializeField] private float lightRumbleHigh = 0.20f;  // 光源内での弱振動(高周波)
    [SerializeField] private float strongRumbleLow = 0.40f;  // エフェクト演出時の強振動(低周波)
    [SerializeField] private float strongRumbleHigh = 0.80f; // エフェクト演出時の強振動(高周波)
    [SerializeField] private float strongRumbleDuration = 0.25f; // 強振動の長さ(秒)
    [SerializeField] private float rumbleStopDelay = 0.15f;       // 「被弾が来なくなってから」弱振動を止める猶予

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
        currentHP = maxHP; // 初期HPを最大HPに設定
        originMaxHP = playerData.maxHP; // 初期最大HPを保存
        currentBattery = playerData.battery; // 初期電池残量を設定
        bulbCooldown = 0f; // 初期電球クールダウン時間を設定
        HPSlotAmount = maxHP / playerDisplayHP; // 1スロットあたりのHPを計算
        HpDcreaseEff.Clear();
        for (int i = 0; i < playerDisplayHP; i++)
        {
            HpDcreaseEff.Add(false); // 初期化
        }
        InitGamepadFromPlayerData();
    }
    private void Start()
    {
        flashlight.ToggleFlashlight(isFlashlightOn); // フラッシュライトの状態を設定
        if (isTest)
        {
            GameEvents.PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
        }
        damageEffect.SetActive(false); // 初期状態ではダメージエフェクトを非表示にする
        healEffect.SetActive(false); // 初期状態では回復エフェクトを非表示にする

        SetPlayerData(playerData); // プレイヤーデータを設定
    }
    private void OnDisable()
    {
        // シーン遷移などで無効化されたときも止めておくと安全
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            gamepad.ResetHaptics();
        }
    }
    private void Update()
    {
        if (isWinner) return; // 勝利者は更新処理を行わない
        if (lastDamageTime > HPRecoverCooldown && !isDying)
        {
            if (currentHP < maxHP)
            {
                if (healEffect != null && !healEffect.activeSelf)
                {
                    healEffect.SetActive(true); // 回復エフェクトを表示
                }
                float newHP = currentHP + HPRecoverAmount * Time.deltaTime;
                currentHP = Mathf.Min(newHP, maxHP); // 最大HPを超えないようにする

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
                healEffect.SetActive(false); // 回復エフェクトを非表示
                GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false));
            }
        }
        else
        {
            lastDamageTime += Time.deltaTime; // クールダウン時間を更新
        }
        if (damageEffectTimer > 0f)
        {
            damageEffectTimer -= Time.deltaTime; // ダメージエフェクトのタイマーを更新
            if (damageEffectTimer <= 0f)
            {
                // ダメージエフェクトのタイマーが0になったらエフェクトを非表示にする
                if (damageEffect != null)
                {
                    damageEffect.SetActive(false);
                }
            }
        }

        if (damageEffectExploTimer > 0f)
        {
            damageEffectExploTimer -= Time.deltaTime; // ダメージエフェクトのタイマーを更新
            if (damageEffectExploTimer <= 0f)
            {
                // ダメージエフェクトのタイマーが0になったらエフェクトを非表示にする
                if (damageEffectExplo != null)
                {
                    damageEffectExplo.SetActive(false);
                }
            }
        }
        if (chargeDamping > 1f)
        {
            chargeDamping -= Time.deltaTime * chargeDampingRecoverAmount; // 充電の減少量を回復
            chargeDamping = Mathf.Max(1f, chargeDamping); // 1f未満にならないようにする
        }
        if (bulbCooldown > 0)
        {
            bulbCooldown -= Time.deltaTime; // 電球クールダウン時間を更新
            bulbCooldown = Mathf.Max(0, bulbCooldown); // 負の値にならないようにする
            if (bulbCooldown <= 0f)
            {
                // 電球クールダウンが終了したらイベントを送信
                GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 1); // 1 = 電球使用可能

            }
        }
        if (isFlashlightOn && flashlight != null)
        {
            // フラッシュライトがオンの場合、電池残量を減少させる
            currentBattery -= Time.deltaTime; // 電池残量を減少
            currentBattery = Mathf.Max(0, currentBattery); // 負の値にならないようにする
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false); // 電池更新イベントを送信
            batteryWordUI?.UpdateBatteryImmediately(currentBattery); // 世界UI更新
            if (currentBattery <= 0f)
            {
                // 電池がなくなった場合、フラッシュライトをオフにする
                flashlight.ForceShutdown();
                isFlashlightOn = false;
                batteryFlashEffect.SetActive(true); // 電池残量が少ないエフェクトを表示
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
        if (isWinner || isDying) return; // 勝利者または既に死亡中のプレイヤーは死亡処理を行わない
        if (IsPlaying)
        {
            Debug.Log("Already Dying, skipping.");
            return;
        }

        isDying = true; // 死亡中フラグを立てる

        damageEffect.SetActive(false); // ダメージエフェクトを非表示にする
        healEffect.SetActive(false); // 回復エフェクトを非表示にする

        animatorMesh.SetTrigger("Dead"); // 死亡アニメーションを再生
        animatorMesh.SetBool("IsAlive", false); // 死亡中のフラグを立てる

        audioSource.Play(); // 死亡音を再生

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
    /// 光源などによるダメージを受ける
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isWinner || isDying) return; // 勝利者または死亡中のプレイヤーはダメージを受けない
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP - currentHP) / 2 + currentHP, maxHP); // 現在のHPに応じて最大HPを調整

        if (!isLightRumbling) StartLightRumble();
        noDamageTimer = 0f;

        lastDamageTime = 0f; // ダメージを受けたのでクールダウンをリセット
        damageEffectTimer = damageEffectDuration; // ダメージエフェクトのタイマーをリセット
        if (damageEffect != null)
        {
            damageEffect.SetActive(true); // ダメージエフェクトを表示
        }
        if (healEffect != null)
        {
            healEffect.SetActive(false); // 回復エフェクトを非表示
        }

        // UI更新イベントを送信
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


        // デバッグログ
        Debug.Log($"[Player{playerData.playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

        // 死亡処理
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
        if (isDying) return false; // 死亡中のプレイヤーは充電できない
        if (currentBattery >= playerData.battery) return false; // 電池が満タンなら何もしない
        currentBattery += (amount / chargeDamping);
        chargeDamping += chargeDampingAmount; // 充電の減少量を増加
        chargeDamping = Mathf.Min(chargeDamping, chargeDampingMax); // 最大充電減少量を超えないようにする
        currentBattery = Mathf.Min(currentBattery, playerData.battery); // 最大電池残量を超えないようにする
        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // 電池更新イベントを送信
        batteryWordUI?.UpdateBattery(currentBattery, true, playerData.battery); // 世界UI更新
        return true; // 充電に成功した場合はtrueを返す
    }

    public bool ThrowBulb()
    {
        if (isDying) return false; // 死亡中のプレイヤーは電球を投げられない
        if (bulbCooldown > 0) return false; // 電球がクールダウン中なら投げられない
        bulbCooldown = playerData.bulbCooldown; // クールダウン時間をリセット
        GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 0); // 0 = 電球ない

        animatorMesh.SetTrigger("Attack"); // 電球を投げるアニメーションを再生
        return true; // 電球を投げることができた
    }

    public bool CanThrowBulb()
    {
        return !isDying && bulbCooldown <= 0; // 死亡中でなく、かつ電球がクールダウン中でない場合に投げられる
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
            damageEffectExploTimer = damageEffectExploDuration; // ダメージエフェクトのタイマーをリセット
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
        // PlayerData.devices 優先
        if (playerData != null)
        {
            if (playerData.devices != null)
            {
                foreach (var d in playerData.devices)
                {
                    if (d is Gamepad gp) { gamepad = gp; break; }
                }
            }
            // PlayerInput.devices も試す
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

    /// <summary>強振動を一定時間。終わったら弱振動に自然復帰（必要なら）</summary>
    private System.Collections.IEnumerator StrongRumblePulse(float low, float high, float duration)
    {
        if (gamepad == null) yield break;

        // 今の状態に関わらず強振動へ
        gamepad.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);

        // ダメージ中なら弱振動を継続、そうでなければ停止
        if (isLightRumbling)
            gamepad.SetMotorSpeeds(lightRumbleLow, lightRumbleHigh);
        else
            gamepad.SetMotorSpeeds(0f, 0f);
    }
}

