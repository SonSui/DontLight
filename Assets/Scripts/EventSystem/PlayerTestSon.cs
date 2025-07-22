using UnityEngine;

public class PlayerTestSon : MonoBehaviour
{
    [Header("プレイヤーの基本設定")]
    public int playerIndex = -1; // プレイヤー番号（PlayerManagerから割り当て）
    public float maxHP = 100f;
    private float originMaxHP = 100f; // 初期最大HP
    private float currentHP;


    public void SetIndex(int index)
    {
        playerIndex = index;
    }
    private void Start()
    {
        currentHP = maxHP;
        GameEvents.PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerIndex, new HPInfo(currentHP, maxHP, false, maxHP));
    }

    public void Die()
    {
        
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
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerIndex, new HPInfo(currentHP, maxHP, true));

        // デバッグログ
        Debug.Log($"[Player{playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

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

        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerIndex, new HPInfo(currentHP, maxHP, false));
    }
}

