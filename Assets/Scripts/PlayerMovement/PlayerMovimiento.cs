using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camara;
    private CharacterController controlador;

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float velocidadRotacion = 10f;

    [Header("Gravedad")]
    [SerializeField] private float Gravedad = -9f;
    private Vector3 velocidadVertical;

    private void Awake()
    {

        Application.targetFrameRate = 60; // Fuerza al juego a intentar ir siempre a 60fps
        controlador = GetComponent<CharacterController>();

        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }

    }

    void FixedUpdate() // Cambiado de Update a FixedUpdate
    {
        if (!enabled || !controlador.enabled) return;

        // 1. Dirección
        Vector3 direccionHorizontal = CalcularDireccionInput();

        // 2. Rotación (Usamos fixedDeltaTime)
        if (direccionHorizontal.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionHorizontal);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.fixedDeltaTime);
        }

        // 3. Gravedad
        ActualizarGravedadFixed();

        // 4. Mover (UNA VEZ, con fixedDeltaTime)
        Vector3 movimientoFinal = (direccionHorizontal * velocidadMovimiento) + velocidadVertical;
        controlador.Move(movimientoFinal * Time.fixedDeltaTime);
    }

    // Crea esta versión pequeña para el FixedUpdate
    private void ActualizarGravedadFixed()
    {
        if (controlador.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f;
        }
        else
        {
            velocidadVertical.y += Gravedad * Time.fixedDeltaTime;
        }
    }

    private Vector3 CalcularDireccionInput()
    {
        float h = 0f;
        float v = 0f;

        // Soporte para flechas y mando
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        h += Input.GetAxisRaw("MandoHorizontal");
        v += Input.GetAxisRaw("MandoVertical");

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        Vector3 forward = camara.forward;
        Vector3 right = camara.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (right * h + forward * v).normalized;
    }
}