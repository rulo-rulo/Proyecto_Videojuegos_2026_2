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
    public Transform puntoA;
    public Transform puntoB;
    public float velocidadPatrulla = 2f;
    public float tiempoDeEspera = 1.5f;
    public float velocidadGiro = 3f;

    [Header("Persecución")]
    public float velocidadPersecucion = 3f;
    public float tiempoDeteccion = 5f;

    [Header("Investigación por Ruido")]
    public float tiempoInvestigacion = 2.5f;

    private Transform destinoActual;
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

        // MUY IMPORTANTE: Evita que el enemigo se caiga de cara si le lanzas una caja fuerte
        rb.freezeRotation = true;

        destinoActual = puntoB;

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
                    estadoActual = Estado.Patrullando;

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
                // Se maneja en la Coroutine
                break;

            case Estado.Persiguiendo:
                PerseguirJugador();
                break;

            case Estado.EsperandoYGirando:
                DetenerMovimientoHorizontal();
                break;
        }
    }

    // -------------------------------------------------- NUEVO SISTEMA DE MOVIMIENTO

    private void AplicarVelocidadHacia(Vector3 destino, float velocidad)
    {
        Vector3 posPlana = new Vector3(rb.position.x, 0, rb.position.z);
        Vector3 destinoPlano = new Vector3(destino.x, 0, destino.z);

        Vector3 direccion = (destinoPlano - posPlana).normalized;

        // Mantenemos la velocidad Y intacta para que la gravedad funcione perfectamente
        // Nota: Si usas Unity 6, puedes cambiar 'velocity' por 'linearVelocity'
        rb.linearVelocity = new Vector3(direccion.x * velocidad, rb.linearVelocity.y, direccion.z * velocidad);
    }

    private void DetenerMovimientoHorizontal()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // -------------------------------------------------- LÓGICA DE ESTADOS

    void MoverHaciaDestino()
    {
        if (destinoActual == null) return;

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

        destinoActual = (destinoActual == puntoA) ? puntoB : puntoA;
        Vector3 nuevoDestinoPlano = new Vector3(destinoActual.position.x, 0, destinoActual.position.z);

        float anguloDiferencia = 180f;

        while (anguloDiferencia > 1f)
        {
            Vector3 direccion = nuevoDestinoPlano - transform.position;
            direccion.y = 0f;

            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadGiro * Time.deltaTime);

            anguloDiferencia = Quaternion.Angle(transform.rotation, rotacionDeseada);
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

        // Caminar
        while (Vector3.Distance(new Vector3(rb.position.x, 0, rb.position.z), puntoPlano) > 0.3f)
        {
            AplicarVelocidadHacia(punto, velocidadPatrulla);
            GirarHacia(punto);
            yield return null; // Esperamos al siguiente frame
        }

        // Al llegar, se detiene y espera
        DetenerMovimientoHorizontal();
        Debug.Log($"[{gameObject.name}] Llegó al origen. Investigando...");
        yield return new WaitForSeconds(tiempoInvestigacion);

        Debug.Log($"[{gameObject.name}] Falsa alarma. Volviendo a patrullar.");
        rutinaInvestigacionActual = null;
        estadoActual = Estado.Patrullando;
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