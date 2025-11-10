using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PatternButtonPrefab : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI dimensionsText;

    private PatternData patternData;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetupPattern(PatternData pattern)
    {
        patternData = pattern;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (patternData == null) return;

        if (nameText != null)
            nameText.text = $"{patternData.Id}. {patternData.Name}";

        if (authorText != null)
            authorText.text = string.IsNullOrEmpty(patternData.Author) ? "Unknown" : patternData.Author;

        if (dimensionsText != null)
            dimensionsText.text = $"{patternData.Width}×{patternData.Height}";
    }

    public PatternData GetPatternData()
    {
        return patternData;
    }
}

