using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerInput))]
public class PlayerWalk : MonoBehaviour
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

    [Header("Walk Settings")]
    [SerializeField] private float rotateSpeed = 2f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float GamepadDeadZone = 0.1f;

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

    private void Update()
    {
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
