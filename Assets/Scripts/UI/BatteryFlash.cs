using UnityEngine;
using DG.Tweening;

public class BatteryFlash : MonoBehaviour
{
    public float cycleDuration = 0.25f;
    public float visibleAlpha = 1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Tween flashTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        StartFlashing();
        Invoke(nameof(DisableItself), 1f);
    }

    private void OnDisable()
    {
        StopFlashing();
        CancelInvoke();
    }

    private void OnDestroy()
    {
        StopFlashing();
    }

    private void StartFlashing()
    {
        if (spriteRenderer == null) return;

        DOTween.Kill(spriteRenderer);
        flashTween?.Kill();
        flashTween = null;

        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        float half = Mathf.Max(0.01f, cycleDuration * 0.5f);

        flashTween = spriteRenderer
            .DOFade(visibleAlpha, half)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear)
            .SetTarget(spriteRenderer);
    }

    private void StopFlashing()
    {
        DOTween.Kill(spriteRenderer);
        if (flashTween != null && flashTween.IsActive())
        {
            flashTween.Kill();
            flashTween = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void DisableItself()
    {
        gameObject.SetActive(false);
    }
}
