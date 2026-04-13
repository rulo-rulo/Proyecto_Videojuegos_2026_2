using UnityEngine;

namespace Telekinesis
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovableObject : MonoBehaviour
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ApplyForce(Vector3 direction, float force)
        {
            rb.isKinematic = false;
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
            Debug.Log($"[Telekinesis] Fuerza aplicada a {gameObject.name} | Dirección: {direction}");
        }
    }
}
