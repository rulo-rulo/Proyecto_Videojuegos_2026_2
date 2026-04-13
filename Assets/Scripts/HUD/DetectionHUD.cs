using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DetectionHUD : MonoBehaviour
{
    public static DetectionHUD Instance;

    [SerializeField] private TextMeshProUGUI countdownText;

    private Dictionary<object, float> activeTimers = new Dictionary<object, float>();

    private void Awake()
    {
        Instance = this;
        countdownText.gameObject.SetActive(false);
    }

    public void ReportTimer(object enemy, float timeRemaining)
    {
        activeTimers[enemy] = timeRemaining;
        UpdateDisplay();
    }

    public void RemoveTimer(object enemy)
    {
        activeTimers.Remove(enemy);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (activeTimers.Count == 0)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        float lowest = float.MaxValue;
        foreach (float t in activeTimers.Values)
        {
            if (t < lowest) lowest = t;
        }

        countdownText.gameObject.SetActive(true);
        countdownText.text = lowest.ToString("F1");
    }
}