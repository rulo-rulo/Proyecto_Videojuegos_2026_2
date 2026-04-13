using UnityEngine;

namespace Possession
{
    [RequireComponent(typeof(PossessionMovement))]
    public class PossessableObject : MonoBehaviour, IPossessable
    {
        [SerializeField] private WeightClass weightClass = WeightClass.Medium;

        private PossessionMovement movement;

        // -------------------------------------------------- Unity

        private void Start()
        {
            movement = GetComponent<PossessionMovement>();
            movement.Deactivate();
        }

        // -------------------------------------------------- IPossessable

        public WeightClass WeightClass => weightClass;
        public Transform Transform     => transform;

        public void OnPossess(float speed)
        {
            movement.Activate(speed);
            Debug.Log($"[Possession] Poseyendo: {gameObject.name} | Velocidad: {speed}");
        }

        public void OnDepossess()
        {
            movement.Deactivate();
            Debug.Log($"[Possession] Dejando: {gameObject.name}");
        }
    }
}