using UnityEngine;

public class TreeSwaySmooth : MonoBehaviour
{
    public float maxSwayAngle = 5f;
    public float swaySpeed = 1f;
    public float damping = 0.5f;

    private float _currentZAngle;
    private float _targetZAngle;
    private float _velocity;
    private Vector3 _initialEuler;

    void Start()
    {
        _initialEuler = transform.eulerAngles;
        _currentZAngle = 0f;
        _targetZAngle = Random.Range(-maxSwayAngle, maxSwayAngle);
    }

    void Update()
    {
        if (Mathf.Abs(_currentZAngle - _targetZAngle) < 0.1f)
        {
            _targetZAngle = Random.Range(-maxSwayAngle, maxSwayAngle);
        }
        _currentZAngle = Mathf.SmoothDamp(
            _currentZAngle,
            _targetZAngle,
            ref _velocity,
            swaySpeed * (1 - damping)
        );
        transform.eulerAngles = new Vector3(
            _initialEuler.x,
            _initialEuler.y,
            _initialEuler.z + _currentZAngle
        );
    }
}