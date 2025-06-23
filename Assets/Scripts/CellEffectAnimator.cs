using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CellEffectAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float initialVisibleDuration = 1.0f;
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private float pauseAfterAnimation = 0.5f;

    [SerializeField] private bool startsVisibleAndFades = true;

    private Sequence currentAnimationSequence;

    void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        if (targetImage == null)
        {
            Debug.LogError("No Image component found or assigned!");
            enabled = false;
        }
    }

    void OnEnable()
    {
        SetupInitialState();
        PlayAnimationLoop();
    }

    void OnDisable()
    {
        currentAnimationSequence?.Kill();
    }

    void SetupInitialState()
    {
        if (startsVisibleAndFades)
        {
            targetImage.color = Color.white;
        }
        else
        {
            targetImage.color = Color.black;
        }
    }

    void PlayAnimationLoop()
    {
        currentAnimationSequence?.Kill();

        currentAnimationSequence = DOTween.Sequence();

        if (startsVisibleAndFades)
        {
            currentAnimationSequence.AppendInterval(initialVisibleDuration);
            currentAnimationSequence.Append(targetImage.DOColor(Color.black, animationDuration).SetEase(Ease.OutQuad));
            currentAnimationSequence.AppendInterval(pauseAfterAnimation);
            currentAnimationSequence.AppendCallback(() => targetImage.color = Color.white);
        }
        else
        {
            currentAnimationSequence.AppendInterval(initialVisibleDuration);
            currentAnimationSequence.Append(targetImage.DOColor(Color.white, animationDuration).SetEase(Ease.OutQuad));
            currentAnimationSequence.AppendInterval(pauseAfterAnimation);
            currentAnimationSequence.AppendCallback(() => targetImage.color = Color.black);
        }

        currentAnimationSequence.SetLoops(-1);
    }
}