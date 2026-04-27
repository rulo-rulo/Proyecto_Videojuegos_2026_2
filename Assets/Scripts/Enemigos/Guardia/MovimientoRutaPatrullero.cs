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
    // Hemos quitado el radioAlerta porque ahora el propio objeto calcula si la onda le llega al enemigo
    [Tooltip("Tiempo que se queda quieto mirando el punto donde sonó el golpe antes de volver a su ruta.")]
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
        destinoActual = puntoB;

        if (ojos == null)
            ojos = GetComponent<OjosPatrullero>();
    }

    void Update()
    {
        if (derrotaActivada) return;

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

                // Si nos ve mientras investigaba un ruido, cancela la investigación para perseguirnos
                if (rutinaInvestigacionActual != null)
                {
                    StopCoroutine(rutinaInvestigacionActual);
                    rutinaInvestigacionActual = null;
                }

                timerDeteccion += Time.deltaTime;

                if (DetectionHUD.Instance != null)
                {
                    DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);
                }

                if (timerDeteccion >= tiempoDeteccion)
                {
                    timerDeteccion = 0f;
                }
            }
            else
            {
                // Si nos pierde de vista, vuelve a patrullar
                if (estadoActual == Estado.Persiguiendo)
                {
                    estadoActual = Estado.Patrullando;
                }

                timerDeteccion = 0f;

                if (DetectionHUD.Instance != null)
                {
                    DetectionHUD.Instance.RemoveTimer(this);
                }
            }
        }

        // Lógica de estados
        switch (estadoActual)
        {
            case Estado.Patrullando:
                MoverHaciaDestino();
                break;

            case Estado.Investigando:
                // Se maneja solo dentro de la Coroutine 'IrAInvestigar'
                break;

            case Estado.Persiguiendo:
                PerseguirJugador();
                break;

            case Estado.EsperandoYGirando:
                break;
        }
    }

    void MoverHaciaDestino()
    {
        if (destinoActual == null) return;

        Vector3 posPlana = new Vector3(rb.position.x, 0, rb.position.z);
        Vector3 destinoPlano = new Vector3(destinoActual.position.x, 0, destinoActual.position.z);

        if (Vector3.Distance(posPlana, destinoPlano) > 0.05f)
        {
            Vector3 nuevaPos = Vector3.MoveTowards(
                rb.position,
                new Vector3(destinoPlano.x, rb.position.y, destinoPlano.z),
                velocidadPatrulla * Time.deltaTime
            );

            rb.MovePosition(nuevaPos);
            GirarHacia(destinoPlano);
        }
        else if (!estaCambiandoDePunto)
        {
            rb.MovePosition(new Vector3(destinoPlano.x, rb.position.y, destinoPlano.z));
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
        Vector3 destino;

        if (enBusqueda)
            destino = new Vector3(puntoAlerta.x, rb.position.y, puntoAlerta.z);
        else
            destino = new Vector3(jugador.position.x, rb.position.y, jugador.position.z);

        Vector3 nuevaPos = Vector3.MoveTowards(
            rb.position,
            destino,
            velocidadPersecucion * Time.deltaTime
        );

        rb.MovePosition(nuevaPos);
        GirarHacia(destino);

        if (enBusqueda && Vector3.Distance(rb.position, destino) < 0.2f)
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

    // -------------------------------------------------- INVESTIGACIÓN

    // Este método es llamado por el MovableObject cuando la onda de sonido lo toca
    public void ReportarInteraccion(Vector3 posicionInteraccion)
    {
        if (derrotaActivada) return;

        // Si ya nos está persiguiendo (viendo), ignora los ruidos de las cajas
        if (estadoActual == Estado.Persiguiendo) return;

        Debug.Log($"[{gameObject.name}] Escuchó un ruido. Yendo a investigar...");

        // Si ya estaba investigando otro ruido, lo cancela para ir al ruido más nuevo
        if (rutinaInvestigacionActual != null)
        {
            StopCoroutine(rutinaInvestigacionActual);
        }

        estaCambiandoDePunto = false;
        rutinaInvestigacionActual = StartCoroutine(IrAInvestigar(posicionInteraccion));
    }

    IEnumerator IrAInvestigar(Vector3 punto)
    {
        estadoActual = Estado.Investigando;

        Vector3 puntoPlano = new Vector3(punto.x, rb.position.y, punto.z);

        // 1. Camina hacia el origen del sonido
        while (Vector3.Distance(new Vector3(rb.position.x, 0, rb.position.z), puntoPlano) > 0.2f)
        {
            Vector3 nuevaPos = Vector3.MoveTowards(rb.position, puntoPlano, velocidadPatrulla * Time.deltaTime);
            rb.MovePosition(nuevaPos);
            GirarHacia(puntoPlano);
            yield return null;
        }

        // 2. Al llegar, se queda quieto el tiempo marcado en 'tiempoInvestigacion'
        Debug.Log($"[{gameObject.name}] Llegó al origen del ruido. Investigando...");
        yield return new WaitForSeconds(tiempoInvestigacion);

        // 3. Termina de investigar y vuelve a su ruta normal
        Debug.Log($"[{gameObject.name}] Falsa alarma. Volviendo a patrullar.");
        rutinaInvestigacionActual = null;
        estadoActual = Estado.Patrullando;

        // Al dejar 'destinoActual' intacto, el Update volverá a llamar a MoverHaciaDestino() 
        // y el enemigo regresará automáticamente al punto de la ruta en el que se había quedado.
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