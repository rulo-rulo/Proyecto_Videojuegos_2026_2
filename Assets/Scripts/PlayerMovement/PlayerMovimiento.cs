using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camara;
    private CharacterController controlador;

    [Header("Movimiento")]
    [Tooltip("Velocidad base del jugador. Ahora es igual a la antigua velocidad de correr.")]
    [SerializeField] private float velocidadMovimiento = 8f;

    // NUEVO: Variable para controlar lo rápido que gira el personaje
    [Tooltip("Velocidad a la que el personaje gira hacia la dirección de movimiento.")]
    [SerializeField] private float velocidadRotacion = 10f;

    [Header("Gravedad")]
    [SerializeField] private float Gravedad = -9f;
    private Vector3 velocidadVertical;

    void Start()
    {

    }

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();

        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    void Update()
    {
        if (!enabled) return;
        MoverJugadorEnPlano();
        AplicarGravedad();
    }

    private void MoverJugadorEnPlano()
    {
        if (!controlador.enabled) return;

        float Horizontal = 0f;
        float Vertical = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) Horizontal += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) Horizontal -= 1f;
        if (Input.GetKey(KeyCode.UpArrow)) Vertical += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) Vertical -= 1f;

        Horizontal += Input.GetAxisRaw("MandoHorizontal");
        Vertical += Input.GetAxisRaw("MandoVertical");

        Horizontal = Mathf.Clamp(Horizontal, -1f, 1f);
        Vertical = Mathf.Clamp(Vertical, -1f, 1f);

        Vector3 adelanteCamara = camara.forward;
        Vector3 derechaCamara = camara.right;

        adelanteCamara.y = 0f;
        derechaCamara.y = 0f;

        adelanteCamara.Normalize();
        derechaCamara.Normalize();

        Vector3 direccionPlano = (derechaCamara * Horizontal + adelanteCamara * Vertical);

        // Si hay algún input de movimiento (sqrMagnitude mayor que casi cero)
        if (direccionPlano.sqrMagnitude > 0.0001f)
        {
            // Normalizamos el vector para que no se mueva mas rapido en diagonal.
            direccionPlano.Normalize();

            // NUEVO: Calculamos la rotación deseada mirando hacia la dirección del movimiento
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionPlano);

            // NUEVO: Giramos suavemente el personaje hacia esa rotación
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
        }

        Vector3 desplazamientoXZ = direccionPlano * (velocidadMovimiento * Time.deltaTime);
        controlador.Move(desplazamientoXZ);
    }

    private void AplicarGravedad()
    {
        if (!controlador.enabled) return;
        velocidadVertical.y += Gravedad * Time.deltaTime;
        controlador.Move(velocidadVertical * Time.deltaTime);

        if (controlador.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f;
        }
    }
}