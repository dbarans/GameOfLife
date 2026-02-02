using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;
    private float fps = 0.0f;

    [SerializeField] private TextMeshProUGUI genCounterText;
    private void Awake()
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
    public void SetGenerationsPerSecond(int gps)
    {
        if (genCounterText != null)
            genCounterText.text = gps + " GPS";
    }

}

