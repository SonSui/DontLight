using UnityEngine;
using DG.Tweening;
using System;

public class GameOverUI : MonoBehaviour
{
    public static Action<PlayerData> OnWinnerSet;

    private RectTransform rectTransform;
    private Sequence animationSequence;

    public Vector2 endPoint = Vector2.zero;
    public float enterDuration = 0.5f;
    public float delayBeforeScale = 0.2f;
    public float scaleDuration = 0.5f;
    public float scaleFactor = 1.5f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        GameEvents.PlayerEvents.OnWinnerSet += PlayEnterAnimation;
    }

    private void OnDisable()
    {
        GameEvents.PlayerEvents.OnWinnerSet -= PlayEnterAnimation;
    }

    private void OnDestroy()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
    }

    private void PlayEnterAnimation(PlayerData _)
    {
        // ������
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        rectTransform.localScale = Vector3.one;
        Vector2 currentPos = rectTransform.anchoredPosition;

        animationSequence = DOTween.Sequence();

        // �ʒu�ړ�
        animationSequence.Append(rectTransform.DOAnchorPos(endPoint, enterDuration).SetEase(Ease.OutCubic));

        // �����Đ� + �ҋ@
        animationSequence.AppendCallback(() =>
        {
            GetComponent<AudioSource>()?.Play();
        });

        animationSequence.AppendInterval(delayBeforeScale);

        // �X�P�[���g��
        animationSequence.Append(rectTransform.DOScale(Vector3.one * scaleFactor, scaleDuration).SetEase(Ease.OutBack));
    }
}