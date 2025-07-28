using UnityEngine;
using DG.Tweening;
using System;

public class GameOverUI : MonoBehaviour
{
    public static Action<PlayerData> OnWinnerSet;

    private RectTransform rectTransform;

    public Vector2 startPoint = new Vector2(0, 768); // 初期位置（画面外上部）
    public Vector2 endPoint = new Vector2(0, 0); // 終了位置（画面中央）
    public float enterDuration = 0.5f; // 入場アニメーションの時間
    public float bounceHeight = 30f; // バウンスの高さ
    public float bounceDuration = 0.2f; // バウンスの時間
    public float finalBounceHeight = 10f; // 最終的なバウンスの高さ
    public float finalBounceDuration = 0.4f; // 最終的なバウンスの時間


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
        rectTransform.anchoredPosition = startPoint; // 初期位置に設定

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