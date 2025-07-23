using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpawnGrenade : MonoBehaviour
{
    private Camera mainCamera;
    private LineRenderer lineRenderer;

    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float timeStep = 0.1f;
    [SerializeField] private float flightTime = 1.5f;

    [Header("Range Settings")]
    [SerializeField] private float maxThrowRange = 15f;
    [SerializeField] private GameObject rangeIndicatorCanvas;
    [SerializeField] private LayerMask targetLayer;

    private bool isAiming = false;
    private Vector3 cachedVelocity;

    private List<Transform> targetList = new List<Transform>();
    private int currentTargetIndex = 0;
    private Transform currentTarget;

    private bool useLockOn = false;

    private void Start()
    {
        mainCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryPoints;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3 targetPoint;

        if (useLockOn)
        {
            // 手柄模式：只在有敌人时显示
            if (targetList.Count == 0)
            {
                // 没敌人，关闭线条，什么也不显示
                currentTarget = null;
                lineRenderer.enabled = false;
                return;
            }
            else
            {
                currentTarget = targetList[currentTargetIndex];
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                if (distance > maxThrowRange)
                {
                    // 目标超出范围，不显示线条
                    currentTarget = null;
                    lineRenderer.enabled = false;
                    return;
                }
                targetPoint = currentTarget.position;
            }
        }
        else
        {
            // 鼠标模式，显示鼠标位置轨迹
            if (!GetMouseTarget(out Vector3 mouseHit))
            {
                lineRenderer.enabled = false;
                return;
            }
            targetPoint = mouseHit;
        }

        cachedVelocity = CalculateLaunchVelocity(transform.position, targetPoint, flightTime);
        ShowTrajectory(transform.position, cachedVelocity);
        lineRenderer.enabled = true;
    }


    public void StartAiming(bool lockOnMode)
    {
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

    public void CancelAimingAndThrow()
    {
        isAiming = false;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);

        // 投掷条件判断
        if (useLockOn)
        {
            if (currentTarget == null ||
                Vector3.Distance(transform.position, currentTarget.position) > maxThrowRange)
            {
                Debug.Log("No target, cancel throwing");
                return;
            }
        }
        else
        {
            // 如果非锁定，cachedVelocity是根据鼠标点算的，理论上没问题
        }

        ThrowGrenade(cachedVelocity);
    }

    public void LockToNextTarget()
    {
        if (targetList.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % targetList.Count;
        currentTarget = targetList[currentTargetIndex];
    }

    private void ThrowGrenade(Vector3 velocity)
    {
        GameObject grenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;  // 这里改成rb.velocity，不是linearVelocity
        }
    }

    private bool GetMouseTarget(out Vector3 hitPoint)
    {
        if (mainCamera == null)
        {
            hitPoint = Vector3.zero;
            return false;
        }

        Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

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
        Collider[] hits = Physics.OverlapSphere(transform.position, maxThrowRange, targetLayer);
        targetList.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.transform.root != transform.root) // 避免锁定自己
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
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }
    }
}
