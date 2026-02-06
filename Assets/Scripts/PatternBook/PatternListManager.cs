using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternListManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform patternListParent;
    [SerializeField] private GameObject patternButtonPrefab;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TextMeshProUGUI pageInfoText;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private GameObject patternsBook;

    [Header("Settings")]
    [SerializeField] private int patternsPerPage = 10;

    private int currentPage = 1;
    private int totalPages;
    private PatternData[] currentPagePatterns;
    private List<GameObject> currentPatternButtons = new List<GameObject>();

    public event Action<PatternData> OnPatternSelected;

    private void Start()
    {
        InitializeUI();
        patternsBook.SetActive(false); 
    }

    private void InitializeUI()
    {
        totalPages = PatternLoader.GetTotalPages(patternsPerPage);
        
        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(PreviousPage);
        
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);
        
        // if (backToMenuButton != null)
        //     backToMenuButton.onClick.AddListener(BackToMenu);

        UpdateNavigationButtons();
    }

    public void LoadPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > totalPages)
            return;

        currentPage = pageNumber;
        currentPagePatterns = PatternLoader.GetPatternsPage(pageNumber, patternsPerPage);
        
        ClearCurrentButtons();
        CreatePatternButtons();
        UpdateNavigationButtons();
        UpdatePageInfo();
    }

    private void ClearCurrentButtons()
    {
        foreach (GameObject button in currentPatternButtons)
        {
            if (button != null)
                Destroy(button);
        }
        currentPatternButtons.Clear();
    }

    private void CreatePatternButtons()
    {
        if (patternButtonPrefab == null || patternListParent == null)
        {
            Debug.LogError("Pattern button prefab or parent not assigned!");
            return;
        }

        for (int i = 0; i < currentPagePatterns.Length; i++)
        {
            PatternData pattern = currentPagePatterns[i];
            GameObject buttonObj = Instantiate(patternButtonPrefab, patternListParent);
            currentPatternButtons.Add(buttonObj);

            // Setup button
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int patternIndex = i; // Capture for closure
                button.onClick.AddListener(() => OnPatternButtonClicked(patternIndex));
            }

            // Setup text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{pattern.Id}. {pattern.Name}";
            }

            // Setup tooltip or additional info
            PatternButtonInfo patternInfo = buttonObj.GetComponent<PatternButtonInfo>();
            if (patternInfo != null)
            {
                patternInfo.SetPatternData(pattern);
            }
        }
    }

    private void OnPatternButtonClicked(int patternIndex)
    {
        if (patternIndex >= 0 && patternIndex < currentPagePatterns.Length)
        {
            PatternData selectedPattern = currentPagePatterns[patternIndex];
            OnPatternSelected?.Invoke(selectedPattern);
            Debug.Log($"Selected pattern: {selectedPattern.Name} (ID: {selectedPattern.Id})");
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            LoadPage(currentPage - 1);
        }
        else
        {
            LoadPage(totalPages);
        }
    }

    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            LoadPage(currentPage + 1);
        }
        else
        {
            LoadPage(1);
        }
    }

    private void BackToMenu()
    {
        // This will be handled by the parent UI system
        //gameObject.SetActive(false);
    }

    private void UpdateNavigationButtons()
    {
        if (previousPageButton != null)
            previousPageButton.interactable = true;
        
        if (nextPageButton != null)
            nextPageButton.interactable = true;
    }

    private void UpdatePageInfo()
    {
        if (pageInfoText != null)
        {
            pageInfoText.text = $" {currentPage} / {totalPages} ";
        }
    }

    public void Show()
    {
        patternsBook.SetActive(true);
        LoadPage(currentPage); // Refresh current page
    }

    public void Hide()
    {
        patternsBook.SetActive(false);
    }
}
