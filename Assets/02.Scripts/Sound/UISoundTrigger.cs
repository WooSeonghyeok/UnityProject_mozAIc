using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("재생 여부")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playHoverSound = true;
    [Header("UI 사운드 타입")]
    [SerializeField] private SoundManager.UIType clickSound = SoundManager.UIType.Click;
    [SerializeField] private SoundManager.UIType hoverSound = SoundManager.UIType.Hover;
    [Header("볼륨 배율")]
    [SerializeField][Range(0f, 1f)] private float clickVolumeScale = 1f;
    [SerializeField][Range(0f, 1f)] private float hoverVolumeScale = 0.6f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound) return;
        if (SoundManager.Instance == null) return;
        if (hoverSound == SoundManager.UIType.None) return;
        SoundManager.Instance.PlayUI(hoverSound, hoverVolumeScale);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!playClickSound) return;
        if (SoundManager.Instance == null) return;
        if (clickSound == SoundManager.UIType.None) return;
        SoundManager.Instance.PlayUI(clickSound, clickVolumeScale);
    }
    public void PlayClickSound()
    {
        if (SoundManager.Instance == null) return;
        if (clickSound == SoundManager.UIType.None) return;
        SoundManager.Instance.PlayUI(clickSound, clickVolumeScale);
    }
    public void PlayHoverSound()
    {
        if (SoundManager.Instance == null) return;
        if (hoverSound == SoundManager.UIType.None) return;
        SoundManager.Instance.PlayUI(hoverSound, hoverVolumeScale);
    }
}