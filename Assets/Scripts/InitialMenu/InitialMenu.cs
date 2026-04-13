using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario para la UI (Image)

public class InitialMenu : MonoBehaviour
{
    [Header("Configuración del Fundido")]
    [Tooltip("Arrastra aquí el PanelFundido de tu Canvas")]
    public Image panelFundido;

    [Tooltip("Velocidad a la que la pantalla se pone en negro")]
    public float velocidadFundido = 1.5f;

    void Start()
    {
        // Preparamos el panel para que empiece invisible y no moleste
        if (panelFundido != null)
        {
            Color colorInicial = panelFundido.color;
            colorInicial.a = 0f;
            panelFundido.color = colorInicial;
            panelFundido.gameObject.SetActive(false);
        }
    }

    // Este es el método del botón Jugar
    public void Jugar()
    {
        // En lugar de cargar la escena de golpe, iniciamos la animación de fundido
        StartCoroutine(FundidoYCarga());
    }

    private IEnumerator FundidoYCarga()
    {
        // 1. Activamos el panel negro
        if (panelFundido != null)
        {
            panelFundido.gameObject.SetActive(true);
            float alpha = 0f;
            Color colorActual = panelFundido.color;

            // 2. Lo vamos oscureciendo poco a poco
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * velocidadFundido;
                colorActual.a = alpha;
                panelFundido.color = colorActual;
                yield return null;
            }
        }

        // 3. Cuando ya está todo negro, cargamos la siguiente escena (buildIndex + 1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Tu método de salir se queda intacto
    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}