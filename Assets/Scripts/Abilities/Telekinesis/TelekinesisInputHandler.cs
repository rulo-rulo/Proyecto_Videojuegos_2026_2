using UnityEngine;
using UnityEngine.Events;

namespace Telekinesis
{
    public class TelekinesisInputHandler : MonoBehaviour
    {
        [SerializeField] private KeyCode actionKey = KeyCode.X;
        [SerializeField] private KeyCode cancelKey = KeyCode.Backspace;

        public event UnityAction OnActionKeyPressed;
        public event UnityAction OnCancelKeyPressed;

        public Vector3 LastDirection { get; private set; } = Vector3.up;

        private void Update()
        {
            if (Input.GetKeyDown(actionKey))
                OnActionKeyPressed?.Invoke();

            if (Input.GetKeyDown(cancelKey))
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
