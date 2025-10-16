using TMPro;
using DG.Tweening;
using UnityEngine;



public static class TextAnimator
{
    private static IVibrationManager vibrationManager;
    public static void Initialize(IVibrationManager manager)
    {
        vibrationManager = manager;
    }
    public static Tween AnimateTextByCharactersPerSecond(TextMeshProUGUI textMesh, float charactersPerSecond)
    {
        if (textMesh == null || charactersPerSecond <= 0 || string.IsNullOrEmpty(textMesh.text))
        {
            return DOTween.Sequence();
        }

        textMesh.maxVisibleCharacters = 0;
        float duration = textMesh.text.Length / charactersPerSecond;
        int lastVisibleCharacterCount = 0;

        return DOTween.To(
            () => textMesh.maxVisibleCharacters,
            x => textMesh.maxVisibleCharacters = x,
            textMesh.text.Length,
            duration
        )
        .SetEase(Ease.Linear)
        .OnUpdate(() => {
            if (textMesh.maxVisibleCharacters > lastVisibleCharacterCount)
            {
                if (vibrationManager != null)
                {
                    vibrationManager.VibrateOnTextLetter();
                }
                else
                {
                    Debug.LogWarning("TextAnimator: IVibrationManager is not initialized.");
                }
                    lastVisibleCharacterCount = textMesh.maxVisibleCharacters;
            }
        });
    }
}