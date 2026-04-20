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
    public float velocidadPatrulla = 3f;
    public float tiempoDeEspera = 1.5f;
    public float velocidadGiro = 5f;

    [Header("Persecución y Derrota")]
    public float velocidadPersecucion = 5f;
    public float tiempoDeteccion = 1.5f;
    public GameObject derrotaPanel;
    public MonoBehaviour movimientoJugador;

    [Header("Investigación por telequinesis")]
    public float radioAlerta = 6f;
    public float tiempoInvestigacion = 1.5f;

    private Transform destinoActual;
    private bool estaCambiandoDePunto = false;
    private Rigidbody rb;

    private float timerDeteccion = 0f;
    private bool derrotaActivada = false;

    private Coroutine rutinaInvestigacionActual;

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

        // Lógica de visión y tiempo
        if (ojos != null && ojos.viendoAlJugador)
        {
            estadoActual = Estado.Persiguiendo;
            estaCambiandoDePunto = false;
            StopAllCoroutines();
            rutinaInvestigacionActual = null;

            timerDeteccion += Time.deltaTime;

            if (DetectionHUD.Instance != null)
            {
                DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);
            }

            if (timerDeteccion >= tiempoDeteccion)
            {
                ActivarDerrota();
            }
        }
        else
        {
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

        // Lógica de movimiento
        switch (estadoActual)
        {
            case Estado.Patrullando:
                MoverHaciaDestino();
                break;

            case Estado.Investigando:
                // El movimiento de investigación lo controla la coroutine
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
        Vector3 destinoJugador = new Vector3(jugador.position.x, rb.position.y, jugador.position.z);

        Vector3 nuevaPos = Vector3.MoveTowards(
            rb.position,
            destinoJugador,
            velocidadPersecucion * Time.deltaTime
        );

        rb.MovePosition(nuevaPos);
        GirarHacia(destinoJugador);
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
        if (derrotaActivada) return;
        if (estadoActual == Estado.Persiguiendo) return;

        float distancia = Vector3.Distance(transform.position, posicionInteraccion);

        if (distancia > radioAlerta)
            return;

        if (rutinaInvestigacionActual != null)
        {
            StopCoroutine(rutinaInvestigacionActual);
        }

        StopAllCoroutines();
        estaCambiandoDePunto = false;

        rutinaInvestigacionActual = StartCoroutine(IrAInvestigar(posicionInteraccion));
    }

    IEnumerator IrAInvestigar(Vector3 punto)
    {
        estadoActual = Estado.Investigando;

        Vector3 puntoPlano = new Vector3(punto.x, rb.position.y, punto.z);

        while (Vector3.Distance(
            new Vector3(rb.position.x, 0, rb.position.z),
            new Vector3(puntoPlano.x, 0, puntoPlano.z)) > 0.2f)
        {
            Vector3 nuevaPos = Vector3.MoveTowards(
                rb.position,
                puntoPlano,
                velocidadPatrulla * Time.deltaTime
            );

            rb.MovePosition(nuevaPos);
            GirarHacia(puntoPlano);
            yield return null;
        }

        yield return new WaitForSeconds(tiempoInvestigacion);

        rutinaInvestigacionActual = null;
        estadoActual = Estado.Patrullando;
        estaCambiandoDePunto = false;
    }

    void ActivarDerrota()
    {
        derrotaActivada = true;

        if (DetectionHUD.Instance != null)
            DetectionHUD.Instance.RemoveTimer(this);

        if (GameManager.Instance != null)
            GameManager.Instance.FinalizarDerrota();

        if (derrotaPanel != null)
            derrotaPanel.SetActive(true);

        if (movimientoJugador != null)
            movimientoJugador.enabled = false;

        Time.timeScale = 0f;
    }
}