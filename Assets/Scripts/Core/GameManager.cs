using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    ShellGame,
    CodeDuel,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    Debug.LogError("GameManager instance not found in scene!");
                }
            }
            return _instance;
        }
    }

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnStateChanged;

    [Header("Game Settings")]
    public int MaxPlayerLives = 3; // Gesamtleben für Spiel oder zurücksetzbar?
    // Basierend auf Vorgabe: "Wenn der Spieler versagt und alle Leben verliert, endet das Spiel". 
    // Es scheint separate Leben für Herausforderungen zu geben? "Sowohl Spieler als auch Dealer haben Lebenspunkte".
    // "Nach einem Sieg öffnen die Leibwächter... nächsten Bereich".
    
    // Wir handhaben spezifische Herausforderungs-Leben in ihren eigenen Managern um es übersichtlich zu halten, 
    // aber GM behandelt globalen Sieg/Niederlage.

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Zum Testen könnten wir in einem bestimmten Zustand oder MainMenu starten
        // ChangeState(GameState.MainMenu);
        
        // Initiale Begrüßungs-Dialog
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.PlaySituationalDialogue(GameType.Both, DialogueCondition.Start);
        }
    }

    [Header("Scene Names")]
    public string MainMenuScene = "MainMenu";
    public string ShellGameScene = "ShellGame";
    public string CodeDuelScene = "CodeDuel";
    public string VictoryScene = "Victory";
    public string GameOverScene = "GameOver";

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        
        Debug.Log($"Spielstatus geändert zu: {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                LoadScene(MainMenuScene);
                if (SoundManager.Instance) SoundManager.Instance.PlayMusic(SoundManager.Instance.MenuMusic);
                break;
            case GameState.ShellGame:
                LoadScene(ShellGameScene);
                if (SoundManager.Instance) SoundManager.Instance.PlayMusic(SoundManager.Instance.ShellGameMusic);
                break;
            case GameState.CodeDuel:
                LoadScene(CodeDuelScene);
                if (SoundManager.Instance) SoundManager.Instance.PlayMusic(SoundManager.Instance.CodeDuelMusic);
                break;
            case GameState.Victory:
                LoadScene(VictoryScene);
                if (SoundManager.Instance) SoundManager.Instance.PlayWin();
                break;
            case GameState.GameOver:
                LoadScene(GameOverScene);
                if (SoundManager.Instance) SoundManager.Instance.PlayLose();
                break;
        }
    }

    void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Lösche alle geladenen Szenen außer DontDestroyOnLoad Objekte
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void Victory()
    {
        ChangeState(GameState.Victory);
    }

    // Helfer zum Starten des Spiels
    public void StartGame()
    {
        ChangeState(GameState.ShellGame);
    }

    // Startet das Spiel komplett neu
    public void RestartGame()
    {
        Debug.Log("Spiel wird neu gestartet...");
        // Gehe zurück zum Hauptmenü und setze Status zurück
        ChangeState(GameState.MainMenu);
    }

    // Beendet das Spiel komplett
    public void QuitGame()
    {
        Debug.Log("Spiel wird beendet...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
