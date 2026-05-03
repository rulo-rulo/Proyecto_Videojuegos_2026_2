using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MovimientoRutaPatrullero : MonoBehaviour
{
    private enum Estado { Patrullando, Investigando, Persiguiendo, EsperandoYGirando }
    [SerializeField] private Estado estadoActual = Estado.Patrullando;

    [Header("Referencias")]
    public OjosPatrullero ojos;
    public Transform jugador;

    [Header("Ruta de Patrulla")]
    [Tooltip("Añade aquí todos los puntos por los que pasará el enemigo.")]
    public Transform[] puntosDePatrulla;
    [Tooltip("Si se marca, elegirá el siguiente punto al azar. Si no, irá en orden.")]
    public bool patrullaAleatoria = false;

    public float velocidadPatrulla = 2f;
    public float tiempoDeEspera = 1.5f;
    public float velocidadGiro = 3f;

    [Header("Persecución")]
    public float velocidadPersecucion = 3f;
    public float tiempoDeteccion = 5f;

    [Header("Investigación por Ruido")]
    public float tiempoInvestigacion = 2.5f;

    private Transform destinoActual;
    private int indicePuntoActual = 0; // Para saber en qué punto de la lista estamos
    private bool estaCambiandoDePunto = false;
    private Rigidbody rb;

    private float timerDeteccion = 0f;
    private bool derrotaActivada = false;

    private Coroutine rutinaInvestigacionActual;

    private Vector3 puntoAlerta;
    private bool enBusqueda = false;

    void OnEnable()
    {
        AlertaGlobal.OnAlertaGlobal += RecibirAlerta;
    }

    void OnDisable()
    {
        AlertaGlobal.OnAlertaGlobal -= RecibirAlerta;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Asignamos el primer punto de la ruta si existe alguno
        if (puntosDePatrulla.Length > 0)
        {
            destinoActual = puntosDePatrulla[0];
        }

        if (ojos == null)
            ojos = GetComponent<OjosPatrullero>();
    }

    void Update()
    {
        if (derrotaActivada)
        {
            DetenerMovimientoHorizontal();
            return;
        }

        if (enBusqueda)
        {
            estadoActual = Estado.Persiguiendo;
        }
        else
        {
            // Lógica de visión
            if (ojos != null && ojos.viendoAlJugador)
            {
                estadoActual = Estado.Persiguiendo;
                estaCambiandoDePunto = false;

                if (rutinaInvestigacionActual != null)
                {
                    StopCoroutine(rutinaInvestigacionActual);
                    rutinaInvestigacionActual = null;
                }

                timerDeteccion += Time.deltaTime;

                if (DetectionHUD.Instance != null)
                    DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);

                if (timerDeteccion >= tiempoDeteccion)
                    timerDeteccion = 0f;
            }
            else
            {
                if (estadoActual == Estado.Persiguiendo)
                {
                    // Al perderte de vista, vuelve a buscar el punto actual de su ruta
                    estadoActual = Estado.Patrullando;
                    if (puntosDePatrulla.Length > 0)
                        destinoActual = puntosDePatrulla[indicePuntoActual];
                }

                timerDeteccion = 0f;

                if (DetectionHUD.Instance != null)
                    DetectionHUD.Instance.RemoveTimer(this);
            }
        }

        // Lógica de estados
        switch (estadoActual)
        {
            case Estado.Patrullando:
                MoverHaciaDestino();
                break;

            case Estado.Investigando:
                break;

            case Estado.Persiguiendo:
                PerseguirJugador();
                break;

            case Estado.EsperandoYGirando:
                DetenerMovimientoHorizontal();
                break;
        }
    }

    // -------------------------------------------------- SISTEMA DE MOVIMIENTO

    private void AplicarVelocidadHacia(Vector3 destino, float velocidad)
    {
        Vector3 posPlana = new Vector3(rb.position.x, 0, rb.position.z);
        Vector3 destinoPlano = new Vector3(destino.x, 0, destino.z);

        Vector3 direccion = (destinoPlano - posPlana).normalized;
        rb.linearVelocity = new Vector3(direccion.x * velocidad, rb.linearVelocity.y, direccion.z * velocidad);
    }

    private void DetenerMovimientoHorizontal()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // -------------------------------------------------- LÓGICA DE ESTADOS

    void MoverHaciaDestino()
    {
        if (destinoActual == null || puntosDePatrulla.Length == 0) return;

        Vector3 posPlana = new Vector3(rb.position.x, 0, rb.position.z);
        Vector3 destinoPlano = new Vector3(destinoActual.position.x, 0, destinoActual.position.z);

        if (Vector3.Distance(posPlana, destinoPlano) > 0.1f)
        {
            AplicarVelocidadHacia(destinoPlano, velocidadPatrulla);
            GirarHacia(destinoPlano);
        }
        else if (!estaCambiandoDePunto)
        {
            DetenerMovimientoHorizontal();
            StartCoroutine(SecuenciaCambioDePunto());
        }
    }

    IEnumerator SecuenciaCambioDePunto()
    {
        estaCambiandoDePunto = true;
        estadoActual = Estado.EsperandoYGirando;

        yield return new WaitForSeconds(tiempoDeEspera);

        // --- MAGIA DEL CAMBIO DE RUTA ---
        if (puntosDePatrulla.Length > 1)
        {
            if (patrullaAleatoria)
            {
                // Elige un número al azar distinto al actual
                int nuevoIndice = indicePuntoActual;
                while (nuevoIndice == indicePuntoActual)
                {
                    nuevoIndice = Random.Range(0, puntosDePatrulla.Length);
                }
                indicePuntoActual = nuevoIndice;
            }
            else
            {
                // Va en orden: 0, 1, 2... y cuando llega al final, vuelve al 0
                indicePuntoActual = (indicePuntoActual + 1) % puntosDePatrulla.Length;
            }
        }

        destinoActual = puntosDePatrulla[indicePuntoActual];
        // --------------------------------

        Vector3 nuevoDestinoPlano = new Vector3(destinoActual.position.x, 0, destinoActual.position.z);
        float anguloDiferencia = 180f;

        while (anguloDiferencia > 1f)
        {
            Vector3 direccion = nuevoDestinoPlano - transform.position;
            direccion.y = 0f;

            if (direccion.sqrMagnitude > 0.01f)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadGiro * Time.deltaTime);
                anguloDiferencia = Quaternion.Angle(transform.rotation, rotacionDeseada);
            }
            else
            {
                break;
            }
            yield return null;
        }

        estadoActual = Estado.Patrullando;
        estaCambiandoDePunto = false;
    }

    void PerseguirJugador()
    {
        Vector3 destino = enBusqueda ? puntoAlerta : jugador.position;
        Vector3 posPlana = new Vector3(rb.position.x, 0, rb.position.z);
        Vector3 destinoPlano = new Vector3(destino.x, 0, destino.z);

        AplicarVelocidadHacia(destino, velocidadPersecucion);
        GirarHacia(destino);

        if (enBusqueda && Vector3.Distance(posPlana, destinoPlano) < 0.3f)
        {
            enBusqueda = false;
            estadoActual = Estado.Patrullando;

            // Cuando termina de buscar el ruido, se dirige al punto que le tocaba
            if (puntosDePatrulla.Length > 0)
                destinoActual = puntosDePatrulla[indicePuntoActual];

            Debug.Log(gameObject.name + " llegó al último punto de detección.");
        }
    }

    void GirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;

        if (direccion.magnitude > 0.01f)
        {
            Quaternion rotacion = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, velocidadGiro * Time.deltaTime);
        }
    }

    public void ReportarInteraccion(Vector3 posicionInteraccion)
    {
        if (derrotaActivada || estadoActual == Estado.Persiguiendo) return;

        Debug.Log($"[{gameObject.name}] Escuchó o vio un objeto. Yendo a investigar...");

        if (rutinaInvestigacionActual != null) StopCoroutine(rutinaInvestigacionActual);

        estaCambiandoDePunto = false;
        rutinaInvestigacionActual = StartCoroutine(IrAInvestigar(posicionInteraccion));
    }

    IEnumerator IrAInvestigar(Vector3 punto)
    {
        estadoActual = Estado.Investigando;
        Vector3 puntoPlano = new Vector3(punto.x, 0, punto.z);

        while (Vector3.Distance(new Vector3(rb.position.x, 0, rb.position.z), puntoPlano) > 0.3f)
        {
            AplicarVelocidadHacia(punto, velocidadPatrulla);
            GirarHacia(punto);
            yield return null;
        }

        DetenerMovimientoHorizontal();
        Debug.Log($"[{gameObject.name}] Llegó al origen. Investigando...");
        yield return new WaitForSeconds(tiempoInvestigacion);

        Debug.Log($"[{gameObject.name}] Falsa alarma. Volviendo a patrullar.");
        rutinaInvestigacionActual = null;
        estadoActual = Estado.Patrullando;

        // Vuelve a su ruta
        if (puntosDePatrulla.Length > 0)
            destinoActual = puntosDePatrulla[indicePuntoActual];
    }

    void RecibirAlerta(Vector3 punto)
    {
        puntoAlerta = punto;
        enBusqueda = true;
        estadoActual = Estado.Persiguiendo;
        estaCambiandoDePunto = false;

        if (rutinaInvestigacionActual != null)
        {
            StopCoroutine(rutinaInvestigacionActual);
            rutinaInvestigacionActual = null;
        }
    }
}