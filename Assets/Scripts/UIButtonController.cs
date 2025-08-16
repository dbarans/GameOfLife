using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Button speedSliderBackButton;

    private List<Button> buttons = new List<Button>();
    private Vector3 originalButtonScale;

    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite pauseSprite;

    private void Awake()
    {
        buttons.Add(startButton);
        buttons.Add(resetButton);
        buttons.Add(speedButton);
        buttons.Add(saveButton);
        buttons.Add(loadButton);

        originalButtonScale = startButton.transform.localScale;
    }

    public void StartGame()
    {
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

    public void ChangeSpeed()
    {
        gameController.ChangeSpeed((int)speedSlider.value);
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

    public void ShowButtons()
    {
        foreach (Button button in buttons)
        {
            ShowElementAnimated(button.gameObject);
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

        foreach (Button button in buttons)
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
            ShowElementAnimated(speedSlider.gameObject);
            ShowElementAnimated(speedSliderBackButton.gameObject);
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
            HideGameObjectAnimated(element, () =>
            {
                completedAnimationsCount++;
                if (completedAnimationsCount == totalElementsToHide)
                {
                    ShowButtons();
                }
            });
        }
    }

    private void HideGameObjectAnimated(GameObject targetObject, Action onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();
        float duration = 0.15f;

        if (targetObject.TryGetComponent<Button>(out Button button))
        {
            button.interactable = false;
        }
        else if (targetObject.TryGetComponent<Slider>(out Slider slider))
        {
            slider.interactable = false;
        }

        sequence.Append(targetObject.transform.DOLocalMove(targetObject.transform.parent.localPosition, duration)).SetEase(Ease.InOutSine);
        sequence.Join(targetObject.transform.DOScale(Vector3.zero, duration)).SetEase(Ease.InOutSine);
        sequence.OnComplete(() =>
        {
            targetObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void ShowElementAnimated(GameObject targetObject)
    {
        targetObject.SetActive(true);
        targetObject.transform.localScale = Vector3.zero;

        targetObject.transform.DOScale(originalButtonScale, 0.15f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                if (targetObject.TryGetComponent<Button>(out Button button))
                {
                    button.interactable = true;
                }
                else if (targetObject.TryGetComponent<Slider>(out Slider slider))
                {
                    slider.interactable = true;
                }
            });
    }
}