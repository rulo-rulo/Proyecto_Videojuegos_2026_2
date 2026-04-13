using UnityEngine;
using TMPro;

namespace Possession
{
    public class PossessionHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private PossessionManager possessionManager;

        private PossessionState lastState;

        private void Update()
        {
            PossessionState currentState = possessionManager.CurrentState;

            if (currentState == lastState) return;

            lastState = currentState;

            switch (currentState)
            {
                case PossessionState.Free:
                    hintText.text = "Poseer: Z";
                    break;

                case PossessionState.Scanning:
                    hintText.text = "Confirmar: Z\nCancelar: Backspace";
                    break;

                case PossessionState.Possessing:
                    hintText.text = "Salir: Z";
                    break;
            }
        }
    }
}
