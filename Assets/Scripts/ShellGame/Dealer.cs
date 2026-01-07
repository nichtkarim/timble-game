using UnityEngine;

public class Dealer : MonoBehaviour
{
    [Header("Settings")]
    public float CheatChance = 0.25f; // 25%

    public bool IsCheating { get; private set; }

    public IntuitionSystem Intuition; // Referenz benötigt

    // Bestimmt ob der Dealer in dieser Runde betrügt
    public void PrepareRound()
    {
        // "Hohe Intuition = Hohe Chance dass Gegner betrügt"
        // "Niedrige Intuition = Niedrige Chance"
        if (Intuition != null)
        {
            // Skaliert sodass 100 Intuition = 40% Betrugs-Chance
            CheatChance = (Intuition.CurrentIntuition / 100f) * 0.4f; 
        }

        IsCheating = UnityEngine.Random.value <= CheatChance;
        if (IsCheating)
        {
            Debug.Log($"Dealer hat entschieden zu BETRÜGEN in dieser Runde. (Chance: {CheatChance:P0})");
        }
        else
        {
            Debug.Log($"Dealer spielt FAIR in dieser Runde. (Chance: {CheatChance:P0})");
        }
    }

    // Gibt true zurück, wenn Betrugsversuch stattfand
    public bool TryCheat()
    {
        return IsCheating;
    }
}
