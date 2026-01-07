using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("Kann Spiel nicht starten: GameManager.Instance ist null!");
        }
    }

    public void OnQuitButton()
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
