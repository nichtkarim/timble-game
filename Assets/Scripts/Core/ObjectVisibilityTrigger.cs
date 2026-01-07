using UnityEngine;

public class ObjectVisibilityTrigger : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] TargetObjects;
    public bool Deactivate = true;
    public bool DestroyObject = true;
    public bool AutoHookToCodeDuel = true;

    private void Start()
    {
        if (AutoHookToCodeDuel)
        {
            CodeDuelManager manager = FindFirstObjectByType<CodeDuelManager>();
            if (manager != null)
            {
                manager.OnGameWon.AddListener(ApplyChange);
            }
        }
    }

    public void ApplyChange()
    {
        if (TargetObjects == null || TargetObjects.Length == 0)
        {
            ApplyToSingle(gameObject);
            return;
        }

        foreach (var obj in TargetObjects)
        {
            if (obj != null) ApplyToSingle(obj);
        }
    }

    private void ApplyToSingle(GameObject target)
    {
        if (DestroyObject)
        {
            Destroy(target);
        }
        else
        {
            target.SetActive(!Deactivate);
        }
    }
}
