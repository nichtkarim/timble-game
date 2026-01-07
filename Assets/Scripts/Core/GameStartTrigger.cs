using UnityEngine;
using TMPro;

public class GameStartTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject StartButtonObject; 
    public UnityEngine.UI.Button StartButton;
    public TMP_Text StartButtonText;
    
    [Header("Preset Settings")]
    public InteractionPreset Preset = InteractionPreset.Custom;
    public enum InteractionPreset { Custom, ShellGame, CodeDuel }

    [Header("Active Settings")]
    public Vector3 PlayerStartPosition;
    public Vector3 PlayerStartRotation;
    public Vector3 CameraStartPosition;
    public Vector3 CameraStartRotation;

    private void OnValidate()
    {
        if (Preset == InteractionPreset.ShellGame)
        {
            PlayerStartPosition = new Vector3(-0.373f, 4.312f, -3.624f); 
            CameraStartPosition = new Vector3(0.0707f, 0.35f, 0.811f);
        }
        else if (Preset == InteractionPreset.CodeDuel)
        {
            PlayerStartPosition = new Vector3(6.73764896f, 0.649999976f, -9.44915962f);
            PlayerStartRotation = new Vector3(0, 96, 0); 
            CameraStartPosition = new Vector3(0.0706850588f, 0.350000024f, 0.81099999f);
            CameraStartRotation = Vector3.zero;
        }
    }

    [Header("Target Manager")]
    public ShellGameManager ShellManager;
    public CodeDuelManager DuelManager;
    [Header("Level Transitions")]
    public LevelTransitionTrigger TransitionTrigger;
    
    private FirstPersonController _player;
    private bool _isPlaying = false;

    private void Start()
    {
        if (StartButtonObject) StartButtonObject.SetActive(false);
        if (StartButton) StartButton.onClick.AddListener(OnInteract);

        if (ShellManager)
        {
            ShellManager.OnVictory.AddListener(OnShellGameWon);
        }
        
        if (DuelManager)
        {
            DuelManager.OnVictory.AddListener(OnCodeDuelWon);
        }
    }

    private void OnDestroy()
    {
        if (ShellManager)
        {
            ShellManager.OnVictory.RemoveListener(OnShellGameWon);
        }
        
        if (DuelManager)
        {
            DuelManager.OnVictory.RemoveListener(OnCodeDuelWon);
        }
    }

    void OnShellGameWon()
    {
        // 1. Erzwinge Verlassen des "Spielmodus" (Aufstehen)
        if (_isPlaying)
        {
            OnInteract(); // Dies schaltet isPlaying auf false und setzt Spielerposition zurück
        }

        // 2. Verstecke Button/Verhindere erneuten Eintritt falls gewünscht, oder ändere Text
        if (StartButtonObject) StartButtonObject.SetActive(false); // Kann nicht sofort wieder gespielt werden
        
        // 3. Entsperre nächsten Weg
        if (TransitionTrigger)
        {
             TransitionTrigger.UnlockPath();
        }
    }
    
    void OnCodeDuelWon()
    {
        // 1. Erzwinge Verlassen des "Spielmodus" (Aufstehen)
        if (_isPlaying)
        {
            OnInteract(); // Dies schaltet isPlaying auf false und setzt Spielerposition zurück
        }

        // 2. Verstecke Button/Verhindere erneuten Eintritt falls gewünscht, oder ändere Text
        if (StartButtonObject) StartButtonObject.SetActive(false); // Kann nicht sofort wieder gespielt werden
        
        // 3. Entsperre nächsten Weg
        if (TransitionTrigger)
        {
             TransitionTrigger.UnlockPath();
        }
    }
    
    void OnInteract()
    {
        if (!_isPlaying)
        {
            // SPIEL STARTEN
            _isPlaying = true;
            if (StartButtonText) StartButtonText.text = "Spiel Verlassen";

            // Teleportiere und Sperre
            if (_player)
            {
                CharacterController cc = _player.GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                
                // 1. Bewege Körper
                _player.transform.position = PlayerStartPosition;
                _player.transform.rotation = Quaternion.Euler(PlayerStartRotation); 
                
                // 2. Bewege Kamera (Kopf)
                if (_player.PlayerCamera)
                {
                    _player.PlayerCamera.transform.localPosition = CameraStartPosition; 
                    _player.PlayerCamera.transform.localRotation = Quaternion.Euler(CameraStartRotation); 
                }

                if (cc) cc.enabled = true;

                _player.enabled = false; // Stoppe Bewegung
                _player.LockCursor(false); // Halte Cursor frei
            }

            if (ShellManager)
            {
                if (_player && _player.PlayerCamera) ShellManager.SetInputCamera(_player.PlayerCamera);
                ShellManager.BeginGame();
            }
            if (DuelManager)
            {
                if (_player && _player.PlayerCamera) DuelManager.SetInputCamera(_player.PlayerCamera);
                DuelManager.BeginGame();
            }
        }
        else
        {
            // SPIEL VERLASSEN
            _isPlaying = false;
            if (StartButtonText) StartButtonText.text = "Spiel Starten";

            if (ShellManager) ShellManager.EndGame();
            if (DuelManager) DuelManager.EndGame();

            // Entsperre
            if (_player)
            {
                _player.enabled = true;
                _player.LockCursor(true); 
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.GetComponent<FirstPersonController>();
            if (StartButtonObject) StartButtonObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (StartButtonObject) StartButtonObject.SetActive(false);
            // Optional _player löschen, aber es zu behalten könnte für Randfälle sicherer sein
            // _player = null; 
        }
    }
}
