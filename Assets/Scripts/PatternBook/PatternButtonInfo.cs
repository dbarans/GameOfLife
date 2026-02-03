using UnityEngine;
using TMPro;

public class PatternButtonInfo : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI dimensionsText;

    private PatternData patternData;

    public void SetPatternData(PatternData pattern)
    {
        patternData = pattern;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (patternData == null) return;

        if (nameText != null)
            nameText.text = patternData.Name;

        if (authorText != null)
            authorText.text = string.IsNullOrEmpty(patternData.Author) ? "Unknown author" : patternData.Author;



        if (dimensionsText != null)
            dimensionsText.text = $"{patternData.Width}×{patternData.Height}";
    }

    public PatternData GetPatternData()
    {
        return patternData;
    }
}

