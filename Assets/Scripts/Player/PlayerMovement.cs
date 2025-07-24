using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset inputActions;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction throwAction;
    private InputAction switchTargetAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Rigidbody rb;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float GamepadDeadZone = 0.1f;

    private bool isGampadConnected = false;

    [SerializeField] private SpawnGrenade grenadeThrower;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        lookAction = playerInput.actions.FindAction("Look");
        throwAction = playerInput.actions.FindAction("Throw");
        switchTargetAction = playerInput.actions.FindAction("SwitchTarget");

        throwAction.started += OnThrowStarted;
        throwAction.canceled += OnThrowCanceled;
        switchTargetAction.performed += OnSwitchTarget;

        playerInput.onControlsChanged += OnDeviceChange;
    }

    private void OnDisable()
    {
        throwAction.started -= OnThrowStarted;
        throwAction.canceled -= OnThrowCanceled;

        if (playerInput != null)
            playerInput.onControlsChanged -= OnDeviceChange;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Moveing();
        Rotating();
    }

    private void Moveing()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void Rotating()
    {
        if (isGampadConnected)
        {
            RotateWithGamepad();
        }
        else
        {
            RotateWithMouse();
        }
    }

    private void RotateWithGamepad()
    {
        if (Mathf.Abs(lookInput.x) > GamepadDeadZone || Mathf.Abs(lookInput.y) > GamepadDeadZone)
        {
            Vector3 playerDirection = Vector3.right * lookInput.x + Vector3.forward * lookInput.y;

            if (playerDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }
    }

    private void RotateWithMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 hitPoint = ray.GetPoint(rayDistance);
            LookAt(hitPoint);
        }
    }

    private void LookAt(Vector3 hitPoint)
    {
        Vector3 Point = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
        transform.LookAt(Point);
    }

    public void OnDeviceChange(PlayerInput input)
    {
        if (input != null && !string.IsNullOrEmpty(input.currentControlScheme))
        {
            isGampadConnected = input.currentControlScheme.Equals("Gamepad");
        }
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        // 是否使用手柄判断自动锁定
        bool isGamepad = playerInput.currentControlScheme == "Gamepad";
        grenadeThrower.StartAiming(isGamepad);
    }

    private void OnSwitchTarget(InputAction.CallbackContext ctx)
    {
        if (grenadeThrower != null)
        {
            grenadeThrower.LockToNextTarget();
        }
    }

    private void OnThrowCanceled(InputAction.CallbackContext ctx)
    {
        grenadeThrower.CancelAimingAndThrow();
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnDeviceChange;
        }
    }
}
