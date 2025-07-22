using UnityEngine;
using UnityEngine.EventSystems;

public class ModelViewer : MonoBehaviour
{
    public float zoomSpeed = 0.5f;
    public float rotateSpeed = 100f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    private Camera cam;
    private Vector3 initialScale;
    private bool isHovering = false;

    void Start()
    {
        cam = Camera.main;
        initialScale = transform.localScale;
    }

    void Update()
    {
        // ·ÀÖ¹ UI µ²×¡
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        isHovering = false;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
                isHovering = true;
        }

        if (!isHovering) return;

        // Ëõ·Å
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 scale = transform.localScale;
            scale += Vector3.one * scroll * zoomSpeed;
            scale = Vector3.Max(initialScale * minScale, Vector3.Min(initialScale * maxScale, scale));
            transform.localScale = scale;
        }

        // Ðý×ª
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -rotX, Space.World);
        }
    }
}