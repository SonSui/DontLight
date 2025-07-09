using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerAction : NetworkBehaviour
{
    public InputActionAsset InputActions;

    private InputAction m_moveAction;
    private InputAction m_jumpAction;
    private InputAction m_lookAction;

    private PlayerInput playerInput;

    private Vector2 m_moveAmt;
    private Vector2 aim;

    private Rigidbody m_rigidbody;

    private bool isGampadConnected = false;

    private Camera localCamera;

    [Header("Walk Settings")]
    [SerializeField] private float rotateSpeed = 2f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float GamepadDeadZone = 0.1f;

    [Header("Fixed Camera Settings")]
    [SerializeField] private Vector3 fixedCameraPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 fixedCameraRotation = new Vector3(0, 0, 0);
    [SerializeField] private float fixedCameraOrthoSize = 10f; // 如果是正交相机

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");
        m_lookAction = InputSystem.actions.FindAction("Look");

        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            SetupFixedCamera();
        }
    }

    private void SetupFixedCamera()
    {
        // 创建固定视角相机
        GameObject cameraObj = new GameObject("FixedPlayerCamera");
        Camera playerCamera = cameraObj.AddComponent<Camera>();

        // 设置相机位置和参数
        cameraObj.transform.position = fixedCameraPosition;
        cameraObj.transform.rotation = Quaternion.Euler(fixedCameraRotation); // 俯视角

        // 设置为当前玩家的主相机
        cameraObj.tag = "MainCamera";

        // 销毁场景原有主相机（如果存在）
        if (Camera.main != null && Camera.main.gameObject != cameraObj)
        {
            Destroy(Camera.main.gameObject);
        }
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }
        m_moveAmt = m_moveAction.ReadValue<Vector2>();
        aim = m_lookAction.ReadValue<Vector2>();

        //if (m_jumpAction.WasPressedThisFrame())
        //{
        //    Jump();
        //}
    }

    private void Jump()
    {
        m_rigidbody.AddForceAtPosition(new Vector3(0, jumpForce, 0), Vector3.up, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        Walking();
        Rotating();
    }

    private void Walking()
    {
        Vector3 moveDirection = new Vector3(m_moveAmt.x, 0f, m_moveAmt.y);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            m_rigidbody.MovePosition(m_rigidbody.position + moveDirection.normalized * walkSpeed * Time.deltaTime);
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
        if (Mathf.Abs(aim.x) > GamepadDeadZone || Mathf.Abs(aim.y) > GamepadDeadZone)
        {
            Vector3 playerDirection = Vector3.right * aim.x + Vector3.forward * aim.y;

            if (playerDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }
    }

    // ﾐﾂﾔｽｷｨ
    public void OnCameraCreated(Camera cam)
    {
        localCamera = cam;
        Debug.Log("Player camera assigned");
    }

    private void RotateWithMouseFollow()
    {
        if (!IsOwner || localCamera == null)
        {
            Debug.LogWarning("No local camera available");
            return;
        }

        Ray ray = localCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            LookAt(ray.GetPoint(rayDistance));
        }
    }

    private void RotateWithMouse()
    {
        if (!IsOwner) return;

        // 使用固定视角的射线检测
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
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
        Debug.Log("Switched control scheme to: " + input.currentControlScheme);
        isGampadConnected = input.currentControlScheme.Equals("Gamepad") ? true : false;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnDeviceChange;
        }
    }
}
