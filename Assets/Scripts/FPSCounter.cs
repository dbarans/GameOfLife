using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;
    private float fps = 0.0f;
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
            fpsText.text = fps.ToString("F0");
        }
    }

}

