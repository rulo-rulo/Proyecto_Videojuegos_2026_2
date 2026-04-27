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

    private Vector3 puntoAlerta;
    private bool enBusqueda = false;

    private bool ignorarJugadorPorObjeto = false;

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

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (ojos == null)
            ojos = GetComponentInChildren<OjosPatrullero>();
    }

    void Update()
    {
        if (derrotaActivada) return;

        // SI ESTÁ EN BÚSQUEDA POR ALERTA, NO DEJES QUE LOS OJOS LO DEVUELVAN A PATRULLA
        if (enBusqueda)
        {
            estadoActual = Estado.Persiguiendo;
        }
        else
        {
            // Lógica de visión y tiempo
            if (!ignorarJugadorPorObjeto && ojos != null && ojos.viendoAlJugador)
            {
                estadoActual = Estado.Persiguiendo;
                estaCambiandoDePunto = false;

                if (rutinaInvestigacionActual != null)
                {
                    StopCoroutine(rutinaInvestigacionActual);
                    rutinaInvestigacionActual = null;
                }

                // Acumulamos tiempo mientras te ve
                timerDeteccion += Time.deltaTime;

                // Avisamos al círculo/barra del HUD
                if (DetectionHUD.Instance != null)
                {
                    DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);
                }

                // Ya no mata al jugador
                if (timerDeteccion >= tiempoDeteccion)
                {
                    timerDeteccion = 0f;
                }
            }
            else
            {
                // Si nos pierde de vista, vuelve a patrullar y resetea el contador
                if (estadoActual == Estado.Persiguiendo)
                {
                    estadoActual = Estado.Patrullando;
                }

                timerDeteccion = 0f;

                // Borramos el círculo/barra del HUD
                if (DetectionHUD.Instance != null)
                {
                    DetectionHUD.Instance.RemoveTimer(this);
                }
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
        Vector3 destino;

        if (enBusqueda)
        {
            destino = new Vector3(puntoAlerta.x, rb.position.y, puntoAlerta.z);
        }
        else
        {
            destino = new Vector3(jugador.position.x, rb.position.y, jugador.position.z);
        }

        Vector3 nuevaPos = Vector3.MoveTowards(
            rb.position,
            destino,
            velocidadPersecucion * Time.deltaTime
        );

        nuevaPos.y = rb.position.y;

        rb.MovePosition(nuevaPos);
        GirarHacia(destino);

        // Si llega al punto de alerta, deja de buscar
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
    public void ReportarInteraccion(Transform objetoInteraccion)
    {
        if (derrotaActivada) return;
        if (objetoInteraccion == null) return;

        if (ojos == null)
            ojos = GetComponentInChildren<OjosPatrullero>();

        if (ojos == null)
        {
            Debug.LogWarning(gameObject.name + " no tiene OjosPatrullero.");
            return;
        }

        if (!ojos.PuedeVerObjeto(objetoInteraccion))
        {
            Debug.Log(gameObject.name + " no ve el objeto movido.");
            return;
        }

        Debug.Log(gameObject.name + " ve el objeto movido y va a investigar.");

        Vector3 posicionInteraccion = objetoInteraccion.position;

        if (rutinaInvestigacionActual != null)
            StopCoroutine(rutinaInvestigacionActual);

        StopAllCoroutines();

        enBusqueda = false;
        estaCambiandoDePunto = false;
        ignorarJugadorPorObjeto = true;

        rutinaInvestigacionActual = StartCoroutine(IrAInvestigar(posicionInteraccion));
    }

    IEnumerator IrAInvestigar(Vector3 punto)
    {
        estadoActual = Estado.Investigando;

        Vector3 puntoPlano = new Vector3(punto.x, rb.position.y, punto.z);

        Vector3 direccionDesdeEnemigo = (puntoPlano - rb.position).normalized;
        Vector3 puntoSeguro = puntoPlano - direccionDesdeEnemigo * 1.2f;
        puntoSeguro.y = rb.position.y;

        float tiempoMaximoParaLlegar = 3f;
        float tiempoActual = 0f;

        while (
            Vector3.Distance(
                new Vector3(rb.position.x, 0, rb.position.z),
                new Vector3(puntoSeguro.x, 0, puntoSeguro.z)
            ) > 0.3f
            && tiempoActual < tiempoMaximoParaLlegar
        )
        {
            tiempoActual += Time.deltaTime;

            Vector3 destinoPlano = new Vector3(puntoSeguro.x, rb.position.y, puntoSeguro.z);

            Vector3 nuevaPos = Vector3.MoveTowards(
                rb.position,
                destinoPlano,
                velocidadPatrulla * Time.deltaTime
            );

            nuevaPos.y = rb.position.y;

            rb.MovePosition(nuevaPos);
            GirarHacia(destinoPlano);

            yield return null;
        }

        yield return new WaitForSeconds(tiempoInvestigacion);

        rutinaInvestigacionActual = null;
        enBusqueda = false;
        estadoActual = Estado.Patrullando;
        estaCambiandoDePunto = false;
        ignorarJugadorPorObjeto = false;

        float distanciaA = Vector3.Distance(transform.position, puntoA.position);
        float distanciaB = Vector3.Distance(transform.position, puntoB.position);

        destinoActual = distanciaA < distanciaB ? puntoA : puntoB;

        Debug.Log(gameObject.name + " termina investigación y vuelve a patrullar.");
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

        Debug.Log(gameObject.name + " recibió alerta y cambia a estado de búsqueda.");
    }

    // DESACTIVADO PARA SPRINT 3
    /*
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

        Time.timeScale = 0f; // Pausa el juego
    }
    */
}