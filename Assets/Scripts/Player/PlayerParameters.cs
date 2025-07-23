using UnityEngine;

public class PlayerParameters : MonoBehaviour
{
    // This script is used to set the player's color based on menu selection
    // このスクリプトは、メニュー選択に基づいてプレイヤーの色を設定するために使用されます

    [SerializeField] private Renderer targetRenderer;

    private void Start()
    {
        // Initialize the player's color to red by default for testing
        // テストのために、デフォルトでプレイヤーの色を赤に初期化します
        InitializePlayerColor(Color.red);
    }

    // This method is called to initialize the player's color by menu selection
    // このメソッドは、メニュー選択によってプレイヤーの色を初期化するために呼び出されます
    public void InitializePlayerColor(Color selectedColor)
    {
        if (targetRenderer == null || targetRenderer.material == null)
        {
            Debug.LogWarning("Renderer or Material didn't seted");
            return;
        }

        // Set color
        // 色を設定します
        targetRenderer.material.SetColor("_MainColor", selectedColor);
    }
}
