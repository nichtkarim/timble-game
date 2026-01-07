using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float Gravity = -9.81f;
    public float JumpHeight = 1.0f;

    [Header("Look")]
    public Camera PlayerCamera;
    public float MouseSensitivity = 2f;
    public float LookXLimit = 80f;

    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    
    // Sperre Cursor für FPS-Gefühl
    private bool _cursorLocked = true;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        LockCursor(true);
    }

    void Update()
    {
        // 1. Bewegung
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        // Drücke linke Shift zum Rennen? Optional.
        float speed = MoveSpeed; 

        float curSpeedX = speed * Input.GetAxis("Vertical");
        float curSpeedY = speed * Input.GetAxis("Horizontal");
        float movementDirectionY = _moveDirection.y;

        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Springen
        if (Input.GetButton("Jump") && _characterController.isGrounded)
        {
            _moveDirection.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        // Schwerkraft
        if (!_characterController.isGrounded)
        {
            _moveDirection.y += Gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);

        // 2. Rotation
        if (_cursorLocked && PlayerCamera != null)
        {
            _rotationX += -Input.GetAxis("Mouse Y") * MouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -LookXLimit, LookXLimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * MouseSensitivity, 0);
        }
        
        // 3. Fußschritt-Sounds
        bool isMoving = (Mathf.Abs(curSpeedX) > 0.1f || Mathf.Abs(curSpeedY) > 0.1f) && _characterController.isGrounded;
        
        if (isMoving && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFootsteps();
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopFootsteps();
        }
        
        // Entsperre Cursor mit ESC (zum Testen oder UI-Interaktion)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(!_cursorLocked);
        }
    }
    
    public void LockCursor(bool lockState)
    {
        _cursorLocked = lockState;
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }
}
