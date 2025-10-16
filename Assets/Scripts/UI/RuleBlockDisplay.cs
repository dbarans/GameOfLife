using UnityEngine;
using TMPro;
using DG.Tweening;

public class RuleBlockDisplay : MonoBehaviour, ISequenceProvider
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI ruleTitleText;
    [SerializeField] private TextMeshProUGUI ruleDescriptionText;

    [Range(1f, 50f)]
    [SerializeField] private float titleCharactersPerSecond = 20f;
    [Range(1f, 50f)]
    [SerializeField] private float descriptionCharactersPerSecond = 15f;
    [SerializeField] private float overallFadeInDuration = 0.5f;
    [SerializeField] private float delayBetweenTitleAndDescription = 0.1f;
    [SerializeField] private float pauseAfterThisElement = 1.0f;

    public CanvasGroup CanvasGroup => canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("RuleBlockDisplay requires a CanvasGroup component!", this);
            }
        }
    }

    public void InitializeState()
    {
        gameObject.SetActive(false);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (ruleTitleText != null)
        {
            ruleTitleText.alpha = 1f;
            ruleTitleText.maxVisibleCharacters = 0;
        }
        if (ruleDescriptionText != null)
        {
            ruleDescriptionText.alpha = 1f;
            ruleDescriptionText.maxVisibleCharacters = 0;
        }
    }

    public Sequence GetAnimationSequence()
    {
        Sequence blockSequence = DOTween.Sequence();
        blockSequence.SetAutoKill(false);

        blockSequence.AppendCallback(() => gameObject.SetActive(true));
        blockSequence.Append(canvasGroup.DOFade(1f, overallFadeInDuration));

        if (ruleTitleText != null)
        {
            blockSequence.Append(TextAnimator.AnimateTextByCharactersPerSecond(ruleTitleText, titleCharactersPerSecond));
        }

        blockSequence.AppendInterval(delayBetweenTitleAndDescription);

        if (ruleDescriptionText != null)
        {
            blockSequence.Append(TextAnimator.AnimateTextByCharactersPerSecond(ruleDescriptionText, descriptionCharactersPerSecond));
        }

        blockSequence.AppendInterval(pauseAfterThisElement);

        return blockSequence;
    }
}