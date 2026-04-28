using UnityEngine;
using System.Collections;

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

        // Configuración para evitar tirones y saltos de colisión
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
        }
    }

    // Método PÚBLICO que activa la corrutina
    public void ExecuteDash()
    {
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        // 1. Apagamos el controlador para que la física mande
        if (controller != null) controller.enabled = false;
        yield return null;

        // 2. Liberamos el Rigidbody
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;

        // 3. Aplicamos fuerza
        Vector3 direction = GetDirection();
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        // 4. Duración del impulso
        yield return new WaitForSeconds(dashDuration);

        // 5. Frenado y re-congelación
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;

        // 6. Sincronización final
        yield return new WaitForFixedUpdate();

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