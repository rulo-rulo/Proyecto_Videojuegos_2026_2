using UnityEngine;
using UnityEngine.UI;

namespace Possession
{
    public class UI_PossessionSlider : MonoBehaviour
    {
        [SerializeField] private Slider            timeSlider;
        [SerializeField] private PossessionManager possessionManager;
        [SerializeField] private PossessableObject thisObject;

        private CanvasGroup canvasGroup;

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        void Update()
        {
            if (possessionManager == null || timeSlider == null || thisObject == null) return;

            bool esPoseido = possessionManager.CurrentState == PossessionState.Possessing
                          && possessionManager.CurrentTarget as PossessableObject == thisObject;

            if (esPoseido)
            {
                timeSlider.maxValue = possessionManager.PossessionDuration;
                timeSlider.value    = possessionManager.PossessionDuration - possessionManager.PossessionTimer;
                canvasGroup.alpha   = 1f;
            }
            else
            {
                canvasGroup.alpha   = 0f;
                timeSlider.value    = 0f;
            }
        }
    }
}