using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FloatingFlipImage : MonoBehaviour
{
    [Header("Target Position")]
    public Vector2 pointA;
    public Vector2 pointB;

    [Header("Move Speed")]
    public float moveSpeed = 250f;

    [Header("Rotation Speed")]
    public float flipSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 currentTarget;
    private bool isFlipping = false;
    private float flipProgress = 0f;
    private bool isFacingRight = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pointA;
        currentTarget = pointB;
        rectTransform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (isFlipping)
        {
            flipProgress += Time.deltaTime * flipSpeed;
            float targetAngle = isFacingRight ? 180f : 0f;
            float yRotation = Mathf.Lerp(isFacingRight ? 0f : 180f, targetAngle, flipProgress);
            rectTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            if (flipProgress >= 1f)
            {
                isFlipping = false;
                flipProgress = 0f;
                currentTarget = (currentTarget == pointA) ? pointB : pointA;
                isFacingRight = !isFacingRight;
            }
        }
        else
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                currentTarget,
                moveSpeed * Time.deltaTime
            );
            if (Vector2.Distance(rectTransform.anchoredPosition, currentTarget) < 0.1f)
            {
                isFlipping = true;
            }
        }
    }
}