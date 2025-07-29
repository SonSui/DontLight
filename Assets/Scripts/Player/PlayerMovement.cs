using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    // 入力アセット (Input Systemのアクション定義)
    // Input action asset (Input System config)
    // 输入动作资源（用于Input System配置）
    public InputActionAsset inputActions;

    private PlayerInput playerInput;

    // 各種アクション定義
    // Input action references
    // 输入动作引用
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction throwAction;
    private InputAction switchTargetAction;
    private InputAction cancelThrowingAction;
    private InputAction chargingAction;
    private InputAction toggleAction;

    // 入力値キャッシュ
    // Cached input values
    // 缓存的输入数值
    private Vector2 moveInput;
    private Vector2 lookInput;

    // プレイヤーのRigidbody（物理移動制御）
    // Rigidbody for player movement
    // 控制玩家物理移动的刚体
    private Rigidbody rb;

    // 移動速度
    // Movement speed
    // 移动速度
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chargeMoveSpeedRate = 0.8f;
    [SerializeField] private float speedDownDuration = 0.3f; // 移動速度ダウンの時間
    private float oriMoveSpeed = 5f;

    // 回転速度
    // Rotation speed
    // 旋转速度
    [SerializeField] private float rotateSpeed = 5f;

    // ゲームパッドのスティックのデッドゾーン
    // Deadzone for gamepad sticks
    // 手柄摇杆的死区
    [SerializeField] private float GamepadDeadZone = 0.1f;

    // 現在ゲームパッドが接続されているか
    // Whether a gamepad is currently active
    // 当前是否连接了手柄
    private bool isGampadConnected = false;

    // グレネード制御クラス参照
    // Reference to grenade-throwing component
    // 手榴弹投掷逻辑的引用
    [SerializeField] private SpawnGrenade grenadeThrower;


    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float autoLockRange = 5f;
    [SerializeField] private float lockBreakDeadzone = 0.15f;

    private List<Transform> lockTargets = new List<Transform>();
    private int lockTargetIndex = 0;
    private Transform currentLockTarget = null;


    // プレイヤーのパラメータ
    // Player parameters
    // 玩家参数
    [SerializeField] private PlayerTestSon playerParams;

    private Tween speedRecoverTween;



    private void Awake()
    {
        // PlayerInput取得と各アクション初期化
        // Get PlayerInput and bind input actions
        // 获取PlayerInput组件并绑定各个输入动作
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        lookAction = playerInput.actions.FindAction("Look");
        throwAction = playerInput.actions.FindAction("Throw");
        switchTargetAction = playerInput.actions.FindAction("SwitchTarget");
        cancelThrowingAction = playerInput.actions.FindAction("CancelThrowing");
        chargingAction = playerInput.actions.FindAction("Charging");
        toggleAction = playerInput.actions.FindAction("ToggleLight");

        // イベント登録（投擲、照準、充電等）
        // Register input events
        // 注册各类输入事件（投掷、取消、充电等）
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
        // イベント解除
        // Unregister input events
        // 注销所有输入事件，避免内存泄漏
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
        // Rigidbodyを取得
        // Get Rigidbody
        // 获取Rigidbody组件
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 入力値の更新
        // Read input values every frame
        // 每帧读取输入值
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();


        grenadeThrower.SetLookInput(lookInput);
        grenadeThrower.SetCanThrow(playerParams.CanThrowBulb());

        if (isGampadConnected)
        {
            if (lookInput.magnitude > lockBreakDeadzone)
            {
                currentLockTarget = null; // プレイヤーが操作したら解除
            }
            else
            {
                if (currentLockTarget == null)
                {
                    UpdateLockTarget();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // 移動・回転の実行（物理更新）
        // Execute movement and rotation in physics update
        // 在物理更新中处理移动和旋转
        Moveing();
        Rotating();
    }

    private void Moveing()
    {
        // 入力から移動方向を計算
        // Convert input into movement direction
        // 将输入转为三维移动方向
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // プレイヤーを指定方向へ移動
            // Move the player using Rigidbody
            // 使用Rigidbody移动玩家
            rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void Rotating()
    {
        // 入力デバイスに応じた回転処理
        // Rotate based on input device
        // 根据输入设备决定旋转方式
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
        // スティック入力があるかチェック
        // Check if stick input is beyond deadzone
        // 判断摇杆输入是否超过死区
        /*if (Mathf.Abs(lookInput.x) > GamepadDeadZone || Mathf.Abs(lookInput.y) > GamepadDeadZone)
        {
            Vector3 playerDirection = Vector3.right * lookInput.x + Vector3.forward * lookInput.y;

            if (playerDirection.sqrMagnitude > 0.01f)
            {
                // プレイヤーの向きを回転させる
                // Rotate player toward stick direction
                // 向手柄方向平滑旋转
                Quaternion targetRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }*/
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

    private void RotateWithMouse()
    {
        // マウス位置を取得し、視線を計算
        // Get mouse position and compute look direction
        // 获取鼠标位置并计算朝向
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
        // Y軸を固定してプレイヤーの向きを変更
        // Rotate the player to face the point (Y-axis fixed)
        // 固定Y轴朝向鼠标命中的点
        Vector3 Point = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
        transform.LookAt(Point);
    }

    public void OnDeviceChange(PlayerInput input)
    {
        // 入力デバイス変更時の処理
        // Detect and handle control scheme change
        // 检测输入设备变化（如切换到手柄）
        if (input != null && !string.IsNullOrEmpty(input.currentControlScheme))
        {
            isGampadConnected = input.currentControlScheme.Equals("Gamepad");
        }
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        // 投擲ボタンを押した時の処理（照準開始）
        // When throw button is pressed: start aiming
        // 开始投掷时（按下键）：开始瞄准

        // 手柄の場合、自動でロックオン
        // If using gamepad, use lock-on mode
        // 如果是手柄，启用自动锁定目标
        bool isGamepad = playerInput.currentControlScheme == "Gamepad";
        grenadeThrower.StartAiming(isGamepad);
    }

    private void OnSwitchTarget(InputAction.CallbackContext ctx)
    {
        // 複数の敵がいる場合、ターゲットを切り替え
        // Switch between lock-on targets
        // 切换锁定目标（如果有多个）
        if (grenadeThrower != null)
        {
            grenadeThrower.LockToNextTarget();
        }
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

    private void OnThrowCanceled(InputAction.CallbackContext ctx)
    {
        // ボタンを離した時に投擲
        // Throw when button is released
        // 松开按键时执行投掷
        if (!grenadeThrower.isAiming)
            return;

        if (playerParams.CanThrowBulb())
        {
            if(grenadeThrower.CancelAimingAndThrow())
            {
                playerParams?.ThrowBulb();
            }
        }
        else
        {
            // 懐中電灯の充電が必要な場合はキャンセルのみ
            // If flashlight needs charging, just cancel aiming
            // 如果需要充电，则仅取消瞄准
            grenadeThrower.CancelAimingOnly();
        }
    }

    private void OnDestroy()
    {
        // オブジェクト破棄時のイベント解除
        // Clean up input listener on destroy
        // 在销毁对象时注销事件
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnDeviceChange;
        }
        if (speedRecoverTween != null && speedRecoverTween.IsActive())
        {
            speedRecoverTween.Kill();
        }
    }

    private void OnThrowCanceledManually(InputAction.CallbackContext ctx)
    {
        // 照準を中断（投擲はしない）
        // Cancel aiming without throwing
        // 取消瞄准（不投掷）
        grenadeThrower.CancelAimingOnly();
    }

    private void OnCharging(InputAction.CallbackContext ctx)
    {
        // 懐中電灯の充電処理
        // Recharge flashlight when button pressed
        // 按下充电键时给手电筒充电
        if (playerParams == null || !playerParams.BatteryCharge())
            return;
        moveSpeed = oriMoveSpeed * chargeMoveSpeedRate;

        // 懐中電灯の充電処理
        if (playerParams == null || !playerParams.BatteryCharge())
            return;

        // 成功したら移動速度を80%にし、既存の回復処理をキャンセルしてリセット
        moveSpeed = oriMoveSpeed * chargeMoveSpeedRate;

        // 既存のTweenがあればKillしてリセット（充電連打に対応）
        if (speedRecoverTween != null && speedRecoverTween.IsActive())
        {
            speedRecoverTween.Kill();
        }

        // 指定時間後に速度を元に戻すTween
        speedRecoverTween = DOVirtual.DelayedCall(speedDownDuration, () =>
        {
            moveSpeed = oriMoveSpeed;
        });

    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        // 懐中電灯のオンオフ切り替え
        // Toggle flashlight on/off
        // 切换手电筒开关状态
        playerParams?.ToggleFlashlight();
    }
    
}
