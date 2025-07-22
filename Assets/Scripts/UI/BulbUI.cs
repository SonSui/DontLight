using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BulbUI : MonoBehaviour
{
    public Image ring; // Bulb UI�̃����O�摜�R���|�[�l���g
    public Image bulbImage; // �d���̉摜�R���|�[�l���g
    public Sprite onSprite; // �d�����_�����Ă���Ƃ��̃X�v���C�g
    public Sprite offSprite; // �d�����������Ă���Ƃ��̃X�v���C�g
    public Color onColor = Color.yellow; // �d�����_�����Ă���Ƃ��̐F
    public Color offColor = Color.gray; // �d�����������Ă���Ƃ��̐F
    private float maxLength = 1f; // Bulb UI�̍ő咷��
    private float currentLength = 0f; // Bulb UI�̌��݂̒���
    private float resetTime = 3f; // �d�����擾����܂ł̎���
    private bool isOn = true; // �d���̏�ԁi�_�������ǂ����j
    private Tween chargeTween; // �d���̏[�d�A�j���[�V�����pTween

    private void Start()
    {
        // ������
        if (ring == null)
        {
            ring = this.gameObject.GetComponent<Image>();
            if (ring == null)
            {
                Debug.LogError("BulbUI: Ring Image component not found. Please assign it in the inspector.");
                return;
            }
            ring.color = onColor;
        }
        if (bulbImage == null)
        {
            bulbImage = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Image>() : null ;
            if (bulbImage == null)
            {
                Debug.LogError("BulbUI: Bulb Image component not found. Please assign it in the inspector.");
                return;
            }
            else
            {
                bulbImage.sprite = onSprite;
            }
        }
        
        // ������Ԃ�ݒ�
        SetBulbState(true);
    }
    public void SetBulbState(bool on)
    {
        if (chargeTween != null && chargeTween.IsActive())
        {
            chargeTween.Kill();
            chargeTween = null;
        }

        this.isOn = on;
        if (isOn)
        {
            ring.color = onColor;
            bulbImage.sprite = onSprite;
            currentLength = maxLength; // �_�����͍ő咷���ɐݒ�
            ring.fillAmount = 1f; // �����O�̕\�����ő�ɂ���
        }
        else
        {
            ring.color = offColor;
            bulbImage.sprite = offSprite;
            currentLength = 0f; // �������͒��������Z�b�g
            ring.fillAmount = 0f; // �����O�̕\��������
            chargeTween = DOTween.To(
                () => currentLength,
                x =>
                {
                    currentLength = x;
                    ring.fillAmount = currentLength / maxLength;
                },
                maxLength,
                resetTime
            ).OnComplete(() =>
            {

                SetBulbState(true);
            });

        }
    }
    public void SetCooldown(float cd)
    {
        resetTime = cd;
    }

    public void StartCooldown()
    {
        if (gameObject.activeInHierarchy)
        {
            if (chargeTween != null && chargeTween.IsActive())
                chargeTween.Kill();

            chargeTween = ring.DOFillAmount(0f, resetTime)
                .SetEase(Ease.Linear)
                .OnComplete(() => SetBulbState(true));
        }
    }
}
