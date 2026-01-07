using UnityEngine;

public class TableItem : MonoBehaviour
{
    public enum ItemType { LifeBoost, IntuitionBoost }
    
    [Header("Item Info")]
    public string ItemName;
    public string ItemDescription;
    public ItemType Type;
    public float BoostAmount = 1f;

    [Header("References")]
    public ShellGameManager ShellManager;
    public CodeDuelManager DuelManager;

    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        Debug.Log($"Interagiere mit {ItemName}");
        
        // Wende Boost an
        if (Type == ItemType.LifeBoost)
        {
            if (ShellManager != null) ShellManager.AddLife((int)BoostAmount);
            if (DuelManager != null) DuelManager.AddLife((int)BoostAmount);
        }
        else if (Type == ItemType.IntuitionBoost)
        {
            if (ShellManager != null && ShellManager.Intuition != null)
            {
                ShellManager.Intuition.AddIntuition(BoostAmount);
            }
        }

        // Zeige Beschreibung in UI
        if (ShellManager != null && ShellManager.UI != null)
        {
            ShellManager.UI.ShowItemDescription(ItemName, ItemDescription);
        }
        if (DuelManager != null && DuelManager.UI != null)
        {
            DuelManager.UI.ShowItemDescription(ItemName, ItemDescription);
        }

        // Spiele Sound ab falls möglich
        if (SoundManager.Instance) SoundManager.Instance.PlayWin(); // Wiederverwendung des Gewinn-Sounds für Aufsammeln

        // Entferne vom Tisch
        gameObject.SetActive(false);
    }
}
