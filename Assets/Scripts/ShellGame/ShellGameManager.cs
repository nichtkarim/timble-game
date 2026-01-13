using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ShellGameManager : MonoBehaviour
{
    [Header("Components")]
    public IntuitionSystem Intuition;
    public UnityEvent OnVictory;
    public Dealer Dealer;
    public ShellGameUI UI;
    public Transform[] CupPositions; // 4 positions
    public Cup[] Cups; // 4 cups
    public GameObject BallPrefab;
    [Header("Audio")]
    public AudioClip ShuffleSound;
    public AudioClip RevealSound;
    private GameObject _ballInstance;

    [Header("Game Settings")]
    public int DealerLives = 10;
    public int PlayerLives = 10;
    public int ShuffleCount = 40; // Harder
    public float ShuffleSpeed = 10f; // Much Faster
    

    private int _ballIndex = 1; // Mittlerer Becher initial
    private bool _canInteract = false;
    private float _initialShuffleSpeed; // Ursprüngliche Shuffle-Geschwindigkeit
    // private bool _roundActive = false; // Ungenutzt vorerst
    
    // Zustand
    private void Start()
    {
        InitializeGame();
    }
    
    private void OnEnable()
    {
        if (Intuition) Intuition.OnIntuitionChanged += HandleIntuitionChange;
    }

    private void OnDisable()
    {
        if (Intuition) Intuition.OnIntuitionChanged -= HandleIntuitionChange;
    }

    void HandleIntuitionChange(float val)
    {
        if (UI) UI.UpdateIntuition(val);
    }

    public void InitializeGame()
    {
        DealerLives = 10; 
        PlayerLives = 10;
        _initialShuffleSpeed = ShuffleSpeed; // Speichere ursprüngliche Geschwindigkeit
        Intuition.Initialize();
        
        // Erzeuge Ball
        if (_ballInstance != null)
        {
             Destroy(_ballInstance);
        }

        if (BallPrefab != null)
        {
            _ballInstance = Instantiate(BallPrefab, Cups[_ballIndex].transform.position, Quaternion.identity);
        }
        
        UpdateUI();
        if (UI) UI.SetHUDActive(false); // Zu Beginn VERSTECKT (Nur HUD, nicht ganze Interaktionen)
    }

    public void BeginGame()
    {
        if (UI) UI.SetHUDActive(true); // ANZEIGEN beim Start
        UI.UpdateStatus("Spiel Gestartet!");
        StartCoroutine(StartRound());
    }

    public void EndGame()
    {
        if (UI) UI.SetHUDActive(false); // VERSTECKEN beim Beenden
        StopAllCoroutines();
        // UI.UpdateStatus("Exited."); // Keine Notwendigkeit Text zu aktualisieren wenn versteckt
    }

    public void UpdateUI()
    {
        if (UI != null)
        {
            UI.UpdateLives(PlayerLives, DealerLives);
            UI.UpdateStatus("Bereit...");
        }
    }

    public void AddLife(int amount)
    {
        PlayerLives += amount;
        UpdateUI();
    }

    IEnumerator StartRound()
    {
        _canInteract = false;
        // _roundActive = true;
        
        // BECHER ZURÜCKSETZEN (Behebt Schwebefehler)
        foreach(var cup in Cups)
        {
             cup.SnapToGround();
        }
        yield return new WaitForSeconds(0.5f);

        Dealer.PrepareRound();
        UI.UpdateStatus("Mischen...");
        
        // 1. Zeige Ball
        yield return ShowBallUnderCup(_ballIndex);
        
        // 2. Verstecke Ball (Becher runter)
        yield return HideBall();

        // 3. Betrugskontrolle
        if (Dealer.IsCheating)
        {
            // Logik für Betrug könnte später hier hin
        }

        // Intuitionsprüfung
        if (Dealer.IsCheating)
        {
            if (Intuition.CheckForDetection())
            {
               UI.ShowIntuitionWarning();
               Debug.Log("Intuitions-WARNUNG ausgelöst!");
            }
        }
        
        // 4. Mischen (Visuell)
        yield return ShuffleRoutine();

        // 5. Spieler ist dran
        UI.UpdateStatus("Wo ist die Kugel?");
        _canInteract = true;
    }

    IEnumerator ShowBallUnderCup(int index)
    {
        // Bewege Ball zur Becher-Position
        if (_ballInstance != null)
        {
            _ballInstance.transform.SetParent(null); // Löse von Becher falls verbunden
            _ballInstance.transform.position = Cups[index].transform.position;
            _ballInstance.transform.rotation = Quaternion.identity;
            _ballInstance.SetActive(true);
        }
        
        // Hebe Becher
        yield return Cups[index].LiftCup();
    }

    IEnumerator HideBall()
    {
        if (_ballInstance != null)
        {
            _ballInstance.transform.SetParent(Cups[_ballIndex].transform);
            _ballInstance.transform.localPosition = Vector3.zero; // Zentriere ihn
            _ballInstance.SetActive(true);
        }
        yield return null;
    }

    IEnumerator ShuffleRoutine()
    {
        // Einfaches Tausch-Mischen
        for (int i = 0; i < ShuffleCount; i++)
        {
            int indexA = UnityEngine.Random.Range(0, Cups.Length);
            int indexB = UnityEngine.Random.Range(0, Cups.Length);
            
            while (indexA == indexB)
            {
                indexB = UnityEngine.Random.Range(0, Cups.Length);
            }

            // Tausche NICHT _ballIndex. 
            // Der Ball ist physisch an den Becher gebunden, also bewegt er sich MIT dem Becher.
            // Der Index im Cups[]-Array bleibt eindeutig für diese Cup-Instanz.

            // Tausche visuelle Positionen
            Vector3 posA = Cups[indexA].transform.position;
            Vector3 posB = Cups[indexB].transform.position;

            Cups[indexA].MoveSpeed = ShuffleSpeed; // Wende Schwierigkeits-Geschwindigkeit an
            Cups[indexB].MoveSpeed = ShuffleSpeed;

            // Entgegengesetzte Kurven sodass sie umeinander kreisen
            float curveDepth = 0.3f; // nach Bedarf anpassen
            Vector3 offsetA = Vector3.forward * curveDepth;
            Vector3 offsetB = Vector3.back * curveDepth;

            StartCoroutine(Cups[indexA].MoveShuffle(posB, offsetA));
            yield return Cups[indexB].MoveShuffle(posA, offsetB); // Warte bis einer fertig ist
            
            if (ShuffleSound && SoundManager.Instance) SoundManager.Instance.PlaySFX(ShuffleSound);
        }

        if (SoundManager.Instance) SoundManager.Instance.StopSFX();
    }

    private Camera _inputCamera;

    public void SetInputCamera(Camera cam)
    {
        _inputCamera = cam;
    }

    private void Update()
    {
        // Zentralisierte Eingabebehandlung für 3D-Objekte (Becher)
        if (Input.GetMouseButtonDown(0))
        {
            if (!_canInteract)
            {
                Debug.LogWarning("Klick Ignoriert: Spiel ist derzeit NICHT in der Interaktionsphase.");
                return;
            }

            Camera cam = _inputCamera != null ? _inputCamera : Camera.main;
            if (cam == null)
            {
                Debug.LogError("Keine Kamera für Raycast gefunden! Markiere MainCamera oder weise Spielerkamera zu.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f); // Visueller Debug

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"Raycast Treffer: {hit.collider.gameObject.name}");
                
                // Fix: Prüfe auch Parent (Collider könnte auf Kinder-Mesh sein)
                Cup cup = hit.collider.GetComponentInParent<Cup>();
                
                // Extra Robustheit: Haben wir den Ball selbst getroffen?
                if (cup == null && _ballInstance != null && hit.collider.gameObject == _ballInstance)
                {
                    cup = _ballInstance.GetComponentInParent<Cup>();
                }
                
                // Fallback: Prüfe Kinder (Vielleicht ist Collider auf Root, Skript auf Kind?)
                if (cup == null) cup = hit.collider.GetComponentInChildren<Cup>();

                if (cup != null)
                {
                    OnCupSelected(cup);
                }
                else
                {
                    Debug.LogError($"FALSCHES OBJEKT GETROFFEN: Du hast auf '{hit.collider.gameObject.name}' geklickt. Dieses Objekt (und seine Parents/Kinder) hat KEIN 'Cup'-Skript! Warnung: Hast du auf den Tisch geklickt?");
                }
            }
            else
            {
                Debug.Log("Raycast hat alles verfehlt. Überprüfe Kamera-Tag 'MainCamera' und Collider-Ebenen.");
            }
        }
    }

    public void OnCupSelected(Cup selectedCup)
    {
        if (!_canInteract) return;
        Debug.Log($"Becher {selectedCup.CupId} ausgewählt.");
        StartCoroutine(ResolveRound(selectedCup));
    }

    public void OnAccuseButton()
    {
        if (!_canInteract) return;
        StartCoroutine(ResolveAccusation());
    }

    IEnumerator ResolveRound(Cup pickedCup)
    {
        _canInteract = false;
        
        bool isCorrectCup = (pickedCup == Cups[_ballIndex]);
        bool cheat = Dealer.IsCheating; // Prüfe Betrugs-Status

        // Wenn es der richtige Becher ist und Dealer nicht betrügt, sollte der Ball hier sein.
        // Löse ihn, damit er auf dem Tisch bleibt wenn Becher sich hebt.
        if (isCorrectCup && !cheat)
        {
            if (_ballInstance != null)
            {
                _ballInstance.transform.SetParent(null);
                _ballInstance.transform.position = pickedCup.transform.position;
                _ballInstance.transform.rotation = Quaternion.identity;
                _ballInstance.SetActive(true);
            }
        }

        // Hebe ausgewählten Becher
        yield return pickedCup.EnableReveal();

        // Prüfe Inhalt
        bool hasBall = false;
        
        if (cheat)
        {
            hasBall = false;
        }
        else
        {
            // Falls wir ihn oben gelöst haben, ist childCount jetzt 0. Also prüfen wir isCorrectCup.
            if (isCorrectCup) 
            {
                hasBall = true;
            }
        }

        if (hasBall)
        {
            // Runde gewonnen
            DealerLives--;
            UI.UpdateStatus("Gefunden!");
            Intuition.ApplyPenalty(0); // Keine Strafe
            if (SoundManager.Instance) SoundManager.Instance.PlayWin();
            
            // Dialog
            if (DialogueManager.Instance) 
                DialogueManager.Instance.PlaySituationalDialogue(GameType.ShellGame, DialogueCondition.Correct);
        }
        else
        {
            PlayerLives--;
            
            if (cheat && isCorrectCup)
            {
                UI.UpdateStatus("Leer! (Gestohlen?)");
            }
            else
            {
                UI.UpdateStatus("Falsch!");
            }

            Intuition.ApplyPenalty(Intuition.WrongCupPenalty);
            if (SoundManager.Instance) SoundManager.Instance.PlayLose();

            // Dialog
            if (DialogueManager.Instance) 
                DialogueManager.Instance.PlaySituationalDialogue(GameType.ShellGame, DialogueCondition.Wrong);
        }

        yield return new WaitForSeconds(2f);
        CheckGameEnd();
    }
    
    IEnumerator ResolveAccusation()
    {
        _canInteract = false;
        UI.UpdateStatus("ANKLAGE!");
        yield return new WaitForSeconds(1f);

        if (Dealer.IsCheating)
        {
            // Richtige Anklage
            DealerLives -= 2;
            UI.UpdateStatus("ERWISCHT!");
        }
        else
        {
            // Falsche Anklage
            PlayerLives -= 2;
            UI.UpdateStatus("FALSCHE ANKLAGE!");
            Intuition.ApplyPenalty(Intuition.WrongAccusationPenalty);
        }
        
        yield return new WaitForSeconds(2f);
        CheckGameEnd();
    }

    void CheckGameEnd()
    {
        UpdateUI();
        
        if (PlayerLives <= 0)
        {
            GameManager.Instance.GameOver();
            return;
        }
        
        if (DealerLives <= 0)
        {
            // SIEG
            Debug.Log("Shell Game gewonnen! Öffne Weg...");
            OnVictory?.Invoke();

            // Abschieds-Dialog
            if (DialogueManager.Instance) 
                DialogueManager.Instance.PlaySituationalDialogue(GameType.Both, DialogueCondition.End);

            return;
        }
        
        // Dynamische Schwierigkeit: Graduell erhöhte ShuffleSpeed basierend auf Vorsprung
        int lifeAdvantage = PlayerLives - DealerLives;
        float targetSpeed = _initialShuffleSpeed;
        
        if (lifeAdvantage >= 4)
        {
            targetSpeed += 1f;
            UI.UpdateStatus($"Schwierigkeit ++++ erhöht!");
        }
        else if (lifeAdvantage >= 3)
        {
            targetSpeed += 0.66f;
            UI.UpdateStatus($"Schwierigkeit +++ erhöht!");
        }
        else if (lifeAdvantage >= 2)
        {
            targetSpeed += 0.33f;
            UI.UpdateStatus($"Schwierigkeit ++ erhöht!");
        }
        
        
        ShuffleSpeed = targetSpeed;
        Debug.Log($"Lebens-Vorsprung: {lifeAdvantage} | ShuffleSpeed: {ShuffleSpeed}");
        
        // Nächste Runde
        StartCoroutine(StartRound());
    }
}
