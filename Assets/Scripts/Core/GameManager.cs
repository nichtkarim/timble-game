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
        // Falls das Script auf einem UI-Objekt (Canvas/RectTransform) liegt, wollen wir nicht den kompletten UI-Tree mit DontDestroyOnLoad mitnehmen.
        bool isUIRoot = GetComponent<Canvas>() != null || GetComponent<RectTransform>() != null;

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (!isUIRoot)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("GameManager läuft auf UI-Root, wird nicht persistent gemacht, um HUDs nicht mitzuschleppen.");
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
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadScene aufgerufen mit leerem Szenennamen.");
            return;
        }

        Debug.Log($"Versuche Szene zu laden: '{sceneName}'");

        // Direkt versuchen, wenn vorhanden
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.sceneLoaded += OnSceneLoadedOnce;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return;
        }

        // Fallback: Case-insensitive Match gegen Build-Settings suchen (für case-sensitive Filesysteme)
        string matchedByCase = FindSceneNameCaseInsensitive(sceneName);
        if (!string.IsNullOrEmpty(matchedByCase))
        {
            Debug.LogWarning($"Szene '{sceneName}' nicht gefunden, lade case-korrigierte Szene '{matchedByCase}'.");
            SceneManager.sceneLoaded += OnSceneLoadedOnce;
            SceneManager.LoadScene(matchedByCase, LoadSceneMode.Single);
            return;
        }

        Debug.LogError($"Szene '{sceneName}' konnte nicht geladen werden. Ist sie in den Build Settings? -> File > Build Settings...");
    }

    string FindSceneNameCaseInsensitive(string requested)
    {
        try
        {
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(path)) continue;
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, requested, System.StringComparison.OrdinalIgnoreCase))
                {
                    return name; // korrekt geschriebener Name aus BuildSettings
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"FindSceneNameCaseInsensitive Exception: {ex.Message}");
        }
        return null;
    }

    void OnSceneLoadedOnce(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedOnce;
        Debug.Log($"Szene geladen: '{scene.name}' (Modus: {mode})");
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
