using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class ButtonPanelSlider : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 extendedPosition;
    private Vector2 patternsBookPosition = new Vector2(0f, 975f);
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject slideInButton;
    private SlideState slideState = SlideState.Hidden;
    public SlideState GetSlideState()
    {
        return slideState;
    }

    public enum SlideState
    {
        Extended,
        Hidden,
        PatternsBook
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        extendedPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void SlideOut()
    {
        if (slideState == SlideState.PatternsBook)
        {
            SlideFromPatternsBook();
            return;
        }
        else
        {

            rectTransform.DOAnchorPos(hiddenPosition, animationDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                slideInButton.SetActive(true);
                slideState = SlideState.Hidden;
            });
        } 
    }
    public void SlideIn()
    {
        slideInButton.SetActive(false);
        rectTransform.DOAnchorPos(extendedPosition, animationDuration).SetEase(Ease.OutBack);
        slideState = SlideState.Extended;
    }

    public void SlideToPatternsBook()
    {
        rectTransform.DOAnchorPos(patternsBookPosition, animationDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                slideState = SlideState.PatternsBook;
            });
    }
    public void SlideFromPatternsBook()
    {
        rectTransform.DOAnchorPos(extendedPosition, animationDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                slideState = SlideState.Extended;
            });
    }

    public bool IsExtended()
    {
        return (slideState == SlideState.Extended || slideState == SlideState.PatternsBook);
    }


}
