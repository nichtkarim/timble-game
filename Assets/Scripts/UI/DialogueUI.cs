using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI DialogueText;

    private void Awake()
    {
        if (DialoguePanel) DialoguePanel.SetActive(false);
    }

    public void ShowText(string text)
    {
        if (DialoguePanel) DialoguePanel.SetActive(true);
        if (DialogueText) DialogueText.text = text;
    }

    public void Hide()
    {
        if (DialoguePanel) DialoguePanel.SetActive(false);
    }
}
