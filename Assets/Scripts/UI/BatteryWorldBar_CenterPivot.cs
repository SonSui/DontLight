using UnityEngine;
using DG.Tweening;

/// <summary>
/// �V�[�����̃o�b�e���[�Q�[�W�i���SPivot��Sprite�ł�����ŐL�k�j
/// �w�i/�t�B��/�t���[��/�����FSpriteRenderer
/// �G�t�F�N�g�FQuad(MeshRenderer) + URP/LightningAdditive
/// UI�ł� UpdateBattery(current, isCharging, max) �ƌ݊�
/// </summary>
public class BatteryWorldBar_CenterPivot : MonoBehaviour
{
    [Header("=== �Q�� ===")]
    [Tooltip("�w�i�i���n�j")]
    public SpriteRenderer under;
    [Tooltip("�c�ʃt�B���i���ɐL�k������j")]
    public SpriteRenderer fill;
    [Tooltip("�O�g")]
    public SpriteRenderer frame;
    [Tooltip("�����i���A�C�R���Ȃǁj")]
    public SpriteRenderer decorate;

    [Tooltip("�G�t�F�N�g�p�FQuad �� Transform�iMeshRenderer ���t���Ă���j")]
    public Transform effectRoot;

    [Header("=== ������ ===")]
    public Color fullColor = Color.yellow; // 95%�ȏ�
    public Color emptyColor = Color.red;    // 10%�ȉ�
    public Color normalColor = Color.white;  // ����ȊO

    [Header("=== �A�j�� ===")]
    [Tooltip("�t�B���̒�����Ԏ���")]
    public float fillDuration = 0.1f;
    [Tooltip("�[�d�G�t�F�N�g�̕\���b��")]
    public float effectDuration = 0.5f;

    [Header("=== �l ===")]
    [SerializeField] private float currentBattery = 10f;
    [SerializeField] private float maxBattery = 10f;
    public float lifeTime = 1f; // �L�����ԁi�b�j�B1�b��Ɏ����Ŕ�\���ɂȂ�

    // ����
    private MeshRenderer effectMR;
    private Tween fillTween, effectShowTween, effectScaleTween;
    private bool isCharging = false;

    // �t�B������Transform�i����␳�ɕK�v�j
    private Vector3 fillLocalPos0;
    private Vector3 fillLocalScale0;
    private Vector3 fillLocalEndPos = new Vector3(-0.42f,0,0);

    // �G�t�F�N�g����
    private Vector3 effLocalPos0;
    private Vector3 effLocalScale0;

    private void Awake()
    {
        if (!fill) Debug.LogWarning("[BatteryWorldBar_CenterPivot] fill �����蓖�ĂĂ��������B");
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
        // === �t�B���������L�^ ===
        fillLocalPos0 = fill.transform.localPosition;
        fillLocalScale0 = fill.transform.localScale;

        // === �G�t�F�N�g�������L�^ ===
        if (effectRoot)
        {
            effLocalPos0 = effectRoot.localPosition;
            effLocalScale0 = effectRoot.localScale;
            SetEffectActive(false);
        }

        if (decorate) decorate.color = fullColor;

        // �������f
        ApplyBattery(currentBattery, false, maxBattery, instant: true);
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        effectShowTween?.Kill();
        effectScaleTween?.Kill();
    }

    /// <summary>UI�łƓ��������ŊO���X�V</summary>
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

        // === ����L�k�i���SPivot�ł����[�Œ�j ===
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

        // === �[�d�G�t�F�N�g�i�c�X�P�[���������Ɂj ===
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

        // === �F���� ===
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
        // === ����L�k�i���SPivot�ł����[�Œ�j ===
        float targetScaleX = fillLocalScale0.x * ratio;
        float targetPosX = (fillLocalEndPos.x - fillLocalPos0.x) * (1 - ratio);
        Vector3 targetScale = new Vector3(targetScaleX, fillLocalScale0.y, fillLocalScale0.z);
        Vector3 targetPos = new Vector3(targetPosX, fillLocalPos0.y, fillLocalPos0.z);
        fillTween?.Kill();
        fill.transform.localScale = targetScale;
        fill.transform.localPosition = targetPos;
        // === �[�d�G�t�F�N�g�i�c�X�P�[���������Ɂj ===
        if (effectRoot)
        {
            effectScaleTween?.Kill();
            effectRoot.localScale = new Vector3(effLocalScale0.x, effLocalScale0.y * ratio, effLocalScale0.z);
            effectRoot.localPosition = new Vector3(effLocalPos0.x - (1 - ratio) * (effLocalScale0.y / 2), effLocalPos0.y, effLocalPos0.z);
        }
        // === �F���� ===
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
