using DG.Tweening;
using UnityEngine;


public class ButtonPanelSlider : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 normalPosition;
    private Vector2 extendedBookPosition = new Vector2(0f, 975f);
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject slideInButton;
    private SlideState slideState = SlideState.Hidden;
    private ExtendedState extendedState = ExtendedState.NotExtended;
    public SlideState GetSlideState()
    {
        return slideState;
    }
    public ExtendedState GetExtendedState()
    { 
        return extendedState;
    }

    public enum SlideState
    {
        Normal,
        Hidden,
        Extended, 

    }
    public enum ExtendedState
    {
        NotExtended,
        PatternsBook,
        MenuPanel
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        normalPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void SlideToHidden(System.Action onComplete = null)
    {
        if (slideState == SlideState.Hidden)
        {
            onComplete?.Invoke();
            return;
        }
        rectTransform.DOKill();
        rectTransform.DOAnchorPos(hiddenPosition, animationDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                //slideInButton.SetActive(true);
                slideState = SlideState.Hidden;
                extendedState = ExtendedState.NotExtended;
                onComplete?.Invoke();
            });
    }

    public void SlideToNormal()
    {
        if (slideState == SlideState.Normal) return;
        rectTransform.DOKill();
        //slideInButton.SetActive(false);
        rectTransform.DOAnchorPos(normalPosition, animationDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                slideState = SlideState.Normal;
                extendedState = ExtendedState.NotExtended;
            });
    }

    public void SlideOut()
    {
        if (slideState == SlideState.Extended)
        {
            SlideToNormal();
            return;
        }
        SlideToHidden(null);
    }

    public void SlideIn()
    {
        SlideToNormal();
    }

    public void SlideToExtended(ExtendedState state)
    {
        if (slideState == SlideState.Extended)
        {
            extendedState = state;
            return;
        }
        rectTransform.DOKill();
        rectTransform.DOAnchorPos(extendedBookPosition, animationDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                slideState = SlideState.Extended;
                extendedState = state;
            });
    }

    public bool IsHidden()
    {
        return (slideState == SlideState.Hidden);
    }


}
