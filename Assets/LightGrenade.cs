using UnityEngine;
using UnityEngine.InputSystem;

public class LightGrenade : MonoBehaviour
{
    [SerializeField] private float parabolaHeight = 5f;
    [SerializeField] private float moveDuration = 1.0f; // 抛物线飞行总时长
    [SerializeField] private float durationTime = 10f; 

    private Vector3 startPosition;
    private Vector3 savedTarget; // 保存G时的目标点
    private Vector3 flyStart;

    private bool isFlying = false;
    private float flyTime = 0f;

    private void Start()
    {
        Destroy(gameObject, durationTime); // 确保在飞行结束后销毁
    }
    private void Update()
    {
        // 1. G被按下时，记录鼠标位置
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Vector3 target;
            if (TryGetMouseGroundPoint(out target))
            {
                savedTarget = target;
                transform.position = startPosition;
                flyTime = 0f;
                isFlying = true;

            }
        }

        // 2. 抛物线移动
        if (isFlying)
        {
            flyTime += Time.deltaTime;
            float t = Mathf.Clamp01(flyTime / moveDuration);

            // 抛物线插值：基础线性插值 + 高度
            Vector3 current = Vector3.Lerp(flyStart, savedTarget, t);
            float parabola = 4 * parabolaHeight * t * (1 - t); // 抛物线公式（顶点在t=0.5）

            current.y += parabola;

            transform.position = current;

            if (t >= 1f)
                isFlying = false;
        }
    }

    // 获取鼠标在y=0地面上的点
    bool TryGetMouseGroundPoint(out Vector3 point)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            point = ray.GetPoint(rayDistance);
            return true;
        }
        point = Vector3.zero;
        return false;
    }
}
