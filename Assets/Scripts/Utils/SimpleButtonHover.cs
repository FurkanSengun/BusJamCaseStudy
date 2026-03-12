using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;



[RequireComponent(typeof(Button))]
public class SimpleButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover (PC için)")]
    [SerializeField] private bool enableHover = true;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.18f;
    [SerializeField] private Ease hoverEase = Ease.OutBack;

    [Header("Press (PC + Mobile)")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float pressDuration = 0.12f;
    [SerializeField] private Ease pressEase = Ease.OutBack;

    [Header("Davranış")]
    [SerializeField] private bool revertOnDisable = true;

    private RectTransform rect;
    private Vector3 initialScale;
    private Tween currentTween;
    private bool isPointerDown = false;
    private bool isHovering = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        initialScale = rect.localScale;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!enableHover) return;
        if (isPointerDown) return; 
        PlayScale(initialScale * hoverScale, hoverDuration, hoverEase);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (isPointerDown) return;
        PlayScale(initialScale, hoverDuration, hoverEase);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        PlayScale(initialScale * pressScale, pressDuration, pressEase);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (isHovering && enableHover)
            PlayScale(initialScale * hoverScale, hoverDuration, hoverEase);
        else
            PlayScale(initialScale, hoverDuration, hoverEase);
    }

    private void PlayScale(Vector3 target, float duration, Ease ease)
    {
        currentTween?.Kill();
        currentTween = rect.DOScale(target, duration).SetEase(ease).SetUpdate(true);
    }

    void OnDisable()
    {
        currentTween?.Kill();
        if (revertOnDisable && rect != null)
            rect.localScale = initialScale;
        isPointerDown = false;
        isHovering = false;
    }
}
