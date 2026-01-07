using UnityEngine;
using System.Collections;

public class EndGameCinematic : MonoBehaviour
{
    [Header("Settings")]
    public Transform ExitPoint;
    public Vector3 SkyLookRotation = new Vector3(-60, 0, 0); // Nach oben schauen
    public float WalkDuration = 8.0f;
    public float LookAtSkyDuration = 5.0f;

    public void StartCinematic(FirstPersonController player)
    {
        StartCoroutine(CinematicRoutine(player));
    }

    private IEnumerator CinematicRoutine(FirstPersonController player)
    {
        Debug.Log("[Cinematic] Starte 13s Endspiel-Sequenz...");
        
        // 1. Deaktiviere Spielersteuerung
        player.enabled = false;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        // 2. Spiele Fußschritte ab
        if (SoundManager.Instance) SoundManager.Instance.PlayFootsteps();

        // 3. Langsamer Gang zum Ausgang
        if (ExitPoint != null)
        {
            Vector3 startPos = player.transform.position;
            Quaternion startRot = player.transform.rotation;
            Vector3 endPos = ExitPoint.position;
            Quaternion endRot = ExitPoint.rotation;

            float elapsed = 0;
            while (elapsed < WalkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / WalkDuration;
                // Benutze SmoothStep für natürlicheres Gefühl
                float smoothT = Mathf.SmoothStep(0, 1, t);

                player.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
                player.transform.rotation = Quaternion.Slerp(startRot, endRot, smoothT);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[Cinematic] Kein ExitPoint zugewiesen! Überspringe Gang.");
            yield return new WaitForSeconds(WalkDuration);
        }

        // 4. Stoppe Fußschritte
        if (SoundManager.Instance) SoundManager.Instance.StopFootsteps();

    // 5. Blicksequenz (Hoch, dann Links, dann Rechts)
    if (player.PlayerCamera != null)
    {
        float phaseDuration = LookAtSkyDuration / 3.0f;
        
        // Phase A: Nach oben schauen
        yield return StartCoroutine(RotateCamera(player.PlayerCamera, Quaternion.Euler(SkyLookRotation), phaseDuration));
        
        // Phase B: Nach links schauen
        Vector3 leftLook = SkyLookRotation + new Vector3(0, -45, 0);
        yield return StartCoroutine(RotateCamera(player.PlayerCamera, Quaternion.Euler(leftLook), phaseDuration));

        // Phase C: Nach rechts schauen
        Vector3 rightLook = SkyLookRotation + new Vector3(0, 45, 0);
        yield return StartCoroutine(RotateCamera(player.PlayerCamera, Quaternion.Euler(rightLook), phaseDuration));
    }

    Debug.Log("[Cinematic] Sequenz abgeschlossen. Löse Sieg aus.");
    
    // 6. Übergang zum Sieg
    if (GameManager.Instance)
    {
        GameManager.Instance.Victory();
    }
}

private IEnumerator RotateCamera(Camera cam, Quaternion targetRot, float duration)
{
    Quaternion startRot = cam.transform.localRotation;
    float elapsed = 0;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        cam.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
        yield return null;
    }
    }
}
