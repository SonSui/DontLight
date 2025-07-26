using UnityEngine;

public class PlayerTestSon : MonoBehaviour
{
    [Header("プレイヤーの基本設定")]
    
    private float currentHP;
    private float originMaxHP = 100f; // 初期最大HP
    public float maxHP = 100f;
    public bool isTest = false;
    private PlayerData playerData = new PlayerData();
    public Material playerMaterial;
    private Material usingMaterial;
    public Renderer playerRenderer;


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
    }
    private void Start()
    {
        currentHP = playerData.maxHP;
        maxHP = playerData.maxHP;
        originMaxHP = playerData.maxHP;
        if (isTest)
        {
            GameEvents.PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
        }
    }

    public void Die()
    {
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(0, 0, true));
        GameEvents.PlayerEvents.OnPlayerDied?.Invoke(this.gameObject);
        Destroy(gameObject);
    }
    /// <summary>
    /// 光源などによるダメージを受ける
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // 現在のHPに応じて最大HPを調整

        // UI更新イベントを送信
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, true));

        // デバッグログ
        Debug.Log($"[Player{playerData.playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

        // 死亡処理
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// HPを回復
    /// </summary>
    public void Heal(float amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false));
    }
}

