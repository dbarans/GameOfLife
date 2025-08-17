using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject menuPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button speedButton;

    [Header("Slider")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Button speedSliderBackButton;

    [Header("Sprites")]
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite pauseSprite;

    private Vector3 originalButtonScale;
    private Dictionary<ButtonType, Button> buttons;

    public event Action OnStartButtonClicked;

    public enum ButtonType { Start, Save, Load, Reset, Speed }

    private void Awake()
    {
        buttons = new Dictionary<ButtonType, Button>
        {
            { ButtonType.Start, startButton },
            { ButtonType.Save, saveButton },
            { ButtonType.Load, loadButton },
            { ButtonType.Reset, resetButton },
            { ButtonType.Speed, speedButton }
        };

        originalButtonScale = startButton.transform.localScale;
    }

    #region Game Control Buttons
    public void StartGame()
    {
        OnStartButtonClicked?.Invoke();
        gameController.RunGame();
        if (gameController.isRunning)
        {
            startButton.image.sprite = pauseSprite;
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(PauseGame);
        }
    }

    public void PauseGame()
    {
        gameController.PauseGame();
        startButton.image.sprite = startSprite;
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartGame);
    }

    public void ResetGame()
    {
        gameController.ResetGame();
        PauseGame();
    }

    public void SaveGame()
    {
        gameController.SaveGame();
    }

    public void LoadGame()
    {
        gameController.LoadGame();
    }

    public void ChangeSpeed()
    {
        gameController.ChangeSpeed((int)speedSlider.value);
    }
    #endregion

    #region Button State Control
    public void SetAllButtonsInteractable(bool interactable)
    {
        foreach (Button button in buttons.Values)
            button.interactable = interactable;
    }

    public void SetButtonInteractable(ButtonType type, bool interactable)
    {
        if (buttons.TryGetValue(type, out var button))
            button.interactable = interactable;
    }

    public void UpdateSaveLoadButtons(bool isGameRunning)
    {
        if (isGameRunning)
        {
            SetButtonInteractable(ButtonType.Save, false);
            SetButtonInteractable(ButtonType.Load, false);
        }
        else
        {
            SetButtonInteractable(ButtonType.Save, true);
            SetButtonInteractable(ButtonType.Load, true);
        }
    }
    #endregion

    #region Button & Slider UI Animations
    public void ShowButtons()
    {
        int completedAnimationsCount = 0;
        int totalButtons = buttons.Count;

        if (totalButtons == 0) return;

        foreach (Button button in buttons.Values)
        {
            ShowElementAnimated(button.gameObject, () =>
            {
                button.interactable = true;
                completedAnimationsCount++;
                if (completedAnimationsCount == totalButtons)
                {
                    UpdateSaveLoadButtons(gameController.isRunning);
                }
            });
        }
    }

    public void HideButtons(Action onAllHidden = null)
    {
        int completedAnimationsCount = 0;
        int totalButtons = buttons.Count;

        if (totalButtons == 0)
        {
            onAllHidden?.Invoke();
            return;
        }

        foreach (Button button in buttons.Values)
        {
            HideGameObjectAnimated(button.gameObject, () =>
            {
                completedAnimationsCount++;
                if (completedAnimationsCount == totalButtons)
                {
                    onAllHidden?.Invoke();
                }
            });
        }
    }

    public void ShowSpeedSlider()
    {
        HideButtons(() =>
        {
            ShowElementAnimated(speedSlider.gameObject, () => speedSlider.interactable = true);
            ShowElementAnimated(speedSliderBackButton.gameObject, () => speedSliderBackButton.interactable = true);
        });
    }

    public void HideSpeedSlider()
    {
        List<GameObject> elementsToHide = new List<GameObject>
        {
            speedSlider.gameObject,
            speedSliderBackButton.gameObject
        };

        int completedAnimationsCount = 0;
        int totalElementsToHide = elementsToHide.Count;

        if (totalElementsToHide == 0)
        {
            ShowButtons();
            return;
        }

        foreach (GameObject element in elementsToHide)
        {
            if (element.TryGetComponent<Button>(out Button button))
                button.interactable = false;
            else if (element.TryGetComponent<Slider>(out Slider slider))
                slider.interactable = false;

            HideGameObjectAnimated(element, () =>
            {
                completedAnimationsCount++;
                if (completedAnimationsCount == totalElementsToHide)
                    ShowButtons();
            });
        }
    }

    private void HideGameObjectAnimated(GameObject targetObject, Action onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();
        float duration = 0.15f;

        sequence.Append(targetObject.transform.DOLocalMove(targetObject.transform.parent.localPosition, duration))
            .SetEase(Ease.InOutSine);
        sequence.Join(targetObject.transform.DOScale(Vector3.zero, duration)).SetEase(Ease.InOutSine);
        sequence.OnComplete(() =>
        {
            targetObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void ShowElementAnimated(GameObject targetObject, Action onComplete = null)
    {
        targetObject.SetActive(true);
        targetObject.transform.localScale = Vector3.zero;

        targetObject.transform.DOScale(originalButtonScale, 0.15f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
    #endregion
}