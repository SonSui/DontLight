using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public struct HPInfo
{
    public HPInfo(float currentHP, float currentHPMax, bool isDamaged = true, float defaultHPMax = -1f)
    {
        this.currentHP = currentHP;
        this.currentHPMax = currentHPMax;
        this.isDamaged = isDamaged;
        this.defaultHPMax = defaultHPMax;
    }
    public float currentHP; // ���݂�HP
    public float currentHPMax; // ���݂̍ő�HP
    public float defaultHPMax; // �f�t�H���g�̍ő�HP
    public bool isDamaged; // �_���[�W���󂯂����ǂ����̃t���O
}
public class HPUI : MonoBehaviour
{
    public GameObject under; // HP�o�[�̉����I�u�W�F�N�g
    public GameObject frame; // HP�o�[�̃t���[��
    public GameObject topFill; // HP�o�[�̏㕔�t�B��
    public GameObject bottomFill; // HP�o�[�̉����t�B��
    private float defaultLength; // �f�t�H���g��HP�o�[�̒���
    private float defaultHeight; // �f�t�H���g��HP�o�[�̍���
    private Tween hpTween; // HP�o�[�̃t�B�����ω�����Tween
    private Tween hpMaxTween; // HP�o�[�̍ő�t�B�����ω�����Tween
    public GameObject decorate; // HP�o�[�̑���
    private Image decorateImage; // HP�o�[�̉摜�R���|�[�l���g

    private float minFillLength = 135f; // �ŏ��t�B���̒����iHP�o�[�����S�ɋ�łȂ��ꍇ�̍ŏ��l�j

    public Sprite normalDecorate; // �ʏ�̑����X�v���C�g
    public Sprite damagedDecorate; // �_���[�W���󂯂��Ƃ��̑����X�v���C�g
    public Sprite deadDecorate; // ���S���̑����X�v���C�g
    private bool isDead = false; // ���S��Ԃ��ǂ����̃t���O

    public Color playerColor = Color.white; // �v���C���[�̐F

    public float fillDuration = 0.1f; // HP�o�[�̃t�B�����ω����鎞��

    private float defaultHPMax = 20f; // �f�t�H���g�̍ő�HP
    private float currentHPMax = 20f; // ���݂̍ő�HP
    private float currentHP = 20f; // ���݂�HP

    private bool isDamaged = false; // �_���[�W���󂯂����ǂ����̃t���O
    private float damageDuration = 0.8f; // �_���[�W���󂯂��Ƃ��̃A�j���[�V�����̎�������
    private Tween damageResetTween; // �_���[�W���󂯂��Ƃ��̑����̃��Z�b�g�pTween
    private Tween shakeTween; // HP�o�[�̗h��A�j���[�V�����p��Tween
    private float shakeRadius = 5f; // HP�o�[�̗h��̔��a
    private Vector2 defaultDecoratePosition; // �����̃f�t�H���g�ʒu

    private void Start()
    {
        // ������
        if (under == null)
        {
            under = this.gameObject;
        }
        defaultLength = under.GetComponent<RectTransform>().sizeDelta.x;
        defaultHeight = under.GetComponent<RectTransform>().sizeDelta.y;
        if (frame != null)
        {
            frame.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �t���[���̃T�C�Y��ݒ�
        }
        if (bottomFill != null)
        {
            bottomFill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �����t�B���̃T�C�Y��ݒ�
            bottomFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0); // �����t�B���̈ʒu��ݒ�
        }
        if (topFill != null)
        {
            topFill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �㕔�t�B���̃T�C�Y��ݒ�
            topFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0); // �㕔�t�B���̈ʒu��ݒ�
        }
        if(decorate != null)
        {
            decorate.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // �����̃T�C�Y��ݒ�
            defaultDecoratePosition = decorate.GetComponent<RectTransform>().anchoredPosition; // �����̃f�t�H���g�ʒu��ۑ�
        }

        decorateImage = decorate.GetComponent<Image>();
        
        // �f�t�H���g�̑����X�v���C�g��ݒ�
        decorateImage.sprite = normalDecorate;
        // ������Ԃ�HP�o�[���X�V
        UpdateHP(new HPInfo(currentHP, currentHPMax, isDamaged, defaultHPMax));
    }
    public void UpdateHP(HPInfo hpInfo)
    {
        currentHP = hpInfo.currentHP;
        currentHPMax = hpInfo.currentHPMax;
        if (currentHP <= 0)
        {
            isDead = true;
        }
        // �ő�HP���ݒ肳��Ă���ꍇ�͂�����g�p
        if (hpInfo.defaultHPMax > 0f)
        {
            defaultHPMax = hpInfo.defaultHPMax;
        }
        // �_���[�W���󂯂����ǂ����̃t���O���X�V
        isDamaged = hpInfo.isDamaged;
        // HP�o�[�̃t�B�����X�V
        float currentHPLength = ((currentHP) / defaultHPMax) * (defaultLength - minFillLength) + minFillLength ;
        float currentHPMaxLength = ((currentHPMax) / defaultHPMax) * (defaultLength - minFillLength) + minFillLength;
        // �t�B���̃T�C�Y���X�V
        RectTransform bottomRT = bottomFill.GetComponent<RectTransform>();
        RectTransform topRT = topFill.GetComponent<RectTransform>();

        if (hpTween != null && hpTween.IsActive()) hpTween.Kill();
        if (hpMaxTween != null && hpMaxTween.IsActive()) hpMaxTween.Kill();

        hpMaxTween = DOTween.To(
            () => bottomRT.sizeDelta.x,
            x => bottomRT.sizeDelta = new Vector2(x, defaultHeight),
            currentHPMaxLength,
            fillDuration
        );

        hpTween = DOTween.To(
            () => topRT.sizeDelta.x,
            x => topRT.sizeDelta = new Vector2(x, defaultHeight),
            currentHPLength,
            fillDuration
        );

        // damage���󂯂��Ƃ��̑����̍X�V
        if (hpInfo.isDamaged)
        {
            isDamaged = true;
            decorateImage.sprite = damagedDecorate;

            // �_���[�W���󂯂��Ƃ��̑����̈ʒu��h�炷
            if (shakeTween == null || !shakeTween.IsActive())
            {
                shakeTween = decorate.GetComponent<RectTransform>().DOShakeAnchorPos(
                    duration: 1f,
                    strength: new Vector2(shakeRadius, shakeRadius),
                    vibrato: 10,
                    randomness: 90,
                    snapping: false,
                    fadeOut: false
                ).SetLoops(-1);
            }
            // ��Ԃ����Z�b�g��Tween���~
            if (damageResetTween != null && damageResetTween.IsActive())
                damageResetTween.Kill();

            // ��莞�Ԍ�Ƀ_���[�W���󂯂���Ԃ����Z�b�g
            damageResetTween = DOVirtual.DelayedCall(damageDuration, () =>
            {
                isDamaged = false;
                if (isDead)
                {
                    decorateImage.sprite = deadDecorate; // ���S���̑����ɕύX
                }
                else
                {
                    decorateImage.sprite = normalDecorate; // �ʏ�̑����ɖ߂�
                }

                // �_���[�W���󂯂��Ƃ��̑����̃��Z�b�g
                if (shakeTween != null)
                {
                    shakeTween.Kill();
                    shakeTween = null;
                }
                
                // �����̈ʒu�����ɖ߂�
                decorate.GetComponent<RectTransform>().anchoredPosition = defaultDecoratePosition;
            }, false);
        }
    }
    public void SetUIColor(Color color)
    {
        if (decorate != null)
        {
            var img = decorate.GetComponent<Image>();
            if (img != null)
                img.color = color;
        }
    }
}
