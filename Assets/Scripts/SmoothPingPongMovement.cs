using UnityEngine;

public class SmoothPingPongMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 100f;
    public float moveDuration = 1f;
    public float pauseDuration = 0.5f;
    public Vector2 moveDirection = new Vector2(-1, 1).normalized;

    [Header("Shake Settings")]
    public float shakeAngle = 5f;
    public float shakeDuration = 0.15f;
    public int shakeCount = 3;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Quaternion originalRotation;
    private float timer = 0f;
    private bool isMovingForward = true;
    private bool isPausing = false;
    private bool isShaking = false;
    private int shakeCounter = 0;
    private float shakeTimer = 0f;
    private bool shakeDirection = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
    }

    void Update()
    {
        if (isShaking)
        {
            HandleShake();
            return;
        }
        if (isPausing)
        {
            timer += Time.deltaTime;
            if (timer >= pauseDuration)
            {
                timer = 0f;
                isPausing = false;
            }
            return;
        }
        HandleMovement();
    }

    void HandleMovement()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / moveDuration);
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

        if (isMovingForward)
        {
            rectTransform.anchoredPosition = originalPosition + moveDirection * moveDistance * smoothProgress;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition + moveDirection * moveDistance * (1 - smoothProgress);
        }

        if (progress >= 1f)
        {
            timer = 0f;

            if (isMovingForward)
            {
                isShaking = true;
                shakeCounter = 0;
                shakeTimer = 0f;
            }
            else
            {
                isPausing = true;
            }

            isMovingForward = !isMovingForward;
        }
    }

    void HandleShake()
    {
        shakeTimer += Time.deltaTime;
        float shakeProgress = Mathf.Clamp01(shakeTimer / shakeDuration);
        float angle = Mathf.Sin(shakeProgress * Mathf.PI) * shakeAngle * (shakeDirection ? 1 : -1);
        rectTransform.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);

        if (shakeProgress >= 1f)
        {
            shakeTimer = 0f;
            shakeCounter++;
            shakeDirection = !shakeDirection;

            if (shakeCounter >= shakeCount * 2)
            {
                rectTransform.localRotation = originalRotation;
                isShaking = false;
                isPausing = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Vector3 startPos = rectTransform != null ? rectTransform.position : transform.position;
        Vector3 direction = new Vector3(moveDirection.x, moveDirection.y, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPos, startPos + direction * moveDistance / 10f);
    }
}