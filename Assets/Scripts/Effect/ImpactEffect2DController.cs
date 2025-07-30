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
        // �����X�P�[��
        transform.localScale = Vector3.one * startScale;

        Vector3 endPosition = transform.localPosition + Vector3.up * moveDistance;

        Sequence seq = DOTween.Sequence();

        // �g�� + �㏸�i0.5�b�j
        seq.Append(transform.DOScale(Vector3.one * endScale, stage1Time).SetEase(Ease.OutBack));
        seq.Join(transform.DOLocalMove(endPosition, stage1Time).SetEase(Ease.OutSine));

        // �ҋ@�i0.3�b�j
        seq.AppendInterval(stage2Time);

        // �k���i0.5�b�j
        seq.Append(transform.DOScale(Vector3.one * startScale, stage3Time).SetEase(Ease.InQuad));

        // �I����ɍ폜
        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}