using UnityEngine;
using UnityEngine.Events;

namespace Possession
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Teclado")]
        [SerializeField] private KeyCode possessionKey = KeyCode.Z;
        [SerializeField] private KeyCode cancelKey = KeyCode.Backspace;

        [Header("Mando")]
        [SerializeField] private KeyCode possessionMando = KeyCode.JoystickButton4;

        [SerializeField] private KeyCode cancelMando = KeyCode.JoystickButton1;

        public event UnityAction OnPossessionKeyPressed;
        public event UnityAction OnCancelKeyPressed;

        private void Update()
        {
            // Dispara el evento si se pulsa la Z en el teclado O el LB/L1 en el mando
            if (Input.GetKeyDown(possessionKey) || Input.GetKeyDown(possessionMando))
            {
                OnPossessionKeyPressed?.Invoke();
            }

            // Dispara el evento si se pulsa Backspace en el teclado O el botón A/X en el mando
            if (Input.GetKeyDown(cancelKey) || Input.GetKeyDown(cancelMando))
            {
                OnCancelKeyPressed?.Invoke();
            }
        }
    }
}