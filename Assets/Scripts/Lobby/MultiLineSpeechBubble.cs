using UnityEngine;

public class MultiLineSpeechBubble : MonoBehaviour
{
    [Header("Bubble Shape")]
    public int pointCount = 120;
    public float radiusX = 1.5f;
    public float radiusY = 1.2f;

    [Header("Tail")]
    public float tailWidth = 0.4f;
    public float tailLength = 0.5f;
    public float tailOffsetAngle = 240f;

    [Header("Noise & Line")]
    public int lineLayerCount = 6;
    public Material lineMaterial;
    public float lineWidth = 0.02f;
    public float noiseStrength = 0.05f;
    public float noiseSpeed = 1f;

    private LineRenderer[] lineRenderers;
    private Vector3[] basePath;

    void Start()
    {
        basePath = GenerateBubblePathWithTail(pointCount);
        lineRenderers = new LineRenderer[lineLayerCount];

        for (int i = 0; i < lineLayerCount; i++)
        {
            GameObject lrObj = new GameObject("Line_" + i);
            lrObj.transform.parent = transform;

            LineRenderer lr = lrObj.AddComponent<LineRenderer>();
            lr.positionCount = pointCount;
            lr.loop = true;
            lr.material = lineMaterial;
            lr.widthMultiplier = lineWidth;
            lr.useWorldSpace = true;

            lineRenderers[i] = lr;
        }
    }

    void Update()
    {
        float time = Time.time * noiseSpeed;
        for (int i = 0; i < lineLayerCount; i++)
        {
            float seed = i * 100f;
            Vector3[] noisyPoints = new Vector3[pointCount];
            for (int j = 0; j < pointCount; j++)
            {
                Vector3 p = basePath[j];
                float offsetX = (Mathf.PerlinNoise(j * 0.1f + seed, time) - 0.5f) * noiseStrength;
                float offsetY = (Mathf.PerlinNoise(j * 0.1f + seed + 100f, time + 50f) - 0.5f) * noiseStrength;
                noisyPoints[j] = transform.position + p + new Vector3(offsetX, offsetY, 0f);
            }
            lineRenderers[i].SetPositions(noisyPoints);
        }
    }

    Vector3[] GenerateBubblePathWithTail(int count)
    {
        Vector3[] path = new Vector3[count];
        float tailAngleRad = tailOffsetAngle * Mathf.Deg2Rad;
        float angleStep = Mathf.PI * 2f / count;

        int tailStartIndex = Mathf.RoundToInt(count * (tailOffsetAngle / 360f));
        int tailLengthIndex = Mathf.RoundToInt(count * 0.03f);

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radiusX;
            float y = Mathf.Sin(angle) * radiusY;
            if (i >= tailStartIndex && i <= tailStartIndex + tailLengthIndex)
            {
                float t = Mathf.InverseLerp(tailStartIndex, tailStartIndex + tailLengthIndex, i);
                x -= Mathf.Sin(t * Mathf.PI) * tailWidth;
                y -= Mathf.Sin(t * Mathf.PI) * tailLength;
            }
            path[i] = new Vector3(x, y, 0f);
        }
        return path;
    }
}
