using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MagicHatAdvanced : MonoBehaviour
{
    [Header("Spatial Rrange")]
    public Transform[] volumeCorners = new Transform[8];

    [Header("Flight Parameters")]
    public float maxSpeed = 8f;
    public float accelerationCurve = 2f;
    public float burstForce = 15f;
    public float rotationTorque = 0.1f;

    [Header("Behavioral Parameters")]
    public float idleProbability = 0.3f;
    public float burstInterval = 1.5f;
    public float directionChangeRate = 0.2f;

    private Rigidbody _rb;
    private Vector3 _targetDirection;
    private float _burstTimer;
    private bool _isBursting;
    private Vector3 _lastPosition;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearDamping = 0.5f;
        _rb.angularDamping = 0.8f;
        _lastPosition = transform.position;

        StartCoroutine(BehaviorLoop());
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            if (Random.value < idleProbability)
            {
                yield return StartCoroutine(IdleMode(Random.Range(0.5f, 2f)));
            }
            else
            {
                yield return StartCoroutine(BurstMode(Random.Range(1f, 3f)));
            }
        }
    }

    IEnumerator IdleMode(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            _rb.AddTorque(new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)) * rotationTorque * 0.3f,
                ForceMode.Acceleration
            );
            Vector3 holdPosition = _lastPosition +
                new Vector3(
                    Mathf.PerlinNoise(Time.time, 0) - 0.5f,
                    Mathf.PerlinNoise(0, Time.time) - 0.5f,
                    Mathf.PerlinNoise(Time.time, Time.time) - 0.5f
                ) * 0.3f;

            Vector3 force = (holdPosition - transform.position) * 2f;
            _rb.AddForce(force, ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator BurstMode(float duration)
    {
        float timer = 0;
        _targetDirection = GetRandomDirectionInsideVolume();

        while (timer < duration)
        {
            if (Random.value < directionChangeRate)
            {
                _targetDirection = Vector3.Slerp(
                    _targetDirection,
                    GetRandomDirectionInsideVolume(),
                    0.3f
                );
            }

            float speedFactor = Mathf.PingPong(timer * 2f, 1f);
            speedFactor = Mathf.Pow(speedFactor, accelerationCurve);

            Vector3 targetVelocity = _targetDirection * maxSpeed * speedFactor;
            Vector3 force = (targetVelocity - _rb.linearVelocity) * 2f;

            if (Random.value < 0.1f)
            {
                force += Random.onUnitSphere * burstForce;
                _rb.AddTorque(Random.onUnitSphere * rotationTorque * 3f,
                    ForceMode.Impulse);
            }

            _rb.AddForce(force, ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    Vector3 GetRandomDirectionInsideVolume()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
        Vector3 p = Vector3.Lerp(
            Vector3.Lerp(
                Vector3.Lerp(volumeCorners[0].position, volumeCorners[1].position, randomPoint.x),
                Vector3.Lerp(volumeCorners[2].position, volumeCorners[3].position, randomPoint.x),
                randomPoint.y
            ),
            Vector3.Lerp(
                Vector3.Lerp(volumeCorners[4].position, volumeCorners[5].position, randomPoint.x),
                Vector3.Lerp(volumeCorners[6].position, volumeCorners[7].position, randomPoint.x),
                randomPoint.y
            ),
            randomPoint.z
        );
        return (p - transform.position).normalized;
    }

    void OnDrawGizmos()
    {
        if (volumeCorners.Length != 8) return;

        Gizmos.color = Color.magenta;
        int[] edges = { 0, 1, 1, 3, 3, 2, 2, 0, 4, 5, 5, 7, 7, 6, 6, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
        for (int i = 0; i < edges.Length; i += 2)
        {
            if (volumeCorners[edges[i]] && volumeCorners[edges[i + 1]])
                Gizmos.DrawLine(
                    volumeCorners[edges[i]].position,
                    volumeCorners[edges[i + 1]].position
                );
        }
    }
}