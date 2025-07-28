using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageBrightness : MonoBehaviour
{
    public Image targetImage;
    public float duration = 1.5f;

    private Color colorBright = Color.white; // #FFFFFF
    private Color colorDark = new Color(0.4745f, 0.4745f, 0.4745f); // #C1C1C1

    void Start()
    {
        if (targetImage != null)
            StartCoroutine(BreathingLoop());
    }

    private IEnumerator BreathingLoop()
    {
        while (true)
        {
            // 从亮 ➜ 暗
            yield return StartCoroutine(FadeToColor(colorDark));
            // 从暗 ➜ 亮
            yield return StartCoroutine(FadeToColor(colorBright));
        }
    }

    private IEnumerator FadeToColor(Color targetColor)
    {
        Color startColor = targetImage.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            targetImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        targetImage.color = targetColor;
    }
}
