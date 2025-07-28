using UnityEngine;
using UnityEngine.UI;

public class TypingSpeechBubbleBubble : MonoBehaviour
{
    public string baseText;
    public string dots;
    public float dotInterval = 0.5f;
    public float pauseDuration = 1f;

    private Text speachContent;
    private int dotCount = 0;
    private float timer = 0f;

    void Start()
    {
        speachContent = GetComponent<Text>();
        speachContent.text = baseText;
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
    }
}