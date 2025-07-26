using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionBox;
    [SerializeField] private Button yesAnswerButton;
    [SerializeField] private Button noAnswerButton;
    [SerializeField] private TextMeshProUGUI yesAnswerText;
    [SerializeField] private TextMeshProUGUI noAnswerText;
    [SerializeField] private float charactersPerSecond = 30f;


    public enum WelcomeSceneState
    {
        Question,
        ShowYesAnswer,
        ShowNoAnswer,
        EnableButtons,
        End
    }
    public enum StatePhase
    {
        Start,
        Update
    }

    private WelcomeSceneState currentState = WelcomeSceneState.Question;
    private StatePhase currentPhase = StatePhase.Start;

    private void Start()
    {
        yesAnswerButton.interactable = false;
        noAnswerButton.interactable = false;
        yesAnswerText.maxVisibleCharacters = 0;
        noAnswerText.maxVisibleCharacters = 0;
        questionBox.maxVisibleCharacters = 0;
    }

    private void Update()
    {
        switch (currentState)
        {
            case WelcomeSceneState.Question:
                HandleQuestionState();
                break;

            case WelcomeSceneState.ShowYesAnswer:
                HandleYesAnswerState();
                break;

            case WelcomeSceneState.ShowNoAnswer:
                HandleNoAnswerState();
                break;

            case WelcomeSceneState.EnableButtons:
               HandleEnableButonsState();
                break;
        }
    }
    private void HandleQuestionState()
    {
        switch (currentPhase)
        {
            case StatePhase.Start:
                Tween messageTween = TextAnimator.AnimateTextByCharactersPerSecond(questionBox, charactersPerSecond);
                messageTween.OnComplete(() =>
                {
                    currentState = WelcomeSceneState.ShowYesAnswer;
                    currentPhase = StatePhase.Start;
                });
                currentPhase = StatePhase.Update;
                break;

        }
    }
    private void HandleYesAnswerState()
    {
        switch (currentPhase)
        {
            case StatePhase.Start:
                Tween yesTween = TextAnimator.AnimateTextByCharactersPerSecond(yesAnswerText, charactersPerSecond);
                yesTween.OnComplete(() =>
                {
                    currentState = WelcomeSceneState.ShowNoAnswer;
                    currentPhase = StatePhase.Start;
                });
                currentPhase = StatePhase.Update;
                break;
        }
    }

    private void HandleNoAnswerState()
    {
        switch (currentPhase)
        {
            case StatePhase.Start:
                Tween noTween = TextAnimator.AnimateTextByCharactersPerSecond(noAnswerText, charactersPerSecond);
                noTween.OnComplete(() =>
                {
                    currentState = WelcomeSceneState.EnableButtons;
                    currentPhase = StatePhase.Start;
                });
                currentPhase = StatePhase.Update;
                break;
        }
    }
    private void HandleEnableButonsState()
    {
        switch (currentPhase)
        {
            case StatePhase.Start:
                yesAnswerButton.interactable = true;
                noAnswerButton.interactable = true;
                currentState = WelcomeSceneState.End;
                currentPhase = StatePhase.Start;
                break;
        }
    }

}
