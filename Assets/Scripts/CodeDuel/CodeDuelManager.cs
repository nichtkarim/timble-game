using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CodeDuelManager : MonoBehaviour
{
    [Header("Settings")]
    public int StartSequenceLength = 3;
    public float SequenceDisplayTime = 2.0f;
    public int MaxRounds = 3; 
    public int OpponentLives = 3;
    public int PlayerLives = 3;

    [Header("References")]
    public CodeDuelUI UI;
    public SwitchButton[] PlayerButtons; 
    public OpponentAI Opponent;
    public UnityEngine.Events.UnityEvent OnVictory;
    public UnityEngine.Events.UnityEvent OnGameWon; // Sofort
    public CodeDuelWinSequence WinSequence;

    [Header("Audio")]
    public AudioClip SequenceSound;
    public AudioClip InputSound;

    private List<int> _currentSequence = new List<int>();
    private List<int> _playerInput = new List<int>();
    private bool _inputPhase = false;
    private Camera _inputCamera;
    
    public void SetInputCamera(Camera cam)
    {
        _inputCamera = cam;
    }
    
    private void Update()
    {
        if (!_inputPhase) return;

        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = _inputCamera != null ? _inputCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SwitchButton btn = hit.collider.GetComponentInParent<SwitchButton>();
                if (btn == null) btn = hit.collider.GetComponentInChildren<SwitchButton>();
                
                if (btn != null)
                {
                    btn.OnClick();
                }
            }
        }
    }
    
    private void Start()
    {
        InitializeDuel();
    }

    public void InitializeDuel()
    {
        HashSet<GameObject> uniqueButtons = new HashSet<GameObject>();
        // 1. Sicherheitsprüfung: Falls Buttons nicht zugewiesen, versuche sie zu finden
        if (PlayerButtons == null || PlayerButtons.Length == 0)
        {
            Debug.LogWarning("PlayerButtons nicht im Inspector zugewiesen! Versuche Auto-Suche...");
            PlayerButtons = FindObjectsOfType<SwitchButton>();
            
            // Sortiere sie um konsistente Reihenfolge zu gewährleisten (z.B. nach Name oder X-Position)
            System.Array.Sort(PlayerButtons, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }

        if (PlayerButtons == null || PlayerButtons.Length == 0)
        {
            Debug.LogError("KRITISCHER FEHLER: Keine SwitchButton-Komponenten in der Szene gefunden! Spiel kann nicht gestartet werden.");
            UI.UpdateStatus("Setup-Fehler: Keine Buttons!");
            return;
        }

        for (int i = 0; i < PlayerButtons.Length; i++)
        {
            if (PlayerButtons[i] != null)
            {
                if (uniqueButtons.Contains(PlayerButtons[i].gameObject))
                {
                    Debug.LogError($"KRITISCHER FEHLER: Button '{PlayerButtons[i].name}' ist doppelt zugewiesen!");
                }
                uniqueButtons.Add(PlayerButtons[i].gameObject);
                PlayerButtons[i].ButtonIndex = i;
                PlayerButtons[i].Manager = this; // Stelle sicher, dass Manager zugewiesen ist
            }
        }
        UpdateUI();
        if (UI) UI.SetHUDActive(false); // Zu Beginn VERSTECKT
    }

    public void BeginGame()
    {
        Debug.Log("[CodeDuel] BeginGame() aufgerufen!");
        if (UI) UI.SetHUDActive(true); // ANZEIGEN beim Start
        UI.UpdateStatus("Duell Gestartet!");
        Debug.Log("[CodeDuel] Starte erste Runden-Coroutine...");
        StartCoroutine(StartRound());
    }

    public void EndGame()
    {
        if (UI) UI.SetHUDActive(false); // VERSTECKEN beim Beenden
        StopAllCoroutines();
    }

    void UpdateUI()
    {
        if (UI) UI.UpdateLives(PlayerLives, OpponentLives);
    }

    public void AddLife(int amount)
    {
        PlayerLives += amount;
        UpdateUI();
    }

    IEnumerator StartRound()
    {
        Debug.Log("[CodeDuel] ===== RUNDE STARTEN =====");
        
        // 1. SICHERHEITSPRÜFUNG
        if (PlayerButtons == null || PlayerButtons.Length == 0)
        {
            Debug.LogError("[CodeDuel] FATAL: Keine PlayerButtons.");
            UI.UpdateStatus("Fehler: Keine Buttons");
            yield break;
        }

        // 2. SETUP
        _inputPhase = false;
        Debug.Log("[CodeDuel] Eingabephase auf FALSE gesetzt (Gegner ist dran)");
        _playerInput.Clear();
        UI.UpdateStatus("Gegner Denkt...");
        yield return new WaitForSeconds(1.5f);

        // 3. SEQUENZ GENERIEREN
        _currentSequence.Clear();
        // Korrigiert: Gegner sollte mindestens StartSequenceLength Züge spielen
        // Wird schwieriger, wenn Gegner Leben verliert
        int bonusMoves = (MaxRounds - OpponentLives); // 0 am Start, steigt wenn Gegner verliert
        int length = StartSequenceLength + bonusMoves; 
        Debug.Log($"[CodeDuel] Generiere Sequenz der Länge {length} (Basis: {StartSequenceLength} + Bonus: {bonusMoves})");;
        
        for(int i=0; i<length; i++)
        {
            _currentSequence.Add(UnityEngine.Random.Range(0, PlayerButtons.Length));
        }
        
        Debug.Log($"[CodeDuel] Generierte Sequenz: [{string.Join(", ", _currentSequence)}]");

        // 4. SEQUENZ ABSPIELEN (Mit skalierter Darstellung)
        UI.UpdateStatus($"Gegner Spielt {length} Züge...");
        yield return new WaitForSeconds(1.0f);
        
        foreach (int btnIndex in _currentSequence)
        {
            // Expliziter Debug
            string btnName = PlayerButtons[btnIndex].name;
            Debug.Log($"[CodeDuel] > SIMON SAGT: {btnName} (Index {btnIndex})");
            UI.UpdateStatus($"SCHAU: {btnName}"); // Zeige Name auf Bildschirm

            // Löse visuellen Effekt aus - Benutze FlashGlow für Gegner (nur Leuchten, kein Drücken)
            StartCoroutine(PlayerButtons[btnIndex].FlashGlow());
            
            // Spiele Sound ab
            if (SequenceSound && SoundManager.Instance) SoundManager.Instance.PlaySFX(SequenceSound);

            // Warte für Flash + Pause
            yield return new WaitForSeconds(0.8f); 
        }

        // 5. SPIELER IST DRAN
        Debug.Log("[CodeDuel] ===== SPIELER IST DRAN - Setze _inputPhase auf TRUE =====");
        _inputPhase = true;
        UI.UpdateStatus("DEIN ZUG!");
        UI.ResetSequenceDisplay(); // Lösche alte Hinweise
        Debug.Log($"[CodeDuel] Warte auf Spielereingabe von {_currentSequence.Count} Buttons...");;
        
        // Timer
        StartCoroutine(RoundTimer(15f)); // Großzügige Zeit
    }

    // IEnumerator PlaySequenceOnButtons() entfernt - Logik in StartRound zusammengeführt zur Vereinfachung

    IEnumerator RoundTimer(float time)
    {
        float timer = time;
        while (timer > 0 && _inputPhase)
        {
            timer -= Time.deltaTime;
            // Optional: UI Timer hier aktualisieren
            yield return null;
        }

        if (_inputPhase)
        {
            // Zeit abgelaufen
            RoundEnd(false, "Zeit Abgelaufen!");
        }
    }

    public void OnPlayerInput(int buttonIndex)
    {
        Debug.Log($"[CodeDuel] OnPlayerInput aufgerufen mit buttonIndex: {buttonIndex}, _inputPhase: {_inputPhase}");
        
        if (!_inputPhase)
        {
            Debug.LogWarning($"[CodeDuel] Eingabe ABGELEHNT - Nicht in Eingabephase! (Button {buttonIndex})");
            return;
        }

        _playerInput.Add(buttonIndex);
        Debug.Log($"[CodeDuel] Spielereingabe hinzugefügt. Aktuelle Sequenz: [{string.Join(", ", _playerInput)}]");
        
        if (InputSound && SoundManager.Instance) SoundManager.Instance.PlaySFX(InputSound);

        int currentStep = _playerInput.Count - 1;
        if (currentStep >= _currentSequence.Count)
        {
            Debug.LogWarning($"[CodeDuel] Eingabe-Index {currentStep} überschreitet Sequenzlänge {_currentSequence.Count}");
            return;
        }

        Debug.Log($"[CodeDuel] Prüfe Schritt {currentStep}: Erwartet {_currentSequence[currentStep]}, Erhalten {buttonIndex}");
        
        if (_playerInput[currentStep] != _currentSequence[currentStep])
        {
            // Spielerfehler!
            string msg = $"FALSCH! Erw: {_currentSequence[currentStep]} Erh: {buttonIndex}";
            Debug.LogError(msg);
            RoundEnd(false, msg); // Zeige genaue Abweichung dem Benutzer
            return;
        }

        Debug.Log($"[CodeDuel] Richtig! Fortschritt: {_playerInput.Count}/{_currentSequence.Count}");
        if (_playerInput.Count == _currentSequence.Count)
        {
            // Spieler fertig!
            Debug.Log("[CodeDuel] Sequenz perfekt abgeschlossen!");
            RoundEnd(true, "PERFEKT!");
        }
    }

    // Veraltet, aber für Schnittstellenkompatibilität behalten (oder kann entfernt werden, falls nicht aufgerufen)
    public void OnOpponentFinished(bool success)
    {
        // Keine Operation im neuen Rundenbasierten Modus
    }

    void RoundEnd(bool playerWon, string reason)
    {
        _inputPhase = false;
        StopAllCoroutines(); 

        UI.UpdateStatus(reason);

        if (playerWon)
        {
            OpponentLives--;
        }
        else
        {
            PlayerLives--;
        }

        UpdateUI();

        if (PlayerLives <= 0)
        {
            GameManager.Instance.GameOver();
        }
        else if (OpponentLives <= 0)
        {
            Debug.Log("Code Duel gewonnen! Rufe sofortige GameWon-Logik auf...");
            OnGameWon?.Invoke(); // Sofort auslösen
            
            // Sieg-Dialog - Sofort abspielen
            if (DialogueManager.Instance) 
                DialogueManager.Instance.PlayCodeDuelWinDialogue();

            if (WinSequence != null)
            {
                Debug.Log("Starte Gewinnsequenz...");
                WinSequence.PlaySequence(() => {
                    Debug.Log("Gewinnsequenz abgeschlossen. Rufe Siegeslogik auf.");
                    OnVictory?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning("WinSequence-Referenz nicht zugewiesen! Überspringe zur Siegeslogik.");
                OnVictory?.Invoke();
            }
        }
        else
        {
            // Nur ein Rundenende, kein Spielende
            if (DialogueManager.Instance)
                DialogueManager.Instance.PlaySituationalDialogue(GameType.CodeDuel, playerWon ? DialogueCondition.Correct : DialogueCondition.Wrong);

            StartCoroutine(WaitAndRestart(3f)); // Längere Wartezeit zum Lesen der Nachricht
        }
    }

    IEnumerator WaitAndRestart(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartRound());
    }
}
