using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;
using System.Globalization;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementNetWork : NetworkBehaviour
{
    public InputActionAsset inputActions;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction throwAction;
    private InputAction switchTargetAction;
    private InputAction cancelThrowingAction;
    private InputAction chargingAction;
    private InputAction toggleAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Rigidbody rb;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chargeMoveSpeedRate = 0.8f;
    [SerializeField] private float speedDownDuration = 0.3f;
    private float oriMoveSpeed = 5f;

    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float GamepadDeadZone = 0.1f;
    private bool isGampadConnected = false;

    [SerializeField] private SpawnGrenadeNetWork grenadeThrower;
    [SerializeField] private GameObject grenadePrefab;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float autoLockRange = 5f;
    [SerializeField] private float lockBreakDeadzone = 0.15f;

    private List<Transform> lockTargets = new List<Transform>();
    private int lockTargetIndex = 0;
    private Transform currentLockTarget = null;

    [SerializeField] private PlayerTestSon playerParams;
    private Tween speedRecoverTween;
    private Vector3 cachedVelocity;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        lookAction = playerInput.actions.FindAction("Look");
        throwAction = playerInput.actions.FindAction("Throw");
        switchTargetAction = playerInput.actions.FindAction("SwitchTarget");
        cancelThrowingAction = playerInput.actions.FindAction("CancelThrowing");
        chargingAction = playerInput.actions.FindAction("Charging");
        toggleAction = playerInput.actions.FindAction("ToggleLight");

        throwAction.started += OnThrowStarted;
        throwAction.canceled += OnThrowCanceled;
        switchTargetAction.performed += OnSwitchTarget;
        cancelThrowingAction.performed += OnThrowCanceledManually;
        chargingAction.performed += OnCharging;
        toggleAction.performed += OnToggle;

        playerInput.onControlsChanged += OnDeviceChange;

        oriMoveSpeed = moveSpeed;
    }

    private void OnDisable()
    {
        throwAction.started -= OnThrowStarted;
        throwAction.canceled -= OnThrowCanceled;
        chargingAction.performed -= OnCharging;
        switchTargetAction.performed -= OnSwitchTarget;
        cancelThrowingAction.performed -= OnThrowCanceledManually;
        toggleAction.performed -= OnToggle;

        if (playerInput != null)
            playerInput.onControlsChanged -= OnDeviceChange;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;

        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        grenadeThrower.SetLookInput(lookInput);
        grenadeThrower.SetCanThrow(playerParams.CanThrowBulb());

        if (isGampadConnected)
        {
            if (lookInput.magnitude > lockBreakDeadzone)
                currentLockTarget = null;
            else if (currentLockTarget == null)
                UpdateLockTarget();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
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
            if (currentLockTarget != null)
            {
                Vector3 dir = currentLockTarget.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                }
            }
            else if (lookInput.magnitude > GamepadDeadZone)
            {
                Vector3 playerDirection = new Vector3(lookInput.x, 0, lookInput.y);
                if (playerDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 hitPoint = ray.GetPoint(rayDistance);
                Vector3 point = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
                transform.LookAt(point);
            }
        }
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        bool isGamepad = playerInput.currentControlScheme == "Gamepad";
        grenadeThrower.StartAiming(isGamepad);
    }

    private void OnThrowCanceled(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            if (playerParams.CanThrowBulb())
            {
                if (grenadeThrower.CancelAimingAndThrow())
                {
                    if (SpawnBulb()) {
                        playerParams.ThrowBulb();
                    }
                }
            }
            else grenadeThrower.CancelAimingOnly();
        }
    }

    private bool SpawnBulb() {
        cachedVelocity = grenadeThrower.GetCachedVelocity();
        Debug.Log("PlayerMovementNetWork cachedVelocity:" + cachedVelocity);
        if (!IsServer) SpawnGrenadeLocalServerRpc(cachedVelocity);
        else SpawnGrenadeLocal(cachedVelocity);
        return true;
    }

    [ServerRpc]
    private void SpawnGrenadeLocalServerRpc(Vector3 velocity)
    {
        GameObject grenade = Instantiate(grenadePrefab, grenadeThrower.GetThrowPosition(), Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = velocity;
        grenade.GetComponent<NetworkObject>()?.Spawn();
    }

    private void SpawnGrenadeLocal(Vector3 velocity)
    {
        GameObject grenade = Instantiate(grenadePrefab, grenadeThrower.GetThrowPosition(), Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = velocity;
        grenade.GetComponent<NetworkObject>()?.Spawn();
    }

    private void OnSwitchTarget(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        grenadeThrower?.LockToNextTarget();
        if (currentLockTarget != null && lockTargets.Count > 1)
        {
            lockTargetIndex = (lockTargetIndex + 1) % lockTargets.Count;
            currentLockTarget = lockTargets[lockTargetIndex];
        }
    }

    private void UpdateLockTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, autoLockRange, enemyLayer);
        lockTargets.Clear();

        foreach (Collider hit in hits)
        {
            var enemy = hit.GetComponentInParent<PlayerTestSon>();
            if (enemy != null && enemy.transform != this.transform)
            {
                lockTargets.Add(enemy.transform);
            }
        }

        if (lockTargets.Count > 0)
        {
            lockTargets.Sort((a, b) =>
                Vector3.Distance(transform.position, a.position).CompareTo(
                Vector3.Distance(transform.position, b.position)));

            lockTargetIndex = 0;
            currentLockTarget = lockTargets[0];
        }
        else
        {
            currentLockTarget = null;
        }
    }

    private void OnCharging(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (playerParams == null || !playerParams.BatteryCharge())
            return;

        moveSpeed = oriMoveSpeed * chargeMoveSpeedRate;

        if (speedRecoverTween != null && speedRecoverTween.IsActive())
        {
            speedRecoverTween.Kill();
        }

        speedRecoverTween = DOVirtual.DelayedCall(speedDownDuration, () =>
        {
            moveSpeed = oriMoveSpeed;
        });
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        playerParams?.ToggleFlashlight();
    }

    private void OnThrowCanceledManually(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        grenadeThrower.CancelAimingOnly();
    }

    public void OnDeviceChange(PlayerInput input)
    {
        if (input != null && !string.IsNullOrEmpty(input.currentControlScheme))
        {
            isGampadConnected = input.currentControlScheme.Equals("Gamepad");
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.onControlsChanged -= OnDeviceChange;
        if (speedRecoverTween != null && speedRecoverTween.IsActive())
            speedRecoverTween.Kill();
    }
}
