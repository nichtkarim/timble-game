using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CodeDuelWinSequence : MonoBehaviour
{
    [Header("Door References")]
    public Transform LeftDoor;
    public Transform RightDoor;
    public Vector3 LeftDoorOpenOffset = new Vector3(-2, 0, 0);
    public Vector3 RightDoorOpenOffset = new Vector3(2, 0, 0);
    public float DoorOpenDuration = 1.5f;

    [Header("Gate References")]
    public Transform LeftGate;
    public Transform RightGate;
    public Vector3 LeftGateOpenOffset = new Vector3(-2, 0, 0);
    public Vector3 RightGateOpenOffset = new Vector3(2, 0, 0);
    public float GateOpenDuration = 1.5f;

    [Header("Level References")]
    public LevelTransitionTrigger TransitionTrigger;
    public List<Bodyguard> Guards;

    [Header("Sequence Settings")]
    public float PreSequenceDelay = 1.0f;

    public void PlaySequence(System.Action onComplete)
    {
        StartCoroutine(SequenceRoutine(onComplete));
    }

    private IEnumerator SequenceRoutine(System.Action onComplete)
    {
        Debug.Log($"[WinSequence] Warte {PreSequenceDelay} Sekunden vor dem Start...");
        yield return new WaitForSeconds(PreSequenceDelay);

        Debug.Log("[WinSequence] Starte Tür-Öffnungs-Sequenz...");
        // 1. Öffne Türen
        yield return StartCoroutine(MoveObjects(LeftDoor, RightDoor, LeftDoorOpenOffset, RightDoorOpenOffset, DoorOpenDuration));

        yield return new WaitForSeconds(0.5f);

        Debug.Log("[WinSequence] Starte Tor-Öffnungs-Sequenz...");
        // 2. Öffne Tore
        yield return StartCoroutine(MoveObjects(LeftGate, RightGate, LeftGateOpenOffset, RightGateOpenOffset, GateOpenDuration));

        yield return new WaitForSeconds(0.5f);

        Debug.Log("[WinSequence] Bewege Wachen und Entsperre Weg...");
        // 3. Bewege Wachen und Entsperre
        if (TransitionTrigger != null)
        {
            TransitionTrigger.MoveBodyguards();
            TransitionTrigger.SetLocked(false);
        }
        
        // Bewege auch Wachen in der Fallback-Liste, falls sie nicht im Trigger sind
        foreach (var guard in Guards)
        {
            if (guard) guard.MoveAside();
        }

        // Warte bis Wachen sich bewegt haben
        yield return new WaitForSeconds(2.0f);

        Debug.Log("[WinSequence] Sequenz abgeschlossen.");
        // 4. Abgeschlossen
        onComplete?.Invoke();
    }

    private IEnumerator MoveObjects(Transform left, Transform right, Vector3 leftOffset, Vector3 rightOffset, float duration)
    {
        if (left == null && right == null) yield break;

        Vector3 leftStartPos = left != null ? left.localPosition : Vector3.zero;
        Vector3 rightStartPos = right != null ? right.localPosition : Vector3.zero;
        
        Vector3 leftEndPos = leftStartPos + leftOffset;
        Vector3 rightEndPos = rightStartPos + rightOffset;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            if (left) left.localPosition = Vector3.Lerp(leftStartPos, leftEndPos, t);
            if (right) right.localPosition = Vector3.Lerp(rightStartPos, rightEndPos, t);

            yield return null;
        }
    }
}
