using UnityEngine;
using System.Collections;

public class Bodyguard : MonoBehaviour
{
    public Transform MoveTarget;
    public float MoveSpeed = 2.0f;
    public string WalkAnimationTrigger = "Walk"; // Falls Mecanim verwendet wird
    public string IdleAnimationTrigger = "Idle";

    private Animator _animator;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        
        // Stelle sicher, dass Leibwächter nicht durch Physik fällt
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
    }

    public void MoveAside()
    {
        if (MoveTarget != null)
        {
            StartCoroutine(MoveRoutine(MoveTarget.position));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination)
    {
        if (_animator) _animator.SetTrigger(WalkAnimationTrigger);

        // Speichere die Start-Y-Position um Höhe beizubehalten
        float startY = transform.position.y;

        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            Vector3 targetPos = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);
            
            // Halte Y-Position konstant um Fallen zu verhindern
            targetPos.y = startY;
            transform.position = targetPos;
            
            // Schaue zum Ziel (nur horizontale Rotation)
            Vector3 dir = (destination - transform.position);
            dir.y = 0; // Ignoriere vertikale Differenz
            dir.Normalize();
            
            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            }

            yield return null;
        }

        if (_animator) _animator.SetTrigger(IdleAnimationTrigger);
    }
}
