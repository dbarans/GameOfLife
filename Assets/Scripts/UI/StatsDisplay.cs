using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;
    private float fps = 0.0f;

    private float measurementTimer = 0f;
    private int generationsCounted = 0;
    private const float MEASUREMENT_INTERVAL = 3.0f;
    private float currentGenerationsPerSecond = 0f;
    [SerializeField] private TextMeshProUGUI genCounterText;
    private void Start()
    {
        float refreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
        Application.targetFrameRate = !float.IsNaN(refreshRate) ? (int)refreshRate : 60;
    }
    void Update()
    {

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;

        if (fpsText != null)
        {
            fpsText.text = fps.ToString("F0")+ " FPS";
        }
    }
    public void UpdateGenerationsPerSecondDisplay()
    {
        measurementTimer += Time.deltaTime;

        if (measurementTimer >= MEASUREMENT_INTERVAL)
        {
            currentGenerationsPerSecond = generationsCounted / measurementTimer;

            if (genCounterText != null)
            {
                genCounterText.text = currentGenerationsPerSecond.ToString("F0") + " GPS";
            }

            measurementTimer = 0f;
            generationsCounted = 0;
        }
    }
    public void IncrementGenerationCount()
    {
        generationsCounted += 1;
    }
    
    public void AddCalculatedGenerations(int count)
    {
        generationsCounted += count;
    }

}

