using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesController : MonoBehaviour
{
    [SerializeField] private GameObject rulesContainer;
    [SerializeField] private PresentationFlowManager rulesPresentationManager;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private Button playButton;
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private const string GAME_SCENE_NAME = "Game";


    public enum RuleState
    {
        Rules,
        Button,
        End
    }
    private enum StatePhase
    {
        Start,
        Update,
        End
    }

    private RuleState ruleState = RuleState.Rules;
    private StatePhase statePhase = StatePhase.Start;


    private void Start()
    {
        playButtonText.maxVisibleCharacters = 0;
        playButton.interactable = false;
    }
    private void Update()
    {
        switch (ruleState)
        {
            case RuleState.Rules:
                HandleRulesState();
                break;
            case RuleState.Button:
                HandleButtonState();
                break;
        }
    }
    private void HandleRulesState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:

                rulesContainer.SetActive(true);
                rulesPresentationManager.StartPresentation(() => {
                    statePhase = StatePhase.End;
                });

                statePhase = StatePhase.Update;
                break;

            case StatePhase.End:
                
                ruleState = RuleState.Button;
                statePhase = StatePhase.Start;
                break;
        }
    }
    private void HandleButtonState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                Tween textButtonTween = TextAnimator.AnimateTextByCharactersPerSecond(playButtonText, charactersPerSecond);
                textButtonTween.OnComplete(() =>
                {
                    playButton.interactable = true;
                });
                statePhase = StatePhase.End;
                break;
        }

    }
    public void OnPlayButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(GAME_SCENE_NAME);
    }
}

