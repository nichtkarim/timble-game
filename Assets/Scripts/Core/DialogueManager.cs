using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DialogueCondition { Correct, Wrong, Start, End }
public enum GameType { ShellGame, CodeDuel, Both }

[System.Serializable]
public class DialogueEntry
{
    public string Name;
    public AudioClip Audio;
    [TextArea(3, 10)]
    public string Transcription;
    public GameType Game;
    public DialogueCondition Condition;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Reference")]
    public DialogueUI DialogueUI;

    [Header("Audio Reference")]
    public AudioSource AudioSource;

    [Header("Dialouge Settings")]
    public List<DialogueEntry> DialogueEntries = new List<DialogueEntry>();

    [Header("Special Dialogues")]
    public DialogueEntry CodeDuelWinDialogue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (AudioSource == null)
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
        }

        // Befülle Datenstruktur automatisch, falls leer
        if (DialogueEntries.Count == 0)
        {
            PopulateDefaultEntries();
        }
    }

    private void PopulateDefaultEntries()
    {
        DialogueEntries.Clear();
        // Start/Ende
        AddEntry("Greeting", "So you're finally awake! Good, you've slept long enough. The rules are very simple: You fail-you stay! Now get started.", GameType.Both, DialogueCondition.Start);
        AddEntry("Farewell", "For the moment, they may leave. But don't be sentimental. We will see each other again!", GameType.Both, DialogueCondition.End);
        
        // Richtig
        AddEntry("Lucky", "You're lucky. You get to keep breathing for now.", GameType.Both, DialogueCondition.Correct);
        AddEntry("Dangerous Mix", "Fascinating. Stupidity combined with luck is a dangerous mix.", GameType.Both, DialogueCondition.Correct);
        AddEntry("Undeserved", "So a success then, huh? Pity. Let's not pretend that it was deserved.", GameType.Both, DialogueCondition.Correct);
        
        // Falsch
        AddEntry("Consequences", "Regrettable. You chose the wrong little hat. And mistakes here... let's just say they have consequences.", GameType.ShellGame, DialogueCondition.Wrong);
        AddEntry("Punished", "Incorrect sequences will be punished.", GameType.CodeDuel, DialogueCondition.Wrong);
        AddEntry("Ignoring Patterns", "They are playing for their lives and ignoring patterns?!", GameType.CodeDuel, DialogueCondition.Wrong);
        AddEntry("Think Later", "Interesting. You press first and think later.", GameType.CodeDuel, DialogueCondition.Wrong);
        AddEntry("Graceful Failure", "Amazing. You fail with a grace that almost suggests it is intentional.", GameType.Both, DialogueCondition.Wrong);
        AddEntry("Self-Confident", "Ah. So this kind of person... Self-confident and consistently wrong!", GameType.Both, DialogueCondition.Wrong);
    }

    private void AddEntry(string name, string text, GameType game, DialogueCondition cond)
    {
        DialogueEntries.Add(new DialogueEntry { Name = name, Transcription = text, Game = game, Condition = cond });
    }

    /// <summary>
    /// Spiele einen spezifischen Dialog nach Namen ab
    /// </summary>
    public void PlayNamedDialogue(string name)
    {
        DialogueEntry entry = DialogueEntries.Find(e => e.Name == name);
        if (entry != null)
        {
            StartCoroutine(PlayDialogueRoutine(entry));
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Dialog mit Namen '{name}' nicht gefunden!");
        }
    }

    /// <summary>
    /// Spiele den spezifischen CodeDuel-Sieg-Dialog ab
    /// </summary>
    public void PlayCodeDuelWinDialogue()
    {
        if (CodeDuelWinDialogue != null && CodeDuelWinDialogue.Audio != null)
        {
            StartCoroutine(PlayDialogueRoutine(CodeDuelWinDialogue));
        }
        else
        {
            Debug.LogWarning("[DialogueManager] CodeDuelWinDialogue nicht zugewiesen oder Audio fehlt!");
        }
    }

    /// <summary>
    /// Spiele einen zufälligen Dialog basierend auf Spielsituation ab
    /// </summary>
    public void PlaySituationalDialogue(GameType game, DialogueCondition condition)
    {
        List<DialogueEntry> matches = DialogueEntries.FindAll(e => 
            (e.Game == game || e.Game == GameType.Both) && 
            e.Condition == condition
        );

        if (matches.Count > 0)
        {
            DialogueEntry selected = matches[Random.Range(0, matches.Count)];
            StartCoroutine(PlayDialogueRoutine(selected));
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Kein Dialog gefunden für {game} / {condition}");
        }
    }

    private IEnumerator PlayDialogueRoutine(DialogueEntry entry)
    {
        if (entry.Audio == null) yield break;

        // Falls bereits etwas abgespielt wird, entweder stoppen oder warten? Üblicherweise stoppen/unterbrechen für Dialog
        AudioSource.Stop();
        
        // Zeige Untertitel
        if (DialogueUI != null)
        {
            DialogueUI.ShowText(entry.Transcription);
        }

        // Spiele Audio ab
        AudioSource.clip = entry.Audio;
        AudioSource.Play();

        // Warte bis Clip fertig ist + kleiner Puffer
        yield return new WaitForSeconds(entry.Audio.length + 0.5f);

        // Verstecke Untertitel
        if (DialogueUI != null)
        {
            DialogueUI.Hide();
        }
    }
}
