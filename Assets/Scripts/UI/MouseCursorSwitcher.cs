using UnityEngine;

public class MouseCursorSwitcher : MonoBehaviour
{
    [Header("�J�[�\���摜�iTexture2D�j")]
    public Texture2D normalCursor; // �ʏ��Ԃ̃J�[�\��
    public Texture2D clickCursor;  // �N���b�N���̃J�[�\��

    [Header("�J�[�\���̃z�b�g�X�|�b�g�i�N���b�N�_�j")]
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
        // ������Ԃ̃J�[�\����ݒ�
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
    }

    void Update()
    {
        // �}�E�X�_�E�����ɃN���b�N�J�[�\���֐؂�ւ�
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
            isMouseDown = true;
        }

        // �}�E�X�A�b�v���ɒʏ�J�[�\���֖߂�
        if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
            isMouseDown = false;
        }

        // �}�E�X�{�^������������ςȂ��̂Ƃ��J�[�\�����ێ�
        if (Input.GetMouseButton(0) && !isMouseDown)
        {
            Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
            isMouseDown = true;
        }
    }
}
