using UnityEngine;
using System.Collections;

public class SwitchButton : MonoBehaviour
{
    public int ButtonIndex;
    public CodeDuelManager Manager;

    [Header("Visual Settings")]
    public Color NormalColor = Color.gray;
    public Color GlowColor = Color.cyan;
    public float PressDepth = 0.1f; // Wie weit sich Button nach unten bewegt beim Drücken
    public float PressSpeed = 10f;

    private Renderer _meshRenderer;
    private Vector3 _originalPosition;
    private Color _originalColor;
    private Material _material;

    private void Awake()
    {
        // Hole 3D-Renderer
        _meshRenderer = GetComponent<Renderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponentInChildren<Renderer>();
        }

        if (_meshRenderer == null)
        {
            Debug.LogError($"[SwitchButton] '{name}' hat KEINEN Renderer! Kann keine visuelle Rückmeldung anzeigen.");
            return;
        }

        // Erstelle eine einzigartige Material-Instanz um andere Objekte nicht zu beeinflussen
        _material = _meshRenderer.material;
        
        // Speichere ursprüngliche Zustände
        _originalPosition = transform.localPosition;
        _originalColor = _material.color;

        Debug.Log($"[SwitchButton] '{name}' als 3D-Button initialisiert (Index {ButtonIndex})");
    }

    private float _lastClickTime = -999f;
    private const float DEBOUNCE_DELAY = 0.2f;

    public void OnClick()
    {
        Debug.Log($"[SwitchButton] Button '{name}' (Index {ButtonIndex}) geklickt!");
        
        if (Manager == null)
        {
            Debug.LogError($"[SwitchButton] Button '{name}' hat KEINEN MANAGER zugewiesen! Kann Klick nicht verarbeiten.");
            return;
        }
        
        if (Time.time - _lastClickTime < DEBOUNCE_DELAY)
        {
            Debug.Log($"[SwitchButton] Button '{name}' Klick ignoriert (Entprellung).");
            return;
        }
        
        _lastClickTime = Time.time;
        
        Debug.Log($"[SwitchButton] Button '{name}' sendet Eingabe {ButtonIndex} an Manager.");
        StartCoroutine(PressDown());
        Manager.OnPlayerInput(ButtonIndex);
        if (SoundManager.Instance) SoundManager.Instance.PlayClick();
    }

    /// <summary>
    /// Leuchteffekt für Gegnerzug (keine Bewegung)
    /// </summary>
    public IEnumerator FlashGlow()
    {
        if (_material == null) yield break;

        // Aktiviere Emission und setze Leuchtfarbe
        _material.EnableKeyword("_EMISSION");
        _material.SetColor("_EmissionColor", GlowColor * 2f); // Multipliziere für Helligkeit
        _material.color = GlowColor;
        
        yield return new WaitForSeconds(0.5f);
        
        // Zurück zu Normal
        _material.color = _originalColor;
        _material.SetColor("_EmissionColor", Color.black);
    }

    /// <summary>
    /// Herunterdrück-Effekt für Spielerzug
    /// </summary>
    public IEnumerator PressDown()
    {
        if (_material == null) yield break;

        // Berechne gedrückte Position (bewege nach unten auf Y-Achse)
        Vector3 pressedPosition = _originalPosition - new Vector3(0, PressDepth, 0);
        
        // Schnelles Herunterdrücken
        float elapsed = 0f;
        float pressDuration = 0.1f;
        
        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pressDuration;
            transform.localPosition = Vector3.Lerp(_originalPosition, pressedPosition, t);
            yield return null;
        }
        
        transform.localPosition = pressedPosition;
        
        // Optional: leichte Farbänderung während gedrückt
        _material.color = GlowColor * 0.8f;
        
        // Kurz halten
        yield return new WaitForSeconds(0.15f);
        
        // Zurück zur Originalposition
        elapsed = 0f;
        float returnDuration = 0.15f;
        
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            transform.localPosition = Vector3.Lerp(pressedPosition, _originalPosition, t);
            yield return null;
        }
        
        transform.localPosition = _originalPosition;
        _material.color = _originalColor;
    }

    /// <summary>
    /// Legacy Flash-Methode - leitet zu FlashGlow um für Kompatibilität
    /// </summary>
    public IEnumerator Flash()
    {
        yield return FlashGlow();
    }
}
