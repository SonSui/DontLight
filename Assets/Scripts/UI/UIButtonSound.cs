using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISelectHandler, ISubmitHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip selectSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        PlaySound(clickSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return;
        }
        PlaySound(selectSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}