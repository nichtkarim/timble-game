using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI TitleText;
    public GameObject UIPanel;

    public void Setup(bool victory)
    {
        TitleText.text = victory ? "DU BIST ENTKOMMEN!" : "SPIEL VORBEI";
        if (UIPanel != null) UIPanel.SetActive(true);
    }

    public void OnRestart()
    {
        // Setze Zeit zurück (falls pausiert)
        Time.timeScale = 1f;
        
        // Deaktiviere EventSystem
        UnityEngine.EventSystems.EventSystem es = UnityEngine.EventSystems.EventSystem.current;
        if (es != null) es.gameObject.SetActive(false);
        
        // Verstecke UI
        if (UIPanel != null) UIPanel.SetActive(false);
        gameObject.SetActive(false);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void OnQuit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            Application.Quit();
        }
    }
}
