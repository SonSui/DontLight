using UnityEngine;
using DG.Tweening;

public class ImpactEffect2DController : MonoBehaviour
{
    public float startScale = 0.01f;
    public float endScale = 0.05f;
    public float moveDistance = 0.5f;
    public float stage1Time = 0.5f;
    public float stage2Time = 0.3f;
    public float stage3Time = 0.5f;

    private void Start()
    {
        // 初期スケール
        transform.localScale = Vector3.one * startScale;

        Vector3 endPosition = transform.localPosition + Vector3.up * moveDistance;

        Sequence seq = DOTween.Sequence();

        // 拡大 + 上昇（0.5秒）
        seq.Append(transform.DOScale(Vector3.one * endScale, stage1Time).SetEase(Ease.OutBack));
        seq.Join(transform.DOLocalMove(endPosition, stage1Time).SetEase(Ease.OutSine));

        // 待機（0.3秒）
        seq.AppendInterval(stage2Time);

        // 縮小（0.5秒）
        seq.Append(transform.DOScale(Vector3.one * startScale, stage3Time).SetEase(Ease.InQuad));

        // 終了後に削除
        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}