using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button createButton;
    public Button backButton;
    public Button closeButton;
    public GameObject popUp;

    private float duration = 0.3f;

    void Start()
    {
        createButton.onClick.AddListener(CreateRoom);
        backButton.onClick.AddListener(BackToTitle);
        closeButton.onClick.AddListener(ClosePop);
    }

    public void CreateRoom()
    {
        StaticEvents.playerStat = "Host";
        StaticEvents.hostIP = StaticEvents.GetLocalIPAddress();
        SceneTransitionManager.Instance.LoadScene("RoomScene");
    }

    public void BackToTitle()
    {
        SceneTransitionManager.Instance.LoadScene("Title");
    }

    public void OpenPop()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        popUp.SetActive(true);
        CanvasGroup canvasGroup = popUp.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popUp.AddComponent<CanvasGroup>();
        }
        float startAlpha = 0;
        canvasGroup.alpha = startAlpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public void ClosePop()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = popUp.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popUp.AddComponent<CanvasGroup>();
        }
        float startAlpha = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            yield return null;
        }
        popUp.SetActive(false);
    }
}
