using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BatteryUI : MonoBehaviour
{
    public GameObject under; // バッテリーUIの下部オブジェクト
    private float defaultLength; // デフォルトのバッテリーUIの長さ
    private float defaultHeight; // デフォルトのバッテリーUIの高さ
    public GameObject fill; // バッテリーUIの上部フィル
    public GameObject effect; // バッテリーUIのエフェクト
    private Vector2 defaultEffectSize = new Vector2(58f,106f); // エフェクトのデフォルトサイズ
    private Vector2 defaultEffectPosition = new Vector2(-56f, -4f); // エフェクトのデフォルト位置
    private Vector2 defaultUIsize = new Vector2(150f,64f);
    public GameObject frame; // バッテリーUIのフレーム
    public GameObject decorate; // バッテリーUIの装飾
    private Image thunder; // バッテリーUIのサンダー画像コンポーネント
    public Color fullColor = Color.yellow; // バッテリーが満タンのときの色
    public Color emptyColor = Color.red; // バッテリーが空のときの色
    public Color normalColor = Color.white; // バッテリーが通常のときの色

    private float minFillLength = 56f; // 最小フィルの長さ（バッテリーUIが完全に空でない場合の最小値）

    public float fillDuration = 0.1f; // バッテリーUIのフィルが変化する時間
    private float currentBattery = 10f; // 現在のバッテリー残量
    private float maxBattery = 10f; // 最大バッテリー残量
    private Tween batteryTween; // バッテリーUIのフィルが変化するTween
    private bool isCharging = false; // バッテリーが充電中かどうか
    private Tween effectTween;
    private Tween effectLengthTween; // エフェクトの長さが変化するTween
    private float effectDuration = 0.5f;
    private void Start()
    {
        // 初期化
        if (under == null)
        {
            under = this.gameObject;
        }
        defaultLength = under.GetComponent<RectTransform>().sizeDelta.x;
        defaultHeight = under.GetComponent<RectTransform>().sizeDelta.y;
        
        fill.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // 初期サイズを設定
        fill.GetComponent<RectTransform>().anchoredPosition = new Vector2(-defaultLength / 2, 0);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // フレームのサイズを設定
        decorate.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultLength, defaultHeight); // 装飾のサイズを設定

        thunder = decorate.GetComponent<Image>();
        thunder.color = fullColor; // 初期色を設定

        defaultEffectSize.x *= defaultHeight / defaultUIsize.y;
        defaultEffectSize.y *= defaultLength / defaultUIsize.x;
        defaultEffectPosition.x *= defaultLength / defaultUIsize.x;
        defaultEffectPosition.y *= defaultHeight / defaultUIsize.y;
        effect.GetComponent<RectTransform>().sizeDelta = defaultEffectSize; // エフェクトのサイズを設定
        effect.GetComponent<RectTransform>().anchoredPosition = defaultEffectPosition; // エフェクトの位置を設定

        effect.SetActive(false); // 初期状態ではエフェクトを非表示にする

        UpdateBattery(currentBattery, false, maxBattery); // 初期バッテリー残量を設定
    }
    public void UpdateBattery(float current,bool isCharge,float max = -1f)
    {
        if (max > 0f) maxBattery = max; // 最大バッテリー残量を更新
        currentBattery = current; // 現在のバッテリー残量を更新
        isCharging = isCharge; // 充電状態を更新

        float currentFillLength = (current / maxBattery) * (defaultLength - minFillLength) + minFillLength; // 現在のバッテリー残量に基づくフィルの長さ
        RectTransform fillRect = fill.GetComponent<RectTransform>();

        if (batteryTween != null && batteryTween.IsActive()) batteryTween.Kill();

        batteryTween = DOTween.To(() => fillRect.sizeDelta.x,
            x => fillRect.sizeDelta = new Vector2(x, defaultHeight),
            currentFillLength,
            fillDuration
            );

        float currentEffectLength = (current / maxBattery) * defaultEffectSize.y; // エフェクトの長さを更新
        RectTransform effectRect = effect.GetComponent<RectTransform>();


        if (isCharging)
        {
            effect.SetActive(true); // 充電中はエフェクトを表示

            if (effectTween != null && effectTween.IsActive()) effectTween.Kill(); // 既存のエフェクトTweenを停止

            // 一定時間後にダメージを受けた状態をリセット
            effectTween = DOVirtual.DelayedCall(effectDuration, () =>
            {
                isCharging = false;
                effect.SetActive(false); // 充電が終わったらエフェクトを非表示にする

            }, false);

            if (effectLengthTween != null && effectLengthTween.IsActive()) effectLengthTween.Kill(); // 既存のエフェクト長さTweenを停止
            effectLengthTween = DOTween.To(() => effectRect.sizeDelta.y,
                y => effectRect.sizeDelta = new Vector2(defaultEffectSize.x, y),
                currentEffectLength,
                fillDuration
            );
        }


        if (currentBattery > maxBattery * 0.95f)
        {
            thunder.color = fullColor; // バッテリーが満タンのときの色
        }
        else if (currentBattery > maxBattery * 0.1f)
        {
            thunder.color = normalColor; // バッテリーが満タンに近いときの色
        }
        else
        {
            thunder.color = emptyColor; // バッテリーが空のときの色
        }
    }


}
