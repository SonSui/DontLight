using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        lineRenderer.enabled = true;
        rangeIndicatorCanvas.SetActive(true);
    }

    public void CancelAimingAndThrow()
    {
        isAiming = false;
        lineRenderer.enabled = false;
        rangeIndicatorCanvas.SetActive(false);
        ThrowGrenade(cachedVelocity);
    }

    public void LockToNextTarget()
    {
        if (targetList.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % targetList.Count;
        currentTarget = targetList[currentTargetIndex];
    }

    private void Update()
    {
        if (!isAiming) return;

        Vector3 targetPoint;

        if (useLockOn && currentTarget != null)
        {
            targetPoint = currentTarget.position;
        }
        else if (GetMouseTarget(out Vector3 mouseHit))
        {
            targetPoint = mouseHit;
        }
        else
        {
            return;
        }

        cachedVelocity = CalculateLaunchVelocity(transform.position, targetPoint, flightTime);
        ShowTrajectory(transform.position, cachedVelocity);
    }

    private void ThrowGrenade(Vector3 velocity)
    {
        GameObject grenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }

    private bool GetMouseTarget(out Vector3 hitPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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
            if (hit.transform != transform)
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
