using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour
{
    public string siguienteNivel = "Nivel_Prototipo";

    public void SiguienteNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(siguienteNivel);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicial");
    }
}