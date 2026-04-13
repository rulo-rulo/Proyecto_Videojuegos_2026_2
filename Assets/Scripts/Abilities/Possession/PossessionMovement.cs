using UnityEngine;

namespace Possession
{
    [RequireComponent(typeof(Rigidbody))]
    public class PossessionMovement : MonoBehaviour
    {
        private Rigidbody rb;
        private float     currentSpeed;
        private bool      isActive;
        private Transform cam;

        // -------------------------------------------------- Unity

        private void Awake()
        {
            rb  = GetComponent<Rigidbody>();
            cam = Camera.main.transform;
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

            Vector3 direction = (camForward * v + camRight * h).normalized;
            Vector3 velocity  = direction * currentSpeed;

            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        // -------------------------------------------------- API pública

        public void Activate(float speed)
        {
            currentSpeed   = speed;
            isActive       = true;
            rb.isKinematic = false;
        }

        public void Deactivate()
        {
            isActive          = false;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic    = true;
        }
    }
}