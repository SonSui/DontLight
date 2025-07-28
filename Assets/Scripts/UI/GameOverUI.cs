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
        // 初期化
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        rectTransform.localScale = Vector3.one;
        Vector2 currentPos = rectTransform.anchoredPosition;

        animationSequence = DOTween.Sequence();

        // 位置移動
        animationSequence.Append(rectTransform.DOAnchorPos(endPoint, enterDuration).SetEase(Ease.OutCubic));

        // 音声再生 + 待機
        animationSequence.AppendCallback(() =>
        {
            GetComponent<AudioSource>()?.Play();
        });

        animationSequence.AppendInterval(delayBeforeScale);

        // スケール拡大
        animationSequence.Append(rectTransform.DOScale(Vector3.one * scaleFactor, scaleDuration).SetEase(Ease.OutBack));
    }
}