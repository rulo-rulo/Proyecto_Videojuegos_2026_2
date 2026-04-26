using UnityEngine;
using UnityEngine.Events;

namespace Telekinesis
{
    public class TelekinesisInputHandler : MonoBehaviour
    {
        [Header("Teclado")]
        [SerializeField] private KeyCode actionKey = KeyCode.X;
        [SerializeField] private KeyCode cancelKey = KeyCode.Backspace;

        [Header("Mando")]
        [SerializeField] private KeyCode actionMando = KeyCode.JoystickButton5;

        [SerializeField] private KeyCode cancelMando = KeyCode.JoystickButton1;

        public event UnityAction OnActionKeyPressed;
        public event UnityAction OnCancelKeyPressed;

        public Vector3 LastDirection { get; private set; } = Vector3.up;

        private void Update()
        {
            // Detectamos la X del teclado O el RB/R1 del mando
            if (Input.GetKeyDown(actionKey) || Input.GetKeyDown(actionMando))
                OnActionKeyPressed?.Invoke();

            // Detectamos el Backspace del teclado O la A/X del mando
            if (Input.GetKeyDown(cancelKey) || Input.GetKeyDown(cancelMando))
                OnCancelKeyPressed?.Invoke();

            ReadDirection();
        }

        private void ReadDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                LastDirection = Vector3.forward;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                LastDirection = Vector3.back;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                LastDirection = Vector3.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                LastDirection = Vector3.right;
        }
    }
}