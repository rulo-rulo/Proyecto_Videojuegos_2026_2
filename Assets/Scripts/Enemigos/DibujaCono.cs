using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DibujaCono : MonoBehaviour
{
    [Header("Configuracion Base")]
    public float distancia = 10f;          // Largo del cono
    [Range(0, 360)]
    public float angulo = 90f;             // Ancho del cono
    public LayerMask capaObstaculo;        // ¿Qué frena la luz?
    public int resolucion = 30;            // Cuántos rayos lanzamos (más = más suave)

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "Cono_Mesh";
        meshFilter.mesh = mesh;
    }

    void LateUpdate() // LateUpdate para que la cámara se mueva primero y el cono después
    {
        HacerMallaVision();
    }

    void HacerMallaVision()
    {
        // 1. Necesitamos al menos 2 rayos para formar un triángulo
        int recuentoRayos = Mathf.RoundToInt(angulo * resolucion);
        float tamañoAngulo = angulo / recuentoRayos;

        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= recuentoRayos; i++)
        {
            // Calculamos el ángulo actual en el abanico
            float angleCurrent = (transform.eulerAngles.y - angulo / 2) + tamañoAngulo * i;
            viewPoints.Add(LanzarRayoParaPunto(angleCurrent));
        }

        // 2. Construcción de los Vértices y Triángulos
        int numVertices = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[(viewPoints.Count - 1) * 3];

        vertices[0] = Vector3.zero; // El primer vértice siempre es el origen (la cámara)

        for (int i = 0; i < viewPoints.Count; i++)
        {
            // Convertimos las direcciones en puntos locales respecto a la cámara
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            // Creamos triángulos conectando: Origen -> Punto actual -> Punto siguiente
            if (i < viewPoints.Count - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // 3. Limpiamos y re-dibujamos la malla
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Para que el material se vea bien
    }

    // Función auxiliar para lanzar el rayo y devolver el punto de impacto o el punto máximo
    Vector3 LanzarRayoParaPunto(float angleDegrees)
    {
        Vector3 dir = DirFromAngle(angleDegrees, false);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, distancia, capaObstaculo))
        {
            return hit.point; // Devolvemos donde chocó con la pared
        }
        else
        {
            // Devolvemos el punto más lejano en esa dirección
            return transform.position + dir * distancia;
        }
    }

    // Trigonometría básica (convierte ángulo -> dirección)
    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
};