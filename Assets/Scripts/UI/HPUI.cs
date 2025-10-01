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
    public float currentHP; // 現在のHP
    public float currentHPMax; // 現在の最大HP
    public float defaultHPMax; // デフォルトの最大HP
    public bool isDamaged; // ダメージを受けたかどうかのフラグ
}
public class HPUI : MonoBehaviour
{
    public GameObject under; // HPバーの下部オブジェクト
    public GameObject frame; // HPバーのフレーム
    public GameObject topFill; // HPバーの上部フィル
    public GameObject bottomFill; // HPバーの下部フィル
    private float defaultLength; // デフォルトのHPバーの長さ
    private float defaultHeight; // デフォルトのHPバーの高さ
    private Tween hpTween; // HPバーのフィルが変化するTween
    private Tween hpMaxTween; // HPバーの最大フィルが変化するTween
    public GameObject decorate; // HPバーの装飾
    private Image decorateImage; // HPバーの画像コンポーネント

    private float minFillLength = 135f; // 最小フィルの長さ（HPバーが完全に空でない場合の最小値）

    public Sprite normalDecorate; // 通常の装飾スプライト
    public Sprite damagedDecorate; // ダメージを受けたときの装飾スプライト
    public Sprite deadDecorate; // 死亡時の装飾スプライト
    private bool isDead = false; // 死亡状態かどうかのフラグ

    public Color playerColor = Color.white; // プレイヤーの色

    public float fillDuration = 0.1f; // HPバーのフィルが変化する時間

    private float defaultHPMax = 20f; // デフォルトの最大HP
    private float currentHPMax = 20f; // 現在の最大HP
    private float currentHP = 20f; // 現在のHP

    private bool isDamaged = false; // ダメージを受けたかどうかのフラグ
    private float damageDuration = 0.8f; // ダメージを受けたときのアニメーションの持続時間
    private Tween damageResetTween; // ダメージを受けたときの装飾のリセット用Tween
    private Tween shakeTween; // HPバーの揺れアニメーション用のTween
    private float shakeRadius = 5f; // HPバーの揺れの半径
    private Vector2 defaultDecoratePosition; // 装飾のデフォルト位置

    private void Start()
    {
        // 初期化
        if (under == null)
        {
            under = this.gameObject;
        }
        defaultLength = under.GetComponent<RectTransform>().sizeDelta.x;
        defaultHeight = under.GetComponent<RectTransform>().sizeDelta.y;
        if (frame != null)
        {
            frame.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // フレームのサイズを設定
        }
        if (bottomFill != null)
        {
            bottomFill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // 下部フィルのサイズを設定
            bottomFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0); // 下部フィルの位置を設定
        }
        if (topFill != null)
        {
            topFill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // 上部フィルのサイズを設定
            topFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0); // 上部フィルの位置を設定
        }
        if(decorate != null)
        {
            decorate.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // 装飾のサイズを設定
            defaultDecoratePosition = decorate.GetComponent<RectTransform>().anchoredPosition; // 装飾のデフォルト位置を保存
        }

        decorateImage = decorate.GetComponent<Image>();
        
        // デフォルトの装飾スプライトを設定
        decorateImage.sprite = normalDecorate;
        // 初期状態のHPバーを更新
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
        // 最大HPが設定されている場合はそれを使用
        if (hpInfo.defaultHPMax > 0f)
        {
            defaultHPMax = hpInfo.defaultHPMax;
        }
        // ダメージを受けたかどうかのフラグを更新
        isDamaged = hpInfo.isDamaged;
        // HPバーのフィルを更新
        float currentHPLength = ((currentHP) / defaultHPMax) * (defaultLength - minFillLength) + minFillLength ;
        float currentHPMaxLength = ((currentHPMax) / defaultHPMax) * (defaultLength - minFillLength) + minFillLength;
        // フィルのサイズを更新
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

        // damageを受けたときの装飾の更新
        if (hpInfo.isDamaged)
        {
            isDamaged = true;
            decorateImage.sprite = damagedDecorate;

            // ダメージを受けたときの装飾の位置を揺らす
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
            // 状態をリセットのTweenを停止
            if (damageResetTween != null && damageResetTween.IsActive())
                damageResetTween.Kill();

            // 一定時間後にダメージを受けた状態をリセット
            damageResetTween = DOVirtual.DelayedCall(damageDuration, () =>
            {
                isDamaged = false;
                if (isDead)
                {
                    decorateImage.sprite = deadDecorate; // 死亡時の装飾に変更
                }
                else
                {
                    decorateImage.sprite = normalDecorate; // 通常の装飾に戻す
                }

                // ダメージを受けたときの装飾のリセット
                if (shakeTween != null)
                {
                    shakeTween.Kill();
                    shakeTween = null;
                }
                
                // 装飾の位置を元に戻す
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
