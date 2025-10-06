using UnityEngine;
using DG.Tweening;

/// <summary>
/// シーン内のバッテリーゲージ（中心PivotのSpriteでも左基準で伸縮）
/// 背景/フィル/フレーム/装飾：SpriteRenderer
/// エフェクト：Quad(MeshRenderer) + URP/LightningAdditive
/// UI版の UpdateBattery(current, isCharging, max) と互換
/// </summary>
public class BatteryWorldBar_CenterPivot : MonoBehaviour
{
    [Header("=== 参照 ===")]
    [Tooltip("背景（下地）")]
    public SpriteRenderer under;
    [Tooltip("残量フィル（横に伸縮させる）")]
    public SpriteRenderer fill;
    [Tooltip("外枠")]
    public SpriteRenderer frame;
    [Tooltip("装飾（雷アイコンなど）")]
    public SpriteRenderer decorate;

    [Tooltip("エフェクト用：Quad の Transform（MeshRenderer が付いている）")]
    public Transform effectRoot;

    [Header("=== 見た目 ===")]
    public Color fullColor = Color.yellow; // 95%以上
    public Color emptyColor = Color.red;    // 10%以下
    public Color normalColor = Color.white;  // それ以外

    [Header("=== アニメ ===")]
    [Tooltip("フィルの長さ補間時間")]
    public float fillDuration = 0.1f;
    [Tooltip("充電エフェクトの表示秒数")]
    public float effectDuration = 0.5f;

    [Header("=== 値 ===")]
    [SerializeField] private float currentBattery = 10f;
    [SerializeField] private float maxBattery = 10f;
    public float lifeTime = 1f; // 有効時間（秒）。1秒後に自動で非表示になる

    // 内部
    private MeshRenderer effectMR;
    private Tween fillTween, effectShowTween, effectScaleTween;
    private bool isCharging = false;

    // フィル初期Transform（左基準補正に必要）
    private Vector3 fillLocalPos0;
    private Vector3 fillLocalScale0;
    private Vector3 fillLocalEndPos = new Vector3(-0.42f,0,0);

    // エフェクト初期
    private Vector3 effLocalPos0;
    private Vector3 effLocalScale0;

    private void Awake()
    {
        if (!fill) Debug.LogWarning("[BatteryWorldBar_CenterPivot] fill を割り当ててください。");
        if (effectRoot) effectMR = effectRoot.GetComponent<MeshRenderer>();
    }
    private void OnEnable()
    {
        Invoke(nameof(DisableItself), lifeTime);
    }
    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Start()
    {
        // === フィル初期を記録 ===
        fillLocalPos0 = fill.transform.localPosition;
        fillLocalScale0 = fill.transform.localScale;

        // === エフェクト初期を記録 ===
        if (effectRoot)
        {
            effLocalPos0 = effectRoot.localPosition;
            effLocalScale0 = effectRoot.localScale;
            SetEffectActive(false);
        }

        if (decorate) decorate.color = fullColor;

        // 初期反映
        ApplyBattery(currentBattery, false, maxBattery, instant: true);
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        effectShowTween?.Kill();
        effectScaleTween?.Kill();
    }

    /// <summary>UI版と同じ引数で外部更新</summary>
    public void UpdateBattery(float current, bool isCharge, float max = -1f)
    {
        gameObject.SetActive(true);
        CancelInvoke();
        Invoke(nameof(DisableItself), lifeTime);
        if (max > 0f) maxBattery = max;
        currentBattery = Mathf.Clamp(current, 0f, Mathf.Max(1e-6f, maxBattery));
        isCharging = isCharge;
        ApplyBattery(currentBattery, isCharging, maxBattery, instant: false);
    }
    public void UpdateBatteryImmediately(float currnt,float max = -1f)
    {
        if (max > 0f) maxBattery = max;
        currentBattery = Mathf.Clamp(currnt, 0f, Mathf.Max(1e-6f, maxBattery));
        ApplyBatteryImmediately(currentBattery,maxBattery);
    }
    private void ApplyBattery(float current, bool charging, float max, bool instant)
    {
        float ratio = Mathf.Clamp01(current / Mathf.Max(1e-6f, max));

        // === 左基準伸縮（中心Pivotでも左端固定） ===
        float targetScaleX = fillLocalScale0.x * ratio;
        float targetPosX = (fillLocalEndPos.x - fillLocalPos0.x) * (1 - ratio);

        Vector3 targetScale = new Vector3(targetScaleX, fillLocalScale0.y, fillLocalScale0.z);
        Vector3 targetPos = new Vector3(targetPosX, fillLocalPos0.y, fillLocalPos0.z);

        fillTween?.Kill();
        if (instant || fillDuration <= 0f)
        {
            fill.transform.localScale = targetScale;
            fill.transform.localPosition = targetPos;
        }
        else
        {
            fillTween = DOTween.Sequence()
                .Join(fill.transform.DOScale(targetScale, fillDuration))
                .Join(fill.transform.DOLocalMove(targetPos, fillDuration))
                .SetUpdate(false);
        }

        // === 充電エフェクト（縦スケールを割合に） ===
        if (effectRoot)
        {
            if (charging)
            {
                SetEffectActive(true);

                effectShowTween?.Kill();
                effectShowTween = DOVirtual.DelayedCall(effectDuration, () =>
                {
                    isCharging = false;
                    SetEffectActive(false);
                }, ignoreTimeScale: false);

                Vector3 effTargetScale = new Vector3(effLocalScale0.x, effLocalScale0.y * ratio, effLocalScale0.z);
                Vector3 effTargetPos = new Vector3(effLocalPos0.x - (1 - ratio) * (effLocalScale0.y / 2), effLocalPos0.y , effLocalPos0.z);
                effectScaleTween?.Kill();
                if (instant || fillDuration <= 0f)
                {
                    effectRoot.localScale = effTargetScale;
                    effectRoot.localPosition = effTargetPos;
                }
                else
                {
                    effectScaleTween = DOTween.Sequence()
                       .Join(effectRoot.DOScale(effTargetScale, fillDuration))
                       .Join(effectRoot.DOLocalMove(effTargetPos, fillDuration))
                       .SetUpdate(false);
                }
            }
        }

        // === 色分け ===
        if (decorate)
        {
            if (current > max * 0.95f) decorate.color = fullColor;
            else if (current > max * 0.10f) decorate.color = normalColor;
            else decorate.color = emptyColor;
        }
    }
    private void ApplyBatteryImmediately(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / Mathf.Max(1e-6f, max));
        // === 左基準伸縮（中心Pivotでも左端固定） ===
        float targetScaleX = fillLocalScale0.x * ratio;
        float targetPosX = (fillLocalEndPos.x - fillLocalPos0.x) * (1 - ratio);
        Vector3 targetScale = new Vector3(targetScaleX, fillLocalScale0.y, fillLocalScale0.z);
        Vector3 targetPos = new Vector3(targetPosX, fillLocalPos0.y, fillLocalPos0.z);
        fillTween?.Kill();
        fill.transform.localScale = targetScale;
        fill.transform.localPosition = targetPos;
        // === 充電エフェクト（縦スケールを割合に） ===
        if (effectRoot)
        {
            effectScaleTween?.Kill();
            effectRoot.localScale = new Vector3(effLocalScale0.x, effLocalScale0.y * ratio, effLocalScale0.z);
            effectRoot.localPosition = new Vector3(effLocalPos0.x - (1 - ratio) * (effLocalScale0.y / 2), effLocalPos0.y, effLocalPos0.z);
        }
        // === 色分け ===
        if (decorate)
        {
            if (current > max * 0.95f) decorate.color = fullColor;
            else if (current > max * 0.10f) decorate.color = normalColor;
            else decorate.color = emptyColor;
        }
    }

    private void SetEffectActive(bool on)
    {
        if (!effectRoot) return;
        if (!effectMR) effectMR = effectRoot.GetComponent<MeshRenderer>();
        if (effectMR) effectMR.enabled = on;
        else effectRoot.gameObject.SetActive(on);
    }
    private void DisableItself()
    {
        gameObject.SetActive(false);
    }
}
