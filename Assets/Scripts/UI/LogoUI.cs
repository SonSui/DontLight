using UnityEngine;
using UnityEngine.UI;

public class LogoUI : MonoBehaviour
{
    public float radius = 0.5f;
    public float speed = 1.0f;

    private float angle = 0.0f;
    private Vector3 position = Vector3.zero;
    private void Start()
    {
        position = GetComponent<RectTransform>().anchoredPosition;
    }

    void Update()
    {
        angle += speed * Time.deltaTime;
        GetComponent<RectTransform>().anchoredPosition = new Vector3(
            position.x,
            position.y+radius * Mathf.Sin(angle),
            position.z
        );
    }
}
