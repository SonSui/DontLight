using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BatteryUI : MonoBehaviour
{
    public GameObject under; // �o�b�e���[UI�̉����I�u�W�F�N�g
    private float defaultLength; // �f�t�H���g�̃o�b�e���[UI�̒���
    private float defaultHeight; // �f�t�H���g�̃o�b�e���[UI�̍���
    public GameObject fill; // �o�b�e���[UI�̏㕔�t�B��
    public GameObject effect; // �o�b�e���[UI�̃G�t�F�N�g
    private Vector2 defaultEffectSize = new Vector2(58f,106f); // �G�t�F�N�g�̃f�t�H���g�T�C�Y
    private Vector2 defaultEffectPosition = new Vector2(-56f, -4f); // �G�t�F�N�g�̃f�t�H���g�ʒu
    private Vector2 defaultUIsize = new Vector2(150f,64f);
    public GameObject frame; // �o�b�e���[UI�̃t���[��
    public GameObject decorate; // �o�b�e���[UI�̑���
    private Image thunder; // �o�b�e���[UI�̃T���_�[�摜�R���|�[�l���g
    public Color fullColor = Color.yellow; // �o�b�e���[�����^���̂Ƃ��̐F
    public Color emptyColor = Color.red; // �o�b�e���[����̂Ƃ��̐F
    public Color normalColor = Color.white; // �o�b�e���[���ʏ�̂Ƃ��̐F

    private float minFillLength = 56f; // �ŏ��t�B���̒����i�o�b�e���[UI�����S�ɋ�łȂ��ꍇ�̍ŏ��l�j

    public float fillDuration = 0.1f; // �o�b�e���[UI�̃t�B�����ω����鎞��
    private float currentBattery = 10f; // ���݂̃o�b�e���[�c��
    private float maxBattery = 10f; // �ő�o�b�e���[�c��
    private Tween batteryTween; // �o�b�e���[UI�̃t�B�����ω�����Tween
    private bool isCharging = false; // �o�b�e���[���[�d�����ǂ���
    private Tween effectTween;
    private Tween effectLengthTween; // �G�t�F�N�g�̒������ω�����Tween
    private float effectDuration = 0.5f;
    private void Start()
    {
        // ������
        if (under == null)
        {
            under = this.gameObject;
        }
        defaultLength = under.GetComponent<RectTransform>().sizeDelta.x;
        defaultHeight = under.GetComponent<RectTransform>().sizeDelta.y;
        
        fill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �����T�C�Y��ݒ�
        fill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �t���[���̃T�C�Y��ݒ�
        decorate.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �����̃T�C�Y��ݒ�

        thunder = decorate.GetComponent<Image>();
        thunder.color = fullColor; // �����F��ݒ�

        defaultEffectSize.x *= defaultHeight / defaultUIsize.y;
        defaultEffectSize.y *= defaultLength / defaultUIsize.x;
        defaultEffectPosition.x *= defaultLength / defaultUIsize.x;
        defaultEffectPosition.y *= defaultHeight / defaultUIsize.y;
        effect.GetComponent<RectTransform>().sizeDelta = defaultEffectSize; // �G�t�F�N�g�̃T�C�Y��ݒ�
        effect.GetComponent<RectTransform>().anchoredPosition = defaultEffectPosition; // �G�t�F�N�g�̈ʒu��ݒ�

        effect.SetActive(false); // ������Ԃł̓G�t�F�N�g���\���ɂ���

        UpdateBattery(currentBattery, false, maxBattery); // �����o�b�e���[�c�ʂ�ݒ�
    }
    public void UpdateBattery(float current,bool isCharge,float max = -1f)
    {
        if (max > 0f) maxBattery = max; // �ő�o�b�e���[�c�ʂ��X�V
        currentBattery = current; // ���݂̃o�b�e���[�c�ʂ��X�V
        isCharging = isCharge; // �[�d��Ԃ��X�V

        float currentFillLength = (current / maxBattery) * (defaultLength - minFillLength) + minFillLength; // ���݂̃o�b�e���[�c�ʂɊ�Â��t�B���̒���
        RectTransform fillRect = fill.GetComponent<RectTransform>();

        if (batteryTween != null && batteryTween.IsActive()) batteryTween.Kill();

        batteryTween = DOTween.To(() => fillRect.sizeDelta.x,
            x => fillRect.sizeDelta = new Vector2(x, defaultHeight),
            currentFillLength,
            fillDuration
            );

        float currentEffectLength = (current / maxBattery) * defaultEffectSize.y; // �G�t�F�N�g�̒������X�V
        RectTransform effectRect = effect.GetComponent<RectTransform>();


        if (isCharging)
        {
            effect.SetActive(true); // �[�d���̓G�t�F�N�g��\��

            if (effectTween != null && effectTween.IsActive()) effectTween.Kill(); // �����̃G�t�F�N�gTween���~

            // ��莞�Ԍ�Ƀ_���[�W���󂯂���Ԃ����Z�b�g
            effectTween = DOVirtual.DelayedCall(effectDuration, () =>
            {
                isCharging = false;
                effect.SetActive(false); // �[�d���I�������G�t�F�N�g���\���ɂ���

            }, false);

            if (effectLengthTween != null && effectLengthTween.IsActive()) effectLengthTween.Kill(); // �����̃G�t�F�N�g����Tween���~
            effectLengthTween = DOTween.To(() => effectRect.sizeDelta.y,
                y => effectRect.sizeDelta = new Vector2(defaultEffectSize.x, y),
                currentEffectLength,
                fillDuration
            );
        }


        if (currentBattery > maxBattery * 0.95f)
        {
            thunder.color = fullColor; // �o�b�e���[�����^���̂Ƃ��̐F
        }
        else if (currentBattery > maxBattery * 0.1f)
        {
            thunder.color = normalColor; // �o�b�e���[�����^���ɋ߂��Ƃ��̐F
        }
        else
        {
            thunder.color = emptyColor; // �o�b�e���[����̂Ƃ��̐F
        }
    }


}
