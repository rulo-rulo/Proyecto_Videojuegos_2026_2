using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camara;
    private CharacterController controlador;

    [Header("Movimiento")]
    [Tooltip("Velocidad base del jugador. Ahora es igual a la antigua velocidad de correr.")]
    [SerializeField] private float velocidadMovimiento = 8f;

    [Header("Gravedad")]
    [SerializeField] private float Gravedad = -9f;
    private Vector3 velocidadVertical;

    void Start()
    {

    }

    // Necesitamos inicializar el CharacterController al inicializar el objeto
    private void Awake()
    {
        // Obtener el componente CharacterController del GameObject
        controlador = GetComponent<CharacterController>();

        // Si no se ha asignado una camara, usar la camara principal
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

    // Método para mover al jugador en el plano horizontal basado en la orientación de la cámara
    private void MoverJugadorEnPlano()
    {
        if (!controlador.enabled) return;

        // ---------------------------------------
        // Movimiento del personje (FLECHAS Y MANDO)
        // ---------------------------------------

        float Horizontal = 0f;
        float Vertical = 0f;

        // 1. Leemos exclusivamente las flechas del teclado
        if (Input.GetKey(KeyCode.RightArrow)) Horizontal += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) Horizontal -= 1f;
        if (Input.GetKey(KeyCode.UpArrow)) Vertical += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) Vertical -= 1f;

        // 2. Leemos EXCLUSIVAMENTE los nuevos ejes del mando
        Horizontal += Input.GetAxisRaw("MandoHorizontal");
        Vertical += Input.GetAxisRaw("MandoVertical");

        // 3. Clampeamos los valores
        Horizontal = Mathf.Clamp(Horizontal, -1f, 1f);
        Vertical = Mathf.Clamp(Vertical, -1f, 1f);

        // Obtener las direcciones adelante y derecha de la camara para el movimiento
        Vector3 adelanteCamara = camara.forward;
        Vector3 derechaCamara = camara.right;

        // Eliminar la componente Y para que no afecte al movimiento horizontal. Queremos que mire pero no que pueda caminar hacia arriba o abajo
        adelanteCamara.y = 0f;
        derechaCamara.y = 0f;

        // Hacemos que no haya ningun lado que se mueva mas que el otro. Se normalizan los vectores.
        adelanteCamara.Normalize();
        derechaCamara.Normalize();

        // Vector de dirección basado en los inputs
        Vector3 direccionPlano = (derechaCamara * Horizontal + adelanteCamara * Vertical);

        // Normalizamos el vector para que no se mueva mas rapido en diagonal.
        if (direccionPlano.sqrMagnitude > 0.0001f)
        {
            direccionPlano.Normalize();
        }

        // Movimiento del personaje usando CharacterController a la nueva velocidad
        Vector3 desplazamientoXZ = direccionPlano * (velocidadMovimiento * Time.deltaTime);

        // Mover el CharacterController
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