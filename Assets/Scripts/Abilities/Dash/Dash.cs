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
    public bool ExecuteDash()
    {
        if (!AbilityManager.Instance.CanUseAbility(this)) return false;

        AbilityManager.Instance.RegisterAbility(this);

        if (controller != null) controller.enabled = false;

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 direction = GetDirection();
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);

        return true;
    }

    private void ResetDash()
    {
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;

        if (controller != null) controller.enabled = true;

        AbilityManager.Instance.ClearAbility(this);
    }

    private Vector3 GetDirection()
    {
        // 1. Leemos el teclado por defecto
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. Leemos también los ejes del joystick que usas en tu movimiento
        h += Input.GetAxisRaw("MandoHorizontal");
        v += Input.GetAxisRaw("MandoVertical");

        // 3. Clampeamos para que, si pulsas teclado y mando a la vez, no corra el doble
        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        Vector3 forward = playerCam.forward;
        Vector3 right = playerCam.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // 4. Si después de comprobar teclado Y mando no hay movimiento, dash hacia adelante
        if (h == 0 && v == 0) return forward;

        // 5. Si hay movimiento, calculamos la diagonal correcta
        return (forward * v + right * h).normalized;
    }
}