using UnityEngine;
using UnityEngine.EventSystems;
using static GameEvents;


public class TitleUIController : MonoBehaviour
{
    public GameObject firstSelectedButton;
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
    public void OnLocalPlaySelected() => UIEvents.OnLocalStart?.Invoke();
    public void OnOnlinePlaySelected() => UIEvents.OnOnlineStart?.Invoke();
    public void OnExitGameSelected() => UIEvents.OnGameClose?.Invoke();
}
