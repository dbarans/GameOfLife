using UnityEngine;
using TMPro;
using DG.Tweening;

public class AnimatedTextBlock : MonoBehaviour, ISequenceProvider
{
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private float objectFadeInDuration = 0.5f;
    [SerializeField] private float textAnimationDuration = 0.8f;
    [Range(1f, 50f)]
    [SerializeField] private float charactersPerSecond = 25f;
    [SerializeField] private float pauseAfterTextAnimation = 0.5f; 

    [Header("Object Fade Out")]
    [SerializeField] private bool includeObjectFadeOut = false;
    [SerializeField] private float objectFadeOutDuration = 0.5f;
    [SerializeField] private float pauseAfterObjectFadeOut = 0.5f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void InitializeState()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (targetText != null)
        {
            targetText.alpha = 0f;
            targetText.maxVisibleCharacters = 0;
        }
    }

    public Sequence GetAnimationSequence()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetAutoKill(false);

        // --- Object Fade In ---
        if (canvasGroup != null)
        {
            sequence.Append(canvasGroup.DOFade(1f, objectFadeInDuration));
            sequence.AppendCallback(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true; 
            });
        }
        else
        {
            sequence.AppendInterval(0.01f);
        }

        // --- Text Animation ---
        if (targetText != null)
        {
            sequence.Append(targetText.DOFade(1f, textAnimationDuration));
            sequence.Join(TextAnimator.AnimateTextByCharactersPerSecond(targetText, charactersPerSecond));
        }
        else
        {
            sequence.AppendInterval(0.01f);
        }

        sequence.AppendInterval(pauseAfterTextAnimation);

        // --- Optional Object Fade Out ---
        if (includeObjectFadeOut)
        {
            if (canvasGroup != null)
            {
                sequence.AppendCallback(() => {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                });
                sequence.Append(canvasGroup.DOFade(0f, objectFadeOutDuration));
            }
            else
            {
                sequence.AppendInterval(0.01f);
            }
            sequence.AppendInterval(pauseAfterObjectFadeOut);
        }

        return sequence;
    }
}