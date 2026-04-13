using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeneradorCono : MonoBehaviour
{
    [Header("Ajustes de Visión (Reales)")]
    public float distanciaVision = 8f; // Longitud del cono
    public float radioBase = 3f;      // Qué tan ancho se abre al final
    [Range(8, 64)] public int resolucion = 32;

    [Header("Colisión de Malla")]
    public LayerMask capaObstaculos;

    Mesh mesh;
    MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "ConoCamara_Mesh";
        meshFilter.mesh = mesh;
    }

    void LateUpdate()
    {
        GenerarMallaCono();
    }

    void GenerarMallaCono()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangulos = new List<int>();

        // 0. El origen (la lente de la cámara)
        vertices.Add(Vector3.zero);

        // Generar puntos del círculo de la base
        for (int i = 0; i < resolucion; i++)
        {
            float progreso = (float)i / resolucion;
            float anguloRad = progreso * Mathf.PI * 2;

            float x = Mathf.Cos(anguloRad) * radioBase;
            float y = Mathf.Sin(anguloRad) * radioBase;

            Vector3 puntoBase = new Vector3(x, y, distanciaVision);

            // Raycast para que el cono choque con paredes (opcional pero queda pro)
            Vector3 dirGlobal = transform.TransformDirection(puntoBase.normalized);
            if (Physics.Raycast(transform.position, dirGlobal, out RaycastHit hit, distanciaVision, capaObstaculos))
            {
                vertices.Add(transform.InverseTransformPoint(hit.point));
            }
            else
            {
                vertices.Add(puntoBase);
            }
        }

        // 1. Triángulos de los LATERALES (Cuerpo del cono)
        for (int i = 1; i <= resolucion; i++)
        {
            int siguiente = (i == resolucion) ? 1 : i + 1;
            triangulos.Add(0);
            triangulos.Add(siguiente);
            triangulos.Add(i);
        }

        // 2. Triángulos de la TAPA (Cerrar el cono al final)
        // Añadimos un punto central para la tapa para que sea perfecta
        int indiceCentroTapa = vertices.Count;
        vertices.Add(new Vector3(0, 0, distanciaVision));

        for (int i = 1; i <= resolucion; i++)
        {
            int siguiente = (i == resolucion) ? 1 : i + 1;
            triangulos.Add(indiceCentroTapa);
            triangulos.Add(i);
            triangulos.Add(siguiente);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangulos.ToArray();
        mesh.RecalculateNormals();
    }
}