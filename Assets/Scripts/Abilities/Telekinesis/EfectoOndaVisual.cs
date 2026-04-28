using UnityEngine;

public class EfectoOnda : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float radioMaximo;
    private float duracion;
    private float tiempoActual = 0f;
    private Color colorBase;

    public void Iniciar(float radio, Color color, float tiempo)
    {
        radioMaximo = radio;
        duracion = tiempo;
        colorBase = color;

        // Configuramos el dibujador de líneas
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.2f; // Grosor de la línea
        lineRenderer.endWidth = 0.2f;
        lineRenderer.positionCount = 51; // Puntos para que el círculo sea redondo

        // Usamos el material por defecto de sprites para que admita transparencias
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = colorBase;
        lineRenderer.endColor = colorBase;

        DibujarCirculo(0f);
    }

    private void Update()
    {
        tiempoActual += Time.deltaTime;
        float progreso = tiempoActual / duracion;

        // Curva suave para que frene un poco al final al expandirse
        float progresoSuavizado = Mathf.Clamp01(Mathf.Sin(progreso * Mathf.PI * 0.5f));

        if (progreso >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Expandir el radio
        float radioActual = Mathf.Lerp(0f, radioMaximo, progresoSuavizado);
        DibujarCirculo(radioActual);

        // Desvanecer la transparencia (Alpha)
        float alpha = Mathf.Lerp(colorBase.a, 0f, progreso);
        Color colorTransparente = new Color(colorBase.r, colorBase.g, colorBase.b, alpha);
        lineRenderer.startColor = colorTransparente;
        lineRenderer.endColor = colorTransparente;
    }

    private void DibujarCirculo(float radio)
    {
        float x, z;
        float angulo = 0f;

        for (int i = 0; i < 51; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angulo) * radio;
            z = Mathf.Cos(Mathf.Deg2Rad * angulo) * radio;

            // Dibujamos el círculo acostado en el suelo (eje X y Z)
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angulo += (360f / 50f);
        }
    }
}