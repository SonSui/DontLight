using UnityEngine;
using DG.Tweening;
using System;

public class GameOverUI : MonoBehaviour
{
    public static Action<PlayerData> OnWinnerSet;

    private RectTransform rectTransform;

    public Vector2 startPoint = new Vector2(0, 768); // �����ʒu�i��ʊO�㕔�j
    public Vector2 endPoint = new Vector2(0, 0); // �I���ʒu�i��ʒ����j
    public float enterDuration = 0.5f; // ����A�j���[�V�����̎���
    public float bounceHeight = 30f; // �o�E���X�̍���
    public float bounceDuration = 0.2f; // �o�E���X�̎���
    public float finalBounceHeight = 10f; // �ŏI�I�ȃo�E���X�̍���
    public float finalBounceDuration = 0.4f; // �ŏI�I�ȃo�E���X�̎���


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 768); 
    }

    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnWinnerSet += PlayEnterAnimation;
    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnWinnerSet -= PlayEnterAnimation;
    }

    private void PlayEnterAnimation(PlayerData _)
    {
        rectTransform.anchoredPosition = startPoint; // �����ʒu�ɐݒ�

        rectTransform.DOAnchorPosY(endPoint.y, enterDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                GetComponent<AudioSource>().Play();
                rectTransform.DOAnchorPosY(bounceHeight,bounceDuration).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        rectTransform.DOAnchorPosY(finalBounceHeight, bounceDuration).SetEase(Ease.InOutQuad)
                            .OnComplete(() =>
                            {
                                rectTransform.DOAnchorPosY(endPoint.y,finalBounceDuration).SetEase(Ease.OutBounce);
                            });
                    });
            });
    }
}