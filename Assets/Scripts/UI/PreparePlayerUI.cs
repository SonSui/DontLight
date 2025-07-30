using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreparePlayerUI : MonoBehaviour
{
    public Image playerImage; // �v���C���[�̉摜
    public TextMeshProUGUI playerNameText; // �v���C���[���̃e�L�X�g
    public PlayerData playerData; // �v���C���[�f�[�^

    public void SetPlayer(PlayerData playerData)
    {
        this.playerData = playerData;
        playerNameText.text = playerData.playerName; // �v���C���[����ݒ�
        playerImage.color = playerData.playerColor; // �v���C���[�̐F��ݒ�
    }
}
