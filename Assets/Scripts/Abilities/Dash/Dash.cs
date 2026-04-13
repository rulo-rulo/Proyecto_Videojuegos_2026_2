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
    public float dashCd = 1.5f;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        // Configuración para evitar que se mueva solo
        rb.useGravity = false;
        rb.isKinematic = true; // Empezamos en Kinematic para bloquear el movimiento físico
        rb.freezeRotation = true;

        // Bloqueamos la posición física para que no deslice por error
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
    }

    void Update()
    {
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;

        if (Input.GetKeyDown(dashKey) && dashCdTimer <= 0)
            ExecuteDash();
    }

    private void ExecuteDash()
    {
        dashCdTimer = dashCd;

        // 1. Apagamos el CharacterController y liberamos el Rigidbody
        if (controller != null) controller.enabled = false;

        rb.isKinematic = false;
        // Liberamos solo la posición para el impulso (mantenemos rotación bloqueada)
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 2. Calculamos la dirección
        Vector3 direction = GetDirection();

        // 3. Aplicamos el impulso
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        rb.linearVelocity = Vector3.zero;

        // 4. Volvemos a congelar el Rigidbody por completo
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;

        if (controller != null) controller.enabled = true;
    }

    private Vector3 GetDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // AQUÍ ESTÁ EL TRUCO: 
        // Tomamos el "forward" de la cámara y lo proyectamos en un plano horizontal puro
        Vector3 forward = playerCam.forward;
        Vector3 right = playerCam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 dir;
        // Si no pulsas nada, va hacia donde apunta la cámara (pero recto)
        if (h == 0 && v == 0)
        {
            dir = forward;
        }
        else
        {
            // Si pulsas teclas, combina las direcciones
            dir = (forward * v + right * h).normalized;
        }

        return dir;
    }
}