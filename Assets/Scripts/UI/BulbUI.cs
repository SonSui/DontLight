using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BulbUI : MonoBehaviour
{
    public Image ring; // Bulb UIのリング画像コンポーネント
    public Image bulbImage; // 電球の画像コンポーネント
    public Sprite onSprite; // 電球が点灯しているときのスプライト
    public Sprite offSprite; // 電球が消灯しているときのスプライト
    public Color onColor = Color.yellow; // 電球が点灯しているときの色
    public Color offColor = Color.gray; // 電球が消灯しているときの色
    private float maxLength = 1f; // Bulb UIの最大長さ
    private float currentLength = 0f; // Bulb UIの現在の長さ
    private float resetTime = 3f; // 電球が取得するまでの時間
    private bool isOn = true; // 電球の状態（点灯中かどうか）
    private Tween chargeTween; // 電球の充電アニメーション用Tween

    private void Start()
    {
        // 初期化
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
        
        // 初期状態を設定
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
            currentLength = maxLength; // 点灯時は最大長さに設定
            ring.fillAmount = 1f; // リングの表示を最大にする
        }
        else
        {
            ring.color = offColor;
            bulbImage.sprite = offSprite;
            currentLength = 0f; // 消灯時は長さをリセット
            ring.fillAmount = 0f; // リングの表示を消す
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
