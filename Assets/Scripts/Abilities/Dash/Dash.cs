using UnityEngine;

public class Dash : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerCam;
    private Rigidbody rb;
    private CharacterController controller;

    [Header("Ajustes de Dash")]
    public float dashForce = 25f;
    public float dashDuration = 0.25f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        // Configuración para que no se mueva solo
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
    }

    // Método PÚBLICO para que lo llame el script de Cooldown
    public void ExecuteDash()
    {
        if (controller != null) controller.enabled = false;

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 direction = GetDirection();
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;

        if (controller != null) controller.enabled = true;
    }

    private Vector3 GetDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = playerCam.forward;
        Vector3 right = playerCam.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        if (h == 0 && v == 0) return forward;
        return (forward * v + right * h).normalized;
    }
}