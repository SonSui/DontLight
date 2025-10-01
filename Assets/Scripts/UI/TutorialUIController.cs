using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialUIController : MonoBehaviour
{
    [Tooltip("ゲームパッド選択ボタン")]
    public GameObject firstSelected;

    [Tooltip("後ろのボタン押し防止の透明画像")]
    public Image raycastBlocker;

    [Header("チュートリアルを開くと禁止するCanvas")]
    public List<CanvasGroup> backgroundGroups = new List<CanvasGroup>();

    private GameObject _prevSelected;
    private bool _isOpen = false;
    private readonly List<bool> _bgPrevInteractables = new List<bool>();

    [Header("チュートリアルイメージ")]
    public List<GameObject> tutorialImages = new List<GameObject>();
    private int _currentIndex = 0;

    private void OnEnable()
    {
        if (_isOpen) ForceFocusInside();
    }

    private void Update()
    {
        if (!_isOpen) return;

        var es = EventSystem.current;
        if (es == null) return;

        var cur = es.currentSelectedGameObject;

        if (cur == null || !IsChildOfModal(cur))
        {
            ForceFocusInside();
        }
    }

    public void Open()
    {
        var es = EventSystem.current;
        if (es == null)
        {
            Debug.LogWarning("Can't find EventSystem");
            return;
        }

        _prevSelected = es.currentSelectedGameObject;

        gameObject.SetActive(true);
        _isOpen = true;

        _currentIndex = 0;

        if (raycastBlocker != null) raycastBlocker.raycastTarget = true;

        _bgPrevInteractables.Clear();
        foreach (var cg in backgroundGroups)
        {
            if (cg == null) { _bgPrevInteractables.Add(true); continue; }
            _bgPrevInteractables.Add(cg.interactable);
            cg.interactable = false;
        }

        foreach (var img in tutorialImages)
        {
            if (img != null) img.SetActive(false);
        }
        if (tutorialImages.Count > 0 && tutorialImages[0] != null)
        {
            tutorialImages[0].SetActive(true);
        }

        SetSelectedSafe(firstSelected);
        
    }

    public void Close()
    {
        var es = EventSystem.current;

        if (raycastBlocker != null) raycastBlocker.raycastTarget = false;

        for (int i = 0; i < backgroundGroups.Count; i++)
        {
            var cg = backgroundGroups[i];
            if (cg == null) continue;
            bool prev = (i < _bgPrevInteractables.Count) ? _bgPrevInteractables[i] : true;
            cg.interactable = prev;
        }
        _bgPrevInteractables.Clear();

        _isOpen = false;
        gameObject.SetActive(false);

        if (es != null && _prevSelected != null)
        {
            SetSelectedSafe(_prevSelected);
        }

        _prevSelected = null;
    }

    private void ForceFocusInside()
    {
        if (!SetSelectedSafe(firstSelected))
        {
            var selectable = GetComponentInChildren<Selectable>(includeInactive: false);
            if (selectable != null) SetSelectedSafe(selectable.gameObject);
        }
    }

    private bool SetSelectedSafe(GameObject go)
    {
        var es = EventSystem.current;
        if (es == null || go == null) return false;
        if (!go.activeInHierarchy) return false;

        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(go);
        return true;
    }

    private bool IsChildOfModal(GameObject go)
    {
        if (go == null) return false;
        var t = go.transform;
        var root = transform;
        while (t != null)
        {
            if (t == root) return true;
            t = t.parent;
        }
        return false;
    }
    public void OnNextButton()
    {
        if(_currentIndex < 0 || _currentIndex >= tutorialImages.Count)
        {
            Close();
            return;
        }
        tutorialImages[_currentIndex].gameObject.SetActive(false);
        _currentIndex++;
        if (_currentIndex >= tutorialImages.Count) Close();
        else
        {
            tutorialImages[_currentIndex].gameObject.SetActive(true);
        }
    }
}
