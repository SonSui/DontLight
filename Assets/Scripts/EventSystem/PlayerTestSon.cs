using UnityEngine;

public class PlayerTestSon : MonoBehaviour
{
    [Header("�v���C���[�̊�{�ݒ�")]
    public int playerIndex = -1; // �v���C���[�ԍ��iPlayerManager���犄�蓖�āj
    public float maxHP = 100f;
    private float originMaxHP = 100f; // �����ő�HP
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
    /// �����Ȃǂɂ��_���[�W���󂯂�
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHP -= damageInfo.damage;
        currentHP = Mathf.Max(0, currentHP);
        maxHP = Mathf.Min((originMaxHP-currentHP)/2 + currentHP,maxHP); // ���݂�HP�ɉ����čő�HP�𒲐�

        // UI�X�V�C�x���g�𑗐M
        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerIndex, new HPInfo(currentHP, maxHP, true));

        // �f�o�b�O���O
        Debug.Log($"[Player{playerIndex}] took {damageInfo.damage} damage. HP: {currentHP}");

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

        GameEvents.PlayerEvents.OnHPChanged?.Invoke(playerIndex, new HPInfo(currentHP, maxHP, false));
    }
}

