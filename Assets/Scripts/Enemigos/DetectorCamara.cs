using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DetectorCamara : MonoBehaviour
{
    [Header("Conexiones")]
    public Transform jugador;
    public string tagJugador = "Player";
    public LayerMask capaObstaculos;

    [Header("Interfaz y Derrota")]
    public GameObject derrotaPanel;
    public MonoBehaviour movimientoJugador;
    public AudioSource sonidoDerrota;
    public float tiempoDeteccion = 1.0f;

    [Header("Dimensiones del Cono")]
    public float rangoVision = 10f;
    [Range(0, 180)] public float aperturaHorizontal = 60f;
    [Range(0, 180)] public float aperturaVertical = 40f;
    public int resolucion = 20;

    [Header("Visualización")]
    public Color colorNormal = new Color(0, 1, 0, 0.3f);
    public Color colorAlerta = new Color(1, 0, 0, 0.5f);

    private Mesh mesh;
    private Material materialCono;
    private bool derrotaActivada = false;
    private float timerDeteccion = 0f;

    public bool  PlayerDetected  => jugadorEncontrado;
    public float DetectionTimer  => timerDeteccion;

    private bool jugadorEncontrado = false;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Malla_Detector_Camara";
        GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material    = new Material(mr.material);
        materialCono   = mr.material;
        materialCono.color = colorNormal;

        if (derrotaPanel != null)
            derrotaPanel.SetActive(false);
    }

    void LateUpdate()
    {
        if (derrotaActivada) return;

        jugadorEncontrado = false;
        GenerarConoYDetectar(ref jugadorEncontrado);

        if (jugadorEncontrado)
        {
            materialCono.color  = colorAlerta;
            timerDeteccion     += Time.deltaTime;

            if (DetectionHUD.Instance != null)
                DetectionHUD.Instance.ReportTimer(this, tiempoDeteccion - timerDeteccion);

            if (timerDeteccion >= tiempoDeteccion)
                ActivarDerrota();
        }
        else
        {
            materialCono.color = colorNormal;
            timerDeteccion     = 0f;

            if (DetectionHUD.Instance != null)
                DetectionHUD.Instance.RemoveTimer(this);
        }
    }

    void GenerarConoYDetectar(ref bool hayContacto)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(Vector3.zero);

        for (int y = 0; y <= resolucion; y++)
        {
            float tY   = (float)y / resolucion;
            float angY = Mathf.Lerp(-aperturaVertical / 2, aperturaVertical / 2, tY);

            for (int x = 0; x <= resolucion; x++)
            {
                float tX   = (float)x / resolucion;
                float angX = Mathf.Lerp(-aperturaHorizontal / 2, aperturaHorizontal / 2, tX);

                Vector3 dirLocal  = Quaternion.Euler(angY, angX, 0) * Vector3.forward;
                Vector3 dirGlobal = transform.TransformDirection(dirLocal);

                float distanciaFinal = rangoVision;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirGlobal, out hit, rangoVision))
                {
                    distanciaFinal = hit.distance;

                    if (hit.collider.CompareTag(tagJugador))
                        hayContacto = true;
                }

                vertices.Add(dirLocal * distanciaFinal);
            }
        }

        List<int> tri = new List<int>();
        int vFila = resolucion + 1;

        for (int y = 0; y < resolucion; y++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                int i = y * vFila + x + 1;
                tri.Add(i);         tri.Add(i + 1);         tri.Add(i + vFila);
                tri.Add(i + vFila); tri.Add(i + 1);         tri.Add(i + vFila + 1);

                if (y == 0)              { tri.Add(0); tri.Add(i + 1);         tri.Add(i); }
                if (y == resolucion - 1) { tri.Add(0); tri.Add(i + vFila);     tri.Add(i + vFila + 1); }
                if (x == 0)              { tri.Add(0); tri.Add(i);             tri.Add(i + vFila); }
                if (x == resolucion - 1) { tri.Add(0); tri.Add(i + vFila + 1); tri.Add(i + 1); }
            }
        }

        mesh.Clear();
        mesh.vertices  = vertices.ToArray();
        mesh.triangles = tri.ToArray();
        mesh.RecalculateNormals();
    }

    void ActivarDerrota()
    {
        derrotaActivada = true;

        if (DetectionHUD.Instance != null)
            DetectionHUD.Instance.RemoveTimer(this);

        if (derrotaPanel != null)
            derrotaPanel.SetActive(true);

        if (movimientoJugador != null)
            movimientoJugador.enabled = false;

        if (sonidoDerrota != null)
            sonidoDerrota.Play();

        Time.timeScale = 0f;
    }
}