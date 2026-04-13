using UnityEngine;
using UnityEngine.UI;

public class UI_Slider : MonoBehaviour
{
    public Slider         timeSlider;
    public VisionCone     visionCone;
    public DetectorCamara detectorCamara;

    private CanvasGroup canvasGroup;
    private float       tiempoDeteccion;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (visionCone != null)
            tiempoDeteccion = visionCone.tiempoDeteccion;
        else if (detectorCamara != null)
            tiempoDeteccion = detectorCamara.tiempoDeteccion;

        if (timeSlider != null)
        {
            timeSlider.maxValue = tiempoDeteccion;
            timeSlider.value    = tiempoDeteccion;
        }

        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (timeSlider == null) return;

        bool   detected = false;
        float  timer    = 0f;

        if (visionCone != null)
        {
            detected = visionCone.playerDetected;
            timer    = visionCone.DetectionTimer;
        }
        else if (detectorCamara != null)
        {
            detected = detectorCamara.PlayerDetected;
            timer    = detectorCamara.DetectionTimer;
        }

        if (detected)
        {
            canvasGroup.alpha  = 1f;
            timeSlider.value   = tiempoDeteccion - timer;
        }
        else
        {
            canvasGroup.alpha  = 0f;
            timeSlider.value   = tiempoDeteccion;
        }
    }
}