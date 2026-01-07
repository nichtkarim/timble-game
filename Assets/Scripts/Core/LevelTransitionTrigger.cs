using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class LevelTransitionTrigger : MonoBehaviour
{
    [Header("Configuration")]
    public GameState NextState; // The state to switch to (e.g., CodeDuel, Victory)
    public List<Bodyguard> Bodyguards;
    
    [Header("Status")]
    public bool IsLocked = true;

    [Header("Cinematic")]
    public EndGameCinematic EndCinematic;
    
    private BoxCollider _trigger;

    private void Awake()
    {
        _trigger = GetComponent<BoxCollider>();
        if (_trigger) _trigger.isTrigger = true;
    }

    public void UnlockPath()
    {
        IsLocked = false;
        MoveBodyguards();
        Debug.Log("Pfad freigeschaltet! Gehe in den Trigger, um fortzufahren.");
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    public void MoveBodyguards()
    {
        foreach (var bg in Bodyguards)
        {
            if (bg) bg.MoveAside();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLocked) return;

        if (other.CompareTag("Player"))
        {
            if (NextState == GameState.Victory && EndCinematic != null)
            {
                FirstPersonController player = other.GetComponent<FirstPersonController>();
                if (player != null)
                {
                    Debug.Log("Starte Endspiel-Cinematic...");
                    EndCinematic.StartCinematic(player);
                    return;
                }
            }

            Debug.Log($"Übergang zu {NextState}...");
            GameManager.Instance.ChangeState(NextState);
        }
    }
}
