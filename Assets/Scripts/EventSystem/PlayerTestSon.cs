using UnityEngine;
using DG.Tweening;

public class PlayerTestSon : MonoBehaviour
{
    [Header("プレイヤーの基本設定")]

    [Header("体力設定")]
    private float currentHP =100f;
    private float originMaxHP = 100f; // 初期最大HP
    public float maxHP = 100f;
    public float HPRecoverCooldown = 1f; // HP回復のクールダウン時間
    private float lastDamageTime = 0f; // 最後にダメージを受けた時間
    public float HPRecoverAmount = 1f; // HP回復量
    public GameObject damageEffect; // ダメージエフェクトの参照
    public float damageEffectDuration = 0.5f; // ダメージエフェクトの持続時間
    private float damageEffectTimer = 0f; // ダメージエフェクトのタイマー
    public GameObject healEffect; // 回復エフェクトの参照

    [Header("電池設定")]
    private float chargeDamping = 1f; // 充電の減少量
    public float chargeDampingMax = 6f; // 充電の減少量の最大値
    public float chargeDampingAmount = 1f; // 充電の減少量の増加量
    public float chargeDampingRecoverAmount = 1f; // 充電の減少量の回復量
    private float currentBattery = 10f; // 初期電池残量
    public Flashlight flashlight; // フラッシュライトの参照
    public bool isFlashlightOn = false; // フラッシュライトの状態

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
        currentHP = maxHP; // 初期HPを最大HPに設定
        originMaxHP = playerData.maxHP; // 初期最大HPを保存
        currentBattery = playerData.battery; // 初期電池残量を設定
        bulbCooldown = 0f; // 初期電球クールダウン時間を設定
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
    }
    private void Update()
    {
        if(isWinner) return; // 勝利者は更新処理を行わない
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
                GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false)); // HP更新イベントを送信
            }
            else if (healEffect != null && healEffect.activeSelf)
            {
                healEffect.SetActive(false); // 回復エフェクトを非表示
            }
        }
        else
        {
            lastDamageTime += Time.deltaTime; // クールダウン時間を更新
        }
        if(damageEffectTimer > 0f)
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
        if(chargeDamping > 1f)
        {
            chargeDamping -= Time.deltaTime * chargeDampingRecoverAmount; // 充電の減少量を回復
            chargeDamping = Mathf.Max(1f, chargeDamping); // 1f未満にならないようにする
        }
        if(bulbCooldown > 0)
        {
            bulbCooldown -= Time.deltaTime; // 電球クールダウン時間を更新
            bulbCooldown = Mathf.Max(0, bulbCooldown); // 負の値にならないようにする
            if(bulbCooldown <= 0f)
            {
                // 電球クールダウンが終了したらイベントを送信
                GameEvents.PlayerEvents.OnBulbStateChanged?.Invoke(playerData.playerIndex, 1); // 1 = 電球使用可能
                
            }
        }
        if(isFlashlightOn && flashlight != null)
        {
            // フラッシュライトがオンの場合、電池残量を減少させる
            currentBattery -= Time.deltaTime; // 電池残量を減少
            currentBattery = Mathf.Max(0, currentBattery); // 負の値にならないようにする
            GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, false); // 電池更新イベントを送信
            if(currentBattery <= 0f)
            {
                // 電池がなくなった場合、フラッシュライトをオフにする
                flashlight.ForceShutdown();
                isFlashlightOn = false;
            }
        }
    }

    public void Die()
    {
        if (isWinner||isDying) return; // 勝利者または既に死亡中のプレイヤーは死亡処理を行わない
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
    }
    /// <summary>
    /// 光源などによるダメージを受ける
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isWinner || isDying) return; // 勝利者または死亡中のプレイヤーはダメージを受けない
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // 現在のHPに応じて最大HPを調整

        lastDamageTime = 0f; // ダメージを受けたのでクールダウンをリセット
        damageEffectTimer = damageEffectDuration; // ダメージエフェクトのタイマーをリセット
        if (damageEffect != null)
        {
            damageEffect.SetActive(true); // ダメージエフェクトを表示
        }
        if(healEffect != null)
        {
            healEffect.SetActive(false); // 回復エフェクトを非表示
        }

        // UI更新イベントを送信
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, true));

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
        currentBattery += (amount/chargeDamping);
        chargeDamping += chargeDampingAmount; // 充電の減少量を増加
        chargeDamping = Mathf.Min(chargeDamping, chargeDampingMax); // 最大充電減少量を超えないようにする
        currentBattery = Mathf.Min(currentBattery, playerData.battery); // 最大電池残量を超えないようにする
        GameEvents.PlayerEvents.OnBatteryChanged?.Invoke(playerData.playerIndex, currentBattery, true); // 電池更新イベントを送信
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

}

