using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PrepareUIManager : MonoBehaviour
{
    public GameObject prepareUI;
    public List<GameObject> spawnPoint = new List<GameObject>();

    public GameObject firstSelectedButton;
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private void OnEnable()
    {
        GameEvents.PrepareUIEvents.OnPlayerDataCreated += CreatePrepareUI;
    }
    private void OnDisable()
    {
        GameEvents.PrepareUIEvents.OnPlayerDataCreated -= CreatePrepareUI;
    }
    private void CreatePrepareUI(PlayerData data)
    {
        if (data.playerIndex < 0 || data.playerIndex >= spawnPoint.Count)
        {
            Debug.LogError("Invalid player index: " + data.playerIndex);
            return;
        }
        GameObject ui = Instantiate(prepareUI, spawnPoint[data.playerIndex].transform);
        ui.GetComponent<PreparePlayerUI>().SetPlayer(data);
    }

    public void OnLocalPlayStartSelected()
    {
        GameEvents.UIEvents.OnLocalGameStart?.Invoke();
    }
    public void OnReturnToTitleSelected()
    {
        GameEvents.UIEvents.OnReturnToTitleScene?.Invoke();
    }
    public void OnBulbCountButtonSelected(int count)
    {
        GameEvents.PrepareUIEvents.OnSetBulbCount?.Invoke(count);
    }
}
