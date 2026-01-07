using UnityEngine;

public class ObjectRotationTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform TargetTransform;
    public Vector3 TargetRotation;
    public bool UseLocalRotation = true;

    [Header("Transition (Optional)")]
    public bool Animate = true;
    public float Duration = 1.0f;

    public void ApplyRotation()
    {
        if (TargetTransform == null) TargetTransform = transform;

        if (Animate)
        {
            StartCoroutine(RotateRoutine());
        }
        else
        {
            if (UseLocalRotation)
                TargetTransform.localRotation = Quaternion.Euler(TargetRotation);
            else
                TargetTransform.rotation = Quaternion.Euler(TargetRotation);
        }
    }

    private System.Collections.IEnumerator RotateRoutine()
    {
        Quaternion startRot = UseLocalRotation ? TargetTransform.localRotation : TargetTransform.rotation;
        Quaternion endRot = Quaternion.Euler(TargetRotation);
        float elapsed = 0;

        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / Duration);
            
            if (UseLocalRotation)
                TargetTransform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            else
                TargetTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
                
            yield return null;
        }
    }
}
