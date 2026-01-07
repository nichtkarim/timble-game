using UnityEngine;
using System;

public class IntuitionSystem : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 100)] public float CurrentIntuition = 100f;
    public float DecayRate = 1f; // 1% per second
    public float WrongCupPenalty = 5f;
    public float WrongAccusationPenalty = 20f;

    public event Action<float> OnIntuitionChanged;
    public event Action OnIntuitionWarning; // Cyan-Blitz-Event

    private bool _isActive = false;

    public void Initialize()
    {
        CurrentIntuition = 100f;
        _isActive = true;
        OnIntuitionChanged?.Invoke(CurrentIntuition);
    }

    private void Update()
    {
        if (!_isActive) return;

        if (CurrentIntuition > 0)
        {
            CurrentIntuition -= DecayRate * Time.deltaTime;
            if (CurrentIntuition < 0) CurrentIntuition = 0;
            OnIntuitionChanged?.Invoke(CurrentIntuition);
        }
    }

    public void ApplyPenalty(float amount)
    {
        CurrentIntuition -= amount;
        if (CurrentIntuition < 0) CurrentIntuition = 0;
        OnIntuitionChanged?.Invoke(CurrentIntuition);
    }

    public void AddIntuition(float amount)
    {
        CurrentIntuition += amount;
        if (CurrentIntuition > 100) CurrentIntuition = 100;
        OnIntuitionChanged?.Invoke(CurrentIntuition);
    }

    // Gibt true zurück, wenn der Spieler den Betrug "spürt" basierend auf aktueller Intuition
    public bool CheckForDetection()
    {
        // "Erkennungschance entspricht aktuellem Intuitionswert"
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool detected = roll <= CurrentIntuition;
        
        if (detected)
        {
            OnIntuitionWarning?.Invoke();
        }
        
        return detected;
    }

    public void StopIntuition()
    {
        _isActive = false;
    }
}
