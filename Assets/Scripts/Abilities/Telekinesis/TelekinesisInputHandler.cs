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

        // Cambiamos el valor por defecto a forward (hacia adelante)
        public Vector3 LastDirection { get; private set; } = Vector3.forward;

        private void Update()
        {
            if (Input.GetKeyDown(actionKey) || Input.GetKeyDown(actionMando))
                OnActionKeyPressed?.Invoke();

            if (Input.GetKeyDown(cancelKey) || Input.GetKeyDown(cancelMando))
                OnCancelKeyPressed?.Invoke();

            ReadDirection();
        }

        private void ReadDirection()
        {
            // 1. Leemos el teclado y el mando juntos
            float h = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("MandoHorizontal");
            float v = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("MandoVertical");

            // 2. Clampeamos para evitar sumar más de 1
            h = Mathf.Clamp(h, -1f, 1f);
            v = Mathf.Clamp(v, -1f, 1f);

            // 3. Si el jugador está moviendo el joystick o las teclas, guardamos esa dirección
            if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
            {
                // La X es horizontal, la Z es la profundidad (adelante/atrás)
                LastDirection = new Vector3(h, 0, v).normalized;
            }
            // Si el jugador suelta el mando, LastDirection recordará la última dirección hacia la que estaba apuntando.
        }
    }
}