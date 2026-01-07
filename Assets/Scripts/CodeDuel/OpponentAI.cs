using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OpponentAI : MonoBehaviour
{
    public float BaseReactionTime = 1.2f; // Langsamere Reaktion um fair zu sein
    public float ErrorChance = 0.05f; // 5% Chance zu versagen

    private CodeDuelManager _manager;
    private Coroutine _aiRoutine;

    public void Initialize(CodeDuelManager manager)
    {
        _manager = manager;
    }

    public void StartInputRoutine(List<int> sequence)
    {
        Stop();
        _aiRoutine = StartCoroutine(InputRoutine(sequence));
    }

    public void Stop()
    {
        if (_aiRoutine != null) StopCoroutine(_aiRoutine);
    }

    IEnumerator InputRoutine(List<int> sequence)
    {
        // Anfangsverzögerung
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

        foreach (int key in sequence)
        {
            // Verzögerung pro Taste
            float delay = BaseReactionTime + Random.Range(-0.1f, 0.1f);
            if (delay < 0.1f) delay = 0.1f;
            yield return new WaitForSeconds(delay);

            // Fehlerprüfung
            if (Random.value < ErrorChance)
            {
                // KI Versagt
                _manager.OnOpponentFinished(false);
                yield break;
            }
        }

        // Erfolgreich abgeschlossen
        _manager.OnOpponentFinished(true);
    }
}
