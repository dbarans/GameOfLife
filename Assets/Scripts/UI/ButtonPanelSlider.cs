using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ButtonPanelSlider : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 extendedPosition;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject slideInButton;
    private bool isExtended = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        extendedPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void SlideOut()
    {
        rectTransform.DOAnchorPos(hiddenPosition, animationDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                slideInButton.SetActive(true);
                isExtended = false;
            });
        
    }
    public void SlideIn()
    {
        slideInButton.SetActive(false);
        rectTransform.DOAnchorPos(extendedPosition, animationDuration).SetEase(Ease.OutBack);
        isExtended = true;
    }

    public bool IsExtended()
    {
        return isExtended;
    }


}
