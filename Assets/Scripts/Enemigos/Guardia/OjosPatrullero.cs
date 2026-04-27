using UnityEngine;
using Telekinesis;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OjosPatrullero : MonoBehaviour
{
    public Material materialCono;
    public float rangoVision = 10f;
    [Range(0, 360)] public float anguloVision = 90f;
    public int resolucion = 100;

    public Color colorNormal = new Color(0, 1, 0, 0.3f);
    public Color colorAlerta = new Color(1, 0, 0, 0.5f);

    [Header("Detección Visual de Objetos")]
    [Tooltip("El cerebro del guardia. Se asigna solo automáticamente.")]
    public MovimientoRutaPatrullero cerebro;
    [Tooltip("A qué velocidad debe moverse la caja para llamar su atención (evita que vea cajas quietas).")]
    public float velocidadMinimaDeteccion = 1f;

    public bool viendoAlJugador = false;
    private Mesh mesh;

    // Evita que el guardia colapse enviando 60 avisos por segundo
    private float cooldownVisual = 0f;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material = new Material(materialCono);
        materialCono = mr.material;
        materialCono.color = colorNormal;
        materialCono.renderQueue = 2450;

        if (cerebro == null)
            cerebro = GetComponentInParent<MovimientoRutaPatrullero>();
    }

    void Update()
    {
        viendoAlJugador = false;

        if (cooldownVisual > 0) cooldownVisual -= Time.deltaTime;

        // 1. Ejecutamos el cerebro real (Detección en Volumen 3D)
        DetectarJugadorYObjetos();

        // 2. Dibujamos el cono de luz en el suelo (Puro efecto visual)
        DibujarCono();

        materialCono.color = viendoAlJugador ? colorAlerta : colorNormal;
    }

    // --------------------------------------------------------
    // SISTEMA DE DETECCIÓN INTELIGENTE
    // --------------------------------------------------------
    void DetectarJugadorYObjetos()
    {
        // Cogemos TODO lo que esté dentro de una esfera alrededor del guardia
        Collider[] cosasCercanas = Physics.OverlapSphere(transform.position, rangoVision);

        foreach (Collider cosa in cosasCercanas)
        {
            // Trazamos una línea imaginaria desde el ojo hasta el objeto
            Vector3 direccionHaciaCosa = (cosa.transform.position - transform.position).normalized;

            // Verificamos si el objeto está dentro del ángulo de su campo de visión
            if (Vector3.Angle(transform.forward, direccionHaciaCosa) <= anguloVision / 2f)
            {
                // A) DETECCIÓN DEL JUGADOR
                if (cosa.CompareTag("Player"))
                {
                    // Raycast para asegurar que no hay una pared tapando al jugador
                    if (Physics.Raycast(transform.position, direccionHaciaCosa, out RaycastHit hit, rangoVision))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            viendoAlJugador = true;
                        }
                    }
                }
                // B) DETECCIÓN DE OBJETOS VOLADORES (Cajas de Telequinesis)
                else if (cooldownVisual <= 0f && cosa.GetComponentInParent<MovableObject>() != null)
                {
                    Rigidbody rb = cosa.GetComponentInParent<Rigidbody>();

                    // ¿Se está moviendo rápido?
                    if (rb != null && rb.linearVelocity.magnitude >= velocidadMinimaDeteccion)
                    {
                        // Raycast para asegurar que la caja no está detrás de un muro de cristal/pared
                        if (Physics.Raycast(transform.position, direccionHaciaCosa, out RaycastHit hitCaja, rangoVision))
                        {
                            // Si el rayo choca directamente contra la caja que vimos...
                            if (hitCaja.collider == cosa)
                            {
                                cerebro.ReportarInteraccion(cosa.transform.position);
                                cooldownVisual = 1f; // Pausamos la detección 1 segundo para no spamear
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    // --------------------------------------------------------
    // SISTEMA VISUAL DEL CONO (Malla Verde)
    // --------------------------------------------------------
    void DibujarCono()
    {
        int vertexCount = resolucion + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(resolucion - 1) * 3];
        vertices[0] = Vector3.zero;

        float angleStep = anguloVision / (resolucion - 1);
        float currentAngle = -anguloVision / 2;

        for (int i = 0; i < resolucion; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = transform.rotation * new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

            Ray ray = new Ray(transform.position, dir);
            RaycastHit hit;
            float distance = rangoVision;

            int mask = ~LayerMask.GetMask("Llave");

            // Solo usamos el raycast aquí para que la malla verde choque contra las paredes y no las atraviese
            if (Physics.Raycast(ray, out hit, rangoVision, mask))
            {
                distance = hit.distance;
            }

            vertices[i + 1] = transform.InverseTransformPoint(transform.position + dir * distance);
            currentAngle += angleStep;
        }

        for (int i = 0; i < resolucion - 1; i++)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}