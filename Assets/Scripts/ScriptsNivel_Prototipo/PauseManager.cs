using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public Animator pauseAnimator;

    private bool isPaused = false;

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        pauseMenu.SetActive(true);
        pauseButton.SetActive(false);

        if (pauseAnimator != null)
        {
            pauseAnimator.Play("PauseMenu", 0, 0f);
        }

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        pauseButton.SetActive(true);

        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MenuInicial");
    }
}