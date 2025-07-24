using UnityEngine;

public class PlayerTestSon : MonoBehaviour
{
    [Header("�v���C���[�̊�{�ݒ�")]
    
    private float currentHP;
    private float originMaxHP = 100f; // �����ő�HP
    public float maxHP = 100f;
    public bool isTest = false;
    private PlayerData playerData = new PlayerData();


    public void SetIndex(int index)
    {
        playerData.playerIndex = index;
    }
    public void SetPlayerData(PlayerData data)
    {
        playerData = data;
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
        
        GameEvents.PlayerEvents.OnPlayerDied?.Invoke(this.gameObject);
        Destroy(gameObject);
    }
    /// <summary>
    /// �����Ȃǂɂ��_���[�W���󂯂�
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // ���݂�HP�ɉ����čő�HP�𒲐�

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

    /// <summary>
    /// HP����
    /// </summary>
    public void Heal(float amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerData.playerIndex, new HPInfo(currentHP, maxHP, false));
    }
}

