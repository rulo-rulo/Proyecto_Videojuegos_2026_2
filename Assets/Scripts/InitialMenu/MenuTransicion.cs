using System.Collections; // Necesario para las Corrutinas
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario para controlar la UI (Image)

public class MenuTransicion : MonoBehaviour
{
    [Header("Configuración del Fundido")]
    [Tooltip("Arrastra aquí el PanelFundido de tu Canvas")]
    public Image panelFundido;

    [Tooltip("Velocidad a la que la pantalla se pone en negro (1 es normal, 2 es el doble de rápido)")]
    public float velocidadFundido = 1.5f;

    void Start()
    {
        // Nos aseguramos de que el panel empiece transparente y desactivado 
        // para que no bloquee los clics en el menú al arrancar.
        if (panelFundido != null)
        {
            Color colorInicial = panelFundido.color;
            colorInicial.a = 0f;
            panelFundido.color = colorInicial;
            panelFundido.gameObject.SetActive(false);
        }
    }

    // Este es el método que asignarás al OnClick() de tu botón "Jugar"
    public void BotonJugar(string nombreEscena)
    {
        // Iniciamos la corrutina que hará el fundido y luego cargará la escena
        StartCoroutine(FundidoYCarga(nombreEscena));
    }

    private IEnumerator FundidoYCarga(string escena)
    {
        // 1. Activamos el panel para que empiece a verse (y bloquee clics)
        panelFundido.gameObject.SetActive(true);

        float alpha = 0f;
        Color colorActual = panelFundido.color;

        // 2. Bucle que va subiendo la opacidad poco a poco
        while (alpha < 1f)
        {
            // Time.deltaTime asegura que el fundido sea suave sin importar los FPS
            alpha += Time.deltaTime * velocidadFundido;
            colorActual.a = alpha;
            panelFundido.color = colorActual;

            // Esperamos al siguiente frame para continuar el bucle
            yield return null;
        }

        // 3. Cuando la pantalla está 100% negra, cargamos la escena del juego
        SceneManager.LoadScene(escena);
    }
}