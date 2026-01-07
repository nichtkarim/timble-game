using UnityEngine;
using System.Collections;

public class Cup : MonoBehaviour
{
    public int CupId;
    public bool HasBall { get; private set; } = false;
    public float MoveSpeed = 5.0f;
    public float LiftHeight = 1.0f;

    private Vector3 _startPosition;
    private ShellGameManager _gameManager;

    public void Initialize(int id, ShellGameManager manager)
    {
        CupId = id;
        _gameManager = manager;
        _startPosition = transform.position;
    }

    public void SetBall(bool hasBall)
    {
        HasBall = hasBall;
    }

    // OnMouseDown entfernt zugunsten von Zentralisiertem Raycast in ShellGameManager
    // Dies vermeidet Probleme mit Kamera-Setup oder individueller Objekt-Logik.

    public IEnumerator MoveTo(Vector3 targetPos)
    {
        float t = 0;
        Vector3 start = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime * MoveSpeed;
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
    }

    public IEnumerator MoveShuffle(Vector3 targetPos, Vector3 curveOffset)
    {
        float t = 0;
        Vector3 start = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime * MoveSpeed;
            // Sinuswelle für Kurve: 0 am Start, 1 in der Mitte, 0 am Ende
            float arc = Mathf.Sin(t * Mathf.PI);
            Vector3 linearPos = Vector3.Lerp(start, targetPos, t);
            transform.position = linearPos + (curveOffset * arc);
            yield return null;
        }
        transform.position = targetPos;
    }

    public IEnumerator LiftCup()
    {
        // Berechne immer von echter Grundposition
        Vector3 downPos = new Vector3(transform.position.x, _startPosition.y, transform.position.z);
        Vector3 upPos = downPos + Vector3.up * LiftHeight;

        // Stelle sicher, dass wir nicht-geerdeten Start sanft behandeln falls nötig, aber vorerst bewege nur nach oben
        yield return MoveTo(upPos);
        
        // Warte
        yield return new WaitForSeconds(1f);

        // Nach unten
        yield return MoveTo(downPos);
    }

    // Reveal hebt nur hoch und bleibt oben (oder geht runter, Logik hängt vom Spielablauf ab)
    // Vorerst hebe einfach hoch
    public IEnumerator EnableReveal()
    {
        Vector3 downPos = new Vector3(transform.position.x, _startPosition.y, transform.position.z);
        Vector3 upPos = downPos + Vector3.up * LiftHeight;
        yield return MoveTo(upPos);
    }

    public void SnapToGround()
    {
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, _startPosition.y, currentPos.z);
    }
    
    public IEnumerator ResetPosition()
    {
        // Gehe zurück nach unten zur ursprünglichen Höhe, aber behalte aktuelle X/Z-Position
        Vector3 currentPos = transform.position;
        // _startPosition sollte bei Start oder Initialize gesetzt werden.
        // Falls Initialize nicht aufgerufen wird, könnte _startPosition null sein. Lassen wir das in Awake oder verlangen Init.
        float targetY = _startPosition.y; 
        
        Vector3 targetPos = new Vector3(currentPos.x, targetY, currentPos.z);
        yield return MoveTo(targetPos);
    }

    private void Awake()
    {
        _startPosition = transform.position;
    }
}
