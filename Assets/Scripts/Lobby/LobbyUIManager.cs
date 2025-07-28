using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button createButton;
    public Button backButton;
    public Button closeFullRoomButton;
    public Button closeDisbandRoomButton;

    [Header("GameObject")]
    public GameObject fullRoom;
    public GameObject disbandRoom;

    private float duration = 0.3f;

    void Start()
    {
        createButton.onClick.AddListener(OnOnlineRoomSelected);
        backButton.onClick.AddListener(OnReturnToTitleSelected);

        closeFullRoomButton.onClick.AddListener(() => ClosePop(fullRoom));
        closeDisbandRoomButton.onClick.AddListener(() => ClosePop(disbandRoom));

        if (StaticEvents.Dissolution) {
            OpenPop(disbandRoom);
            StaticEvents.Dissolution = false;
        }
    }

    public void OpenFullRoomPop() {
        OpenPop(fullRoom);
    }

    public void OpenPop(GameObject popUp)
    {
        StartCoroutine(FadeIn(popUp));
    }

    IEnumerator FadeIn(GameObject popUp)
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

    public void ClosePop(GameObject popUp)
    {
        StartCoroutine(FadeOut(popUp));
    }

    IEnumerator FadeOut(GameObject popUp)
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

    public void OnOnlineRoomSelected()
    {
        StaticEvents.playerStat = "Host";
        StaticEvents.hostIP = StaticEvents.GetLocalIPAddress();
        GameEvents.UIEvents.OnOnlineRoomEnter?.Invoke();
    }

    public void OnReturnToTitleSelected()
    {
        GameEvents.UIEvents.OnReturnToTitleScene?.Invoke();
    }
}
