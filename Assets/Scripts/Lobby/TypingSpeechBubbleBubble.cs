using UnityEngine;
using TMPro;

public class TypingSpeechBubbleBubble : MonoBehaviour
{
    [Header("Normal Settings")]
    public string baseText;
    public string dots;
    public float dotInterval = 0.5f;
    public float pauseDuration = 1f;

    [Header("Effect Settings")]
    public float shakeIntensity = 1f;
    public float scaleIntensity = 0.1f;
    public float colorIntensity = 0.1f;
    public float effectSpeed = 3f;

    private TMP_Text speachContent;
    private int dotCount = 0;
    private float timer = 0f;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;
    private float effectTimer = 0f;

    void Start()
    {
        speachContent = GetComponent<TMP_Text>();
        speachContent.text = baseText;

        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalColor = speachContent.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= dotInterval)
        {
            timer = 0f;
            dotCount++;

            if (dotCount <= dots.Length)
            {
                speachContent.text = baseText + dots.Substring(0, dotCount);
            }
            else if (dotCount == dots.Length + 1)
            {
                speachContent.text = baseText;
            }
            else if (dotCount >= dots.Length + 1 + (pauseDuration / dotInterval))
            {
                dotCount = 0;
            }
        }
        effectTimer += Time.deltaTime * effectSpeed;
        float shakeX = Mathf.Sin(effectTimer * 2f) * shakeIntensity;
        float shakeY = Mathf.Cos(effectTimer * 1.5f) * shakeIntensity;
        transform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
        float scaleFactor = 1f + Mathf.Sin(effectTimer) * scaleIntensity;
        transform.localScale = originalScale * scaleFactor;
        float colorOffset = Mathf.Sin(effectTimer * 0.5f) * colorIntensity;
        speachContent.color = new Color(
            originalColor.r + colorOffset,
            originalColor.g + colorOffset,
            originalColor.b + colorOffset,
            originalColor.a);
    }
}