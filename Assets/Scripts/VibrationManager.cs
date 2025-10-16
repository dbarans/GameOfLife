using Effects;

public class VibrationManager : IVibrationManager
{
    private const long DefaultGenerationDurationMs = 30;
    private const int DefaultGenerationAmplitude = 120;

    private const long DefaultTextLetterDurationMs = 30;
    private const int DefaultTextLetterAmplitude = 80;

    private const long DefaultTouchDurationMs = 10;
    private const int DefaultTouchAmplitude = 40;

    public void VibrateOnGeneration()
    {
        Vibration.Vibrate(DefaultGenerationDurationMs, DefaultGenerationAmplitude);
    }

    public void VibrateOnTextLetter()
    {
        Vibration.Vibrate(DefaultTextLetterDurationMs, DefaultTextLetterAmplitude);
    }
    public void VibrateOnTouch()
    {
        Vibration.Vibrate(DefaultTouchDurationMs, DefaultTouchAmplitude);
    }
}
