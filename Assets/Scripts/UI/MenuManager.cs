using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public event Action TutorialRequested;

    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button rulesButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button coffeeButton;
    [SerializeField] private Button aboutGameButton;

    private const string COFFEE_URL = "https://buymeacoffee.com/dbarans";


    private void Start()
    {
        InitializeUI();
        menuPanel.SetActive(false);
    }
    private void InitializeUI()
    {
        tutorialButton.onClick.AddListener(OnTutorialButtonClicked);
        rulesButton.onClick.AddListener(OnRulesButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        coffeeButton.onClick.AddListener(OnCoffeeButtonClicked);
        aboutGameButton.onClick.AddListener(OnAboutGameButtonClicked);
    }

    public void Show()
    {
        menuPanel.SetActive(true);
    }

    public void Hide()
    {
        menuPanel.SetActive(false);
    }

    public void OnTutorialButtonClicked()
    {
        TutorialRequested?.Invoke();
    }
    public void OnRulesButtonClicked()
    {
        Debug.Log("Opening Rules Panel");
    }
    public void OnSettingsButtonClicked()
    {
        Debug.Log("Opening Settings Panel");
    }
    public void OnCoffeeButtonClicked()
    {
        Application.OpenURL(COFFEE_URL);
        Debug.Log("Opening Coffee Panel");
    }
    public void OnAboutGameButtonClicked()
    {
        Debug.Log("Opening About Game Panel");
    }

}

