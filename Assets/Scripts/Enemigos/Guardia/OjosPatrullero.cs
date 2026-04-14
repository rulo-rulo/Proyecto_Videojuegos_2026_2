using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OjosPatrullero : MonoBehaviour
{
    public Material materialCono;
    public float rangoVision = 10f;
    [Range(0, 360)] public float anguloVision = 90f;
    public int resolucion = 100;

    public Color colorNormal = new Color(0, 1, 0, 0.3f);
    public Color colorAlerta = new Color(1, 0, 0, 0.5f);

    public bool viendoAlJugador = false;
    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material = new Material(materialCono);
        materialCono = mr.material;
        materialCono.color = colorNormal;
    }

    void Update()
    {
        viendoAlJugador = false;
        DibujarCono();
        materialCono.color = viendoAlJugador ? colorAlerta : colorNormal;
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

            int mask = ~LayerMask.GetMask("Llave");

            if (Physics.Raycast(ray, out hit, rangoVision, mask))
            {
                distance = hit.distance;
                if (hit.collider.CompareTag("Player"))
                {
                    viendoAlJugador = true;
                }
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