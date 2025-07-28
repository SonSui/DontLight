using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class SpawnGrenade : MonoBehaviour
{
    private Camera mainCamera;
    private LineRenderer lineRenderer;
    private bool canThrow = true;
    public Color[] lineColor; 

    // グレネードのプレハブ
    // Grenade prefab
    // 手榴弹的预制体
    [SerializeField] private GameObject grenadePrefab;

    // 弾道のポイント数
    // Number of trajectory points
    // 弹道的轨迹点数量
    [SerializeField] private int trajectoryPoints = 30;

    // 弾道描画の時間ステップ
    // Time step for trajectory calculation
    // 弹道计算的时间步长
    [SerializeField] private float timeStep = 0.1f;

    // 投擲の飛行時間
    // Flight time of the grenade
    // 手榴弹的飞行时间
    [SerializeField] private float flightTime = 1.5f;

    [Header("Range Settings")]
    // 最大投擲距離
    // Maximum throwing range
    // 最大投掷范围
    [SerializeField] private float maxThrowRange = 15f;

    // 射程インジケーター
    // Range indicator canvas
    // 范围指示器
    [SerializeField] private GameObject rangeIndicatorCanvas;
    private Image rangeIndicatorImage;
    [SerializeField] private GameObject damageRangeIndicatorCanvas;
    private Image damageRangeIndicatorImage;

    // ターゲット層
    // Target layer
    // 目标检测层
    [SerializeField] private LayerMask targetLayer;

    // 照準中かどうか
    // Whether the player is aiming
    // 是否处于瞄准状态
    public bool isAiming = false;

    private Vector3 cachedVelocity;

    // ターゲットリスト
    // List of targets
    // 目标列表
    private List<Transform> targetList = new List<Transform>();

    private int currentTargetIndex = 0;
    private Transform currentTarget;

    // ロックオンモード使用中か
    // Whether lock-on mode is active
    // 是否使用锁定模式
    private bool useLockOn = false;

    private Vector2 lookInput = Vector2.zero;
    private float lockBreakDeadzone = 0.15f;

    private void Awake()
    {
        if(lineColor.Length < 2)
        {
            lineColor = new Color[2] {
                Color.white, // 投擲可能時の色
                Color.red    // 投擲不可時の色
            };
        }
    }
    private void Start()
    {
        // カメラとLineRendererの初期化
        // Initialize camera and LineRenderer
        // 初始化主摄像机和LineRenderer
        mainCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryPoints;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);
        damageRangeIndicatorCanvas.SetActive(false);
        rangeIndicatorImage = rangeIndicatorCanvas.transform.GetChild(0)?.GetComponent<Image>();
        damageRangeIndicatorImage = damageRangeIndicatorCanvas.transform.GetChild(0)?.GetComponent<Image>();
        if (rangeIndicatorImage != null)
        {
            rangeIndicatorImage.color = lineColor[0]; // 投擲可能時の色
        }
        if (damageRangeIndicatorImage != null)
        {
            damageRangeIndicatorImage.color = lineColor[0]; // 投擲可能時の色
        }
    }

    private void Update()
    {
        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3 targetPoint;

        bool joystickOverride = lookInput.magnitude > lockBreakDeadzone;

        if (!useLockOn)
        {
            // マウス入力で照準
            if (!GetMouseTarget(out Vector3 mouseHit))
            {
                lineRenderer.enabled = false;
                damageRangeIndicatorCanvas.SetActive(false);
                return;
            }

            targetPoint = mouseHit;
            damageRangeIndicatorCanvas.SetActive(true);
            damageRangeIndicatorCanvas.transform.position = targetPoint;
        }
        else if (useLockOn && lookInput.magnitude <= lockBreakDeadzone && targetList.Count > 0)
        {
            // ロックオンモード：ターゲットを選択
            currentTarget = targetList[currentTargetIndex];
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance > maxThrowRange)
            {
                currentTarget = null;
                lineRenderer.enabled = false;
                return;
            }
            targetPoint = currentTarget.position;
            damageRangeIndicatorCanvas.SetActive(true);
            damageRangeIndicatorCanvas.transform.position = targetPoint;
        }
        else
        {
            // ゲームパッドの右スティック入力で照準
            Vector3 dir = new Vector3(lookInput.x, 0, lookInput.y);
            if (dir.sqrMagnitude < 0.01f)
            {
                lineRenderer.enabled = false;
                return;
            }

            dir = dir.normalized;
            float throwPower = Mathf.Clamp01(lookInput.magnitude);

            Vector3 rawPoint = transform.position + dir * maxThrowRange * throwPower;
            targetPoint = new Vector3(rawPoint.x, rangeIndicatorCanvas.transform.position.y, rawPoint.z);

            damageRangeIndicatorCanvas.SetActive(true);
            damageRangeIndicatorCanvas.transform.position = targetPoint;
        }

        cachedVelocity = CalculateLaunchVelocity(transform.position, targetPoint, flightTime);
        ShowTrajectory(transform.position, cachedVelocity);
        lineRenderer.enabled = true;

        damageRangeIndicatorCanvas.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }
    public void StartAiming(bool lockOnMode)
    {
        // 照準開始
        // Start aiming
        // 开始瞄准
        isAiming = true;
        useLockOn = lockOnMode;

        if (useLockOn)
        {
            FindTargetsInRange();
            currentTargetIndex = 0;
            currentTarget = targetList.Count > 0 ? targetList[currentTargetIndex] : null;
        }
        else
        {
            currentTarget = null;
        }

        rangeIndicatorCanvas.SetActive(true);

        if (useLockOn && currentTarget != null)
            lineRenderer.enabled = true;
        else if (!useLockOn && GetMouseTarget(out _))
            lineRenderer.enabled = true;
    }

    public bool CancelAimingAndThrow()
    {
        // 照準解除と投擲
        // Cancel aiming and throw
        // 取消瞄准并投掷
        if (!isAiming) return false;

        isAiming = false;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);
        damageRangeIndicatorCanvas.SetActive(false);

        if (useLockOn && lookInput.magnitude <= lockBreakDeadzone)
        {
            if (currentTarget == null ||
                Vector3.Distance(transform.position, currentTarget.position) > maxThrowRange)
            {
                Debug.Log("No target, cancel throwing");
                return false;
            }
        }

        ThrowGrenade(cachedVelocity);
        return true;
    }

    public void LockToNextTarget()
    {
        // 次のターゲットにロックオン
        // Lock on to the next target
        // 切换到下一个目标
        if (targetList.Count == 0) return;
        currentTargetIndex = (currentTargetIndex + 1) % targetList.Count;
        currentTarget = targetList[currentTargetIndex];
    }

    private void ThrowGrenade(Vector3 velocity)
    {
        // グレネードを生成し投擲
        // Instantiate and throw grenade
        // 生成并投掷手榴弹
        GameObject grenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;  // 修正：使用rb.velocity
        }
    }

    private bool GetMouseTarget(out Vector3 hitPoint)
    {
        // マウス位置を地面上の点に変換
        // Get mouse target point on ground
        // 获取鼠标指向的地面位置
        if (mainCamera == null)
        {
            hitPoint = Vector3.zero;
            return false;
        }

        Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, rangeIndicatorCanvas.transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 rawPoint = ray.GetPoint(enter);
            Vector3 dir = rawPoint - transform.position;

            if (dir.magnitude > maxThrowRange)
            {
                dir = dir.normalized * maxThrowRange;
            }

            hitPoint = transform.position + dir;
            return true;
        }

        hitPoint = Vector3.zero;
        return false;
    }

    private void FindTargetsInRange()
    {
        // 射程内のターゲットを探す
        // Find targets within throw range
        // 搜索投掷范围内的目标
        Collider[] hits = Physics.OverlapSphere(transform.position, maxThrowRange, targetLayer);
        targetList.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.transform.root != transform.root) // 自分を除外
            {
                targetList.Add(hit.transform);
            }
        }

        targetList.Sort((a, b) =>
            Vector3.Distance(transform.position, a.position).CompareTo(
            Vector3.Distance(transform.position, b.position)));
    }

    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float time)
    {
        // 投擲速度を計算
        // Calculate launch velocity
        // 计算投掷速度
        Vector3 displacement = targetPoint - startPoint;
        Vector3 horizontal = new Vector3(displacement.x, 0, displacement.z);
        float y = displacement.y;
        float g = Mathf.Abs(Physics.gravity.y);

        Vector3 velocityXZ = horizontal / time;
        float velocityY = y / time + 0.5f * g * time;

        return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }

    private void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        // 弾道をLineRendererで描画
        // Draw trajectory with LineRenderer
        // 使用LineRenderer绘制弹道
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }
    }

    public void CancelAimingOnly()
    {
        // 照準のみキャンセル
        // Cancel aiming only
        // 仅取消瞄准
        isAiming = false;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);
        damageRangeIndicatorCanvas.SetActive(false);
        currentTarget = null;
    }

    public void SetCanThrow(bool canThrow)
    {
        // 投擲可能か設定
        // Set whether throwing is allowed
        // 设置是否可以投掷
        this.canThrow = canThrow;
        if (lineRenderer != null)
        lineRenderer.colorGradient = canThrow ? new Gradient
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(lineColor[0], 0f),
                new GradientColorKey(lineColor[0], 1f)
            }
        } : new Gradient
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(lineColor[1], 0f),
                new GradientColorKey(lineColor[1], 1f)
            }
        };

        if (rangeIndicatorImage != null)
        {
            rangeIndicatorImage.color = canThrow ? lineColor[0] : lineColor[1];
        }
        if (damageRangeIndicatorImage != null)
        {
            damageRangeIndicatorImage.color = canThrow ? lineColor[0] : lineColor[1];
        }
    }
}
