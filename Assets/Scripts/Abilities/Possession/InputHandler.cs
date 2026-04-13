using UnityEngine;
using UnityEngine.Events;

namespace Possession
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private KeyCode possessionKey = KeyCode.E;
        [SerializeField] private KeyCode cancelKey     = KeyCode.Backspace;

        public event UnityAction OnPossessionKeyPressed;
        public event UnityAction OnCancelKeyPressed;

        private void Update()
        {
            if (Input.GetKeyDown(possessionKey))
                OnPossessionKeyPressed?.Invoke();

            if (Input.GetKeyDown(cancelKey))
                OnCancelKeyPressed?.Invoke();
        }
    }
}