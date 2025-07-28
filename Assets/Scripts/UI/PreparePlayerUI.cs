using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreparePlayerUI : MonoBehaviour
{
    public Image playerImage; // プレイヤーの画像
    public TextMeshProUGUI playerNameText; // プレイヤー名のテキスト
    public PlayerData playerData; // プレイヤーデータ

    public void SetPlayer(PlayerData playerData)
    {
        this.playerData = playerData;
        playerNameText.text = playerData.playerName; // プレイヤー名を設定
        playerImage.color = playerData.playerColor; // プレイヤーの色を設定
    }
}
