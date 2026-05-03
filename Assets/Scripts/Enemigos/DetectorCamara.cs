
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DetectorCamara : MonoBehaviour
{
    [Header("Conexiones")]
    public Transform jugador;
    public string tagJugador = "Player";
    public LayerMask capaObstaculos;

    [Header("Interfaz y Alerta")]
    public float tiempoDeteccion = 1.0f;
    public float velocidadEnfoque = 4f;

    [Header("Dimensiones del Cono")]
    public float rangoVision = 10f;
    [Range(0, 180)] public float aperturaHorizontal = 60f;
    [Range(0, 180)] public float aperturaVertical = 40f;
    public int resolucion = 20;

    [Header("Visualización")]
    public Color colorNormal = new Color(0, 1, 0, 0.3f);
    public Color colorAlerta = new Color(1, 0, 0, 0.5f);

    [Header("Límite de seguimiento")]
    public float maxAnguloSeguimiento = 45f;

    private Quaternion rotacionInicial;

    private Mesh mesh;
    private Material materialCono;
    private float timerDeteccion = 0f;
    private bool alertaActivada = false;
    private bool jugadorEncontrado = false;
    private Vector3 ultimoPuntoDeteccion;
    private float tiempoPerdida = 0.6f;
    private float timerPerdida = 0f;

    public bool PlayerDetected => jugadorEncontrado;
    public float DetectionTimer => timerDeteccion;


    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Malla_Detector_Camara";
        GetComponent<MeshFilter>().mesh = mesh;
        rotacionInicial = transform.rotation;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material = new Material(mr.material);
        materialCono = mr.material;
        materialCono.color = colorNormal;
    }

    void LateUpdate()
    {
        bool detectadoAhora = PuedeVerJugador();

        if (detectadoAhora)
        {
            jugadorEncontrado = true;
            timerPerdida = 0f;
        }
        else if (jugadorEncontrado)
        {
            timerPerdida += Time.deltaTime;

            if (timerPerdida >= tiempoPerdida)
            {
                jugadorEncontrado = false;
            }
        }

        GenerarConoYDetectar();

        materialCono.SetColor("_Color", jugadorEncontrado ? colorAlerta : colorNormal);

        if (jugadorEncontrado)
        {
            timerDeteccion += Time.deltaTime;
            ultimoPuntoDeteccion = jugador.position;

           EnfocarJugador();

            if (DetectionHUD.Instance != null)
                DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);

            if (timerDeteccion >= tiempoDeteccion && !alertaActivada)
            {
                ActivarAlerta();
            }
        }
        else
        {
            timerDeteccion = 0f;
            alertaActivada = false;

            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionInicial,
            2f * Time.deltaTime
            );

            if (DetectionHUD.Instance != null)
                DetectionHUD.Instance.RemoveTimer(this);
        }
    }

    bool PuedeVerJugador()
    {
        if (jugador == null)
        {
            Debug.LogWarning("No hay jugador asignado en DetectorCamara");
            return false;
        }

        // Convertimos la posición del jugador a coordenadas locales de la cámara
        Vector3 jugadorLocal = transform.InverseTransformPoint(jugador.position);

        // Si está detrás de la cámara, no cuenta
        if (jugadorLocal.z < 0)
            return false;

        // Comprobamos distancia
        float distancia = new Vector2(jugadorLocal.x, jugadorLocal.z).magnitude;

        if (distancia > rangoVision)
            return false;

        // Comprobamos si está dentro del ángulo horizontal del cono
        float angulo = Mathf.Atan2(jugadorLocal.x, jugadorLocal.z) * Mathf.Rad2Deg;

        if (Mathf.Abs(angulo) > aperturaHorizontal / 2f)
            return false;

        return true;
    }


    void GenerarConoYDetectar()
    {
        int pasos = resolucion;
        float anguloPaso = aperturaHorizontal / pasos;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangulos = new List<int>();

        vertices.Add(Vector3.zero);

        float anguloActual = -aperturaHorizontal / 2;

        for (int i = 0; i <= pasos; i++)
        {
            float rad = anguloActual * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            Vector3 dirGlobal = transform.TransformDirection(dir);

            float distanciaFinal = rangoVision;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dirGlobal, out hit, rangoVision))
            {
                distanciaFinal = hit.distance;

            
            }

            vertices.Add(transform.InverseTransformPoint(transform.position + dirGlobal * distanciaFinal));

            anguloActual += anguloPaso;
        }

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangulos.Add(0);
            triangulos.Add(i);
            triangulos.Add(i + 1);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangulos.ToArray();
        mesh.RecalculateNormals();
    }

    void EnfocarJugador()
    {
        if (jugador == null) return;

        Vector3 objetivo = jugador.position + Vector3.down * 0.5f;
        Vector3 direccion = objetivo - transform.position;

        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        float diferenciaAngulo = Mathf.DeltaAngle(
            rotacionInicial.eulerAngles.y,
            rotacionObjetivo.eulerAngles.y
        );

        if (Mathf.Abs(diferenciaAngulo) > maxAnguloSeguimiento)
        {
            jugadorEncontrado = false;
            return;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadEnfoque * Time.deltaTime
        );
    }

    void ActivarAlerta()
    {
        alertaActivada = true;

        if (DetectionHUD.Instance != null)
            DetectionHUD.Instance.RemoveTimer(this);

        AlertaGlobal.Activar(ultimoPuntoDeteccion);

        Debug.Log("La cámara ha activado una alerta global en: " + ultimoPuntoDeteccion);
    }
}