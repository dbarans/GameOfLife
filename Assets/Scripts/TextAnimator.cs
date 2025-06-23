using UnityEngine;
using TMPro;
using DG.Tweening;
using Effects;


public static class TextAnimator
{
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
                Vibration.Vibrate(30, 80);

                lastVisibleCharacterCount = textMesh.maxVisibleCharacters;
            }
        });
    }
}