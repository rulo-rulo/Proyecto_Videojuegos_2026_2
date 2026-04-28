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
    public MovimientoRutaPatrullero cerebro;
    public float velocidadMinimaDeteccion = 1f;

    public bool viendoAlJugador = false;
    private Mesh mesh;
    private float cooldownVisual = 0f;

    // Guardamos la máscara de colisión para no recalcularla en cada frame
    private int mascaraObstaculos;

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

        // NUEVO: Ignoramos la Llave y la nueva capa IgnorarVision
        mascaraObstaculos = ~LayerMask.GetMask("Llave", "IgnorarVision");
    }

    void Update()
    {
        viendoAlJugador = false;

        if (cooldownVisual > 0) cooldownVisual -= Time.deltaTime;

        DetectarJugadorYObjetos();
        DibujarCono();

        materialCono.color = viendoAlJugador ? colorAlerta : colorNormal;
    }

    void DetectarJugadorYObjetos()
    {
        Collider[] cosasCercanas = Physics.OverlapSphere(transform.position, rangoVision);

        foreach (Collider cosa in cosasCercanas)
        {
            Vector3 direccionHaciaCosa = (cosa.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, direccionHaciaCosa) <= anguloVision / 2f)
            {
                if (cosa.CompareTag("Player"))
                {
                    // NUEVO: Le pasamos la máscara y le decimos que ignore los Triggers
                    if (Physics.Raycast(transform.position, direccionHaciaCosa, out RaycastHit hit, rangoVision, mascaraObstaculos, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            viendoAlJugador = true;
                        }
                    }
                }
                else if (cooldownVisual <= 0f && cosa.GetComponentInParent<MovableObject>() != null)
                {
                    Rigidbody rb = cosa.GetComponentInParent<Rigidbody>();

                    if (rb != null && rb.linearVelocity.magnitude >= velocidadMinimaDeteccion)
                    {
                        // NUEVO: Le pasamos la máscara y le decimos que ignore los Triggers
                        if (Physics.Raycast(transform.position, direccionHaciaCosa, out RaycastHit hitCaja, rangoVision, mascaraObstaculos, QueryTriggerInteraction.Ignore))
                        {
                            if (hitCaja.collider == cosa)
                            {
                                cerebro.ReportarInteraccion(cosa.transform.position);
                                cooldownVisual = 1f;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

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

            // NUEVO: Usamos la máscara para dibujar, ignorando los Triggers
            if (Physics.Raycast(ray, out hit, rangoVision, mascaraObstaculos, QueryTriggerInteraction.Ignore))
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