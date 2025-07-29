using UnityEngine;

public class MouseCursorSwitcher : MonoBehaviour
{
    [Header("カーソル画像（Texture2D）")]
    public Texture2D normalCursor; // 通常状態のカーソル
    public Texture2D clickCursor;  // クリック時のカーソル

    [Header("カーソルのホットスポット（クリック点）")]
    public Vector2 hotSpot = Vector2.zero;

    private static MouseCursorSwitcher instance;
    private bool isMouseDown = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 初期状態のカーソルを設定
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
    }

    void Update()
    {
        // マウスダウン時にクリックカーソルへ切り替え
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
            isMouseDown = true;
        }

        // マウスアップ時に通常カーソルへ戻す
        if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
            isMouseDown = false;
        }

        // マウスボタンが押されっぱなしのときカーソルを維持
        if (Input.GetMouseButton(0) && !isMouseDown)
        {
            Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
            isMouseDown = true;
        }
    }
}
