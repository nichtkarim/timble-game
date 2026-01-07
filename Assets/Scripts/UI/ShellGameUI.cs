using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShellGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI PlayerLivesText;
    public TextMeshProUGUI DealerLivesText;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI IntuitionText; // "Your intuition says..."
    public Slider IntuitionSlider; // Optional bar
    public Image FlashOverlay; // Cyan flash
    public Button AccuseButton;
    public TextMeshProUGUI ItemDescriptionText; // For item pickups

    private void Start()
    {
        FlashOverlay.canvasRenderer.SetAlpha(0);
        IntuitionText.text = "";
        if (ItemDescriptionText) ItemDescriptionText.text = "";
    }

    public void UpdateLives(int player, int dealer)
    {
        PlayerLivesText.text = $"Spieler Leben: {player}";
        DealerLivesText.text = $"Dealer Leben: {dealer}";
    }

    public void UpdateIntuition(float value)
    {
        if (IntuitionSlider != null)
        {
            IntuitionSlider.value = value / 100f; // Assuming 0-100 input, slider usually 0-1
        }
    }

    public void UpdateStatus(string msg)
    {
        StatusText.text = msg;
    }

    public void ShowIntuitionWarning()
    {
        StartCoroutine(FlashRoutine());
        IntuitionText.text = "Deine Intuition sagt: Der Dealer hat betrogen!";
        // Verstecke Text automatisch nach einer Weile oder beim nächsten Zustand?
        Invoke("ClearIntuitionText", 3f);
    }

    void ClearIntuitionText()
    {
        IntuitionText.text = "";
    }

    public void ShowItemDescription(string name, string desc)
    {
        if (ItemDescriptionText)
        {
            ItemDescriptionText.text = $"<b>{name}</b>: {desc}";
            CancelInvoke("ClearItemDescription");
            Invoke("ClearItemDescription", 4f);
        }
    }

    void ClearItemDescription()
    {
        if (ItemDescriptionText) ItemDescriptionText.text = "";
    }

    IEnumerator FlashRoutine()
    {
        FlashOverlay.color = Color.cyan;
        FlashOverlay.CrossFadeAlpha(0.5f, 0.1f, false);
        yield return new WaitForSeconds(0.1f);
        FlashOverlay.CrossFadeAlpha(0f, 0.5f, false);
        FlashOverlay.CrossFadeAlpha(0f, 0.5f, false);
    }

    public void SetHUDActive(bool active)
    {
        if (PlayerLivesText) PlayerLivesText.gameObject.SetActive(active);
        if (DealerLivesText) DealerLivesText.gameObject.SetActive(active);
        if (StatusText) StatusText.gameObject.SetActive(active);
        if (IntuitionText) IntuitionText.gameObject.SetActive(active);
        if (IntuitionSlider) IntuitionSlider.gameObject.SetActive(active);
        if (AccuseButton) AccuseButton.gameObject.SetActive(active);
        if (ItemDescriptionText) ItemDescriptionText.gameObject.SetActive(active);
    }
}
