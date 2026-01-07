using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CodeDuelUI : MonoBehaviour
{
    public TextMeshProUGUI PlayerLivesText;
    public TextMeshProUGUI OpponentLivesText;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI SequenceDisplay; // e.g., "RED BLUE GREEN"
    public TextMeshProUGUI ItemDescriptionText;

    public void UpdateLives(int player, int opponent)
    {
        PlayerLivesText.text = $"Du: {player}";
        OpponentLivesText.text = $"Gegner: {opponent}";
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

    public void UpdateStatus(string text)
    {
        StatusText.text = text;
    }

    // ShowSequence ist zugunsten von direktem Button-Blinken (Simon Says Stil) veraltet
    public void ResetSequenceDisplay()
    {
        SequenceDisplay.text = "...";
    }

    public void SetHUDActive(bool active)
    {
        if (PlayerLivesText) PlayerLivesText.gameObject.SetActive(active);
        if (OpponentLivesText) OpponentLivesText.gameObject.SetActive(active);
        if (StatusText) StatusText.gameObject.SetActive(active);
        if (SequenceDisplay) SequenceDisplay.gameObject.SetActive(active);
        if (ItemDescriptionText) ItemDescriptionText.gameObject.SetActive(active);
    }
}
