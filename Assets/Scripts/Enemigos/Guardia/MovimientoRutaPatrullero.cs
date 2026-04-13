using UnityEngine;
using System.Collections;

public class MovimientoRutaPatrullero : MonoBehaviour
{
    private enum Estado { Patrullando, Persiguiendo, EsperandoYGirando }
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
    public float tiempoDeteccion = 1.5f; // Segundos que tarda en atraparte si te ve
    public GameObject derrotaPanel;
    public MonoBehaviour movimientoJugador;

    private Transform destinoActual;
    private bool estaCambiandoDePunto = false;

    // Variables de control de tiempo
    private float timerDeteccion = 0f;
    private bool derrotaActivada = false;

    void Start()
    {
        destinoActual = puntoB;
        if (ojos == null) ojos = GetComponent<OjosPatrullero>();
    }

    void Update()
    {
        // Si ya nos ha pillado y el juego está parado, no hacemos nada más
        if (derrotaActivada) return;

        // LÓGICA DE VISIÓN Y TIEMPO
        if (ojos.viendoAlJugador)
        {
            estadoActual = Estado.Persiguiendo;
            estaCambiandoDePunto = false;
            StopAllCoroutines();

            // Acumulamos tiempo mientras te ve
            timerDeteccion += Time.deltaTime;

            // Avisamos al círculo/barra del HUD
            if (DetectionHUD.Instance != null)
            {
                DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);
            }

            // Si el tiempo llega al límite, perdemos
            if (timerDeteccion >= tiempoDeteccion)
            {
                ActivarDerrota();
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

        // LÓGICA DE MOVIMIENTO
        switch (estadoActual)
        {
            case Estado.Patrullando:
                MoverHaciaDestino();
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
        Vector3 posPlana = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 destinoPlano = new Vector3(destinoActual.position.x, 0, destinoActual.position.z);

        if (Vector3.Distance(posPlana, destinoPlano) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(destinoPlano.x, transform.position.y, destinoPlano.z),
                velocidadPatrulla * Time.deltaTime);

            GirarHacia(destinoPlano);
        }
        else if (!estaCambiandoDePunto)
        {
            transform.position = new Vector3(destinoPlano.x, transform.position.y, destinoPlano.z);
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
            direccion.y = 0;
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
        Vector3 destinoJugador = new Vector3(jugador.position.x, transform.position.y, jugador.position.z);
        transform.position = Vector3.MoveTowards(transform.position, destinoJugador, velocidadPersecucion * Time.deltaTime);
        GirarHacia(destinoJugador);
    }

    void GirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0;
        if (direccion.magnitude > 0.01f)
        {
            Quaternion rotacion = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, velocidadGiro * Time.deltaTime);
        }
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

        Time.timeScale = 0f; // Pausa el juego
    }
}