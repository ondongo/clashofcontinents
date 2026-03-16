using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    Camera cam;
    Transform cachedTransform;

    [Header("Zoom")]
    public float zoomMinDistance = 6f;
    public float zoomMaxDistance = 60f;
    public float zoomStep = 3f;
    public float zoomLerpSpeed = 12f;

    [Header("Pan")]
    public float panSensitivity = 1f;

    [Header("Limits")]
    public bool useLimits;
    public Vector2 limitMin = new Vector2(-50f, -50f);
    public Vector2 limitMax = new Vector2(50f, 50f);

    Vector3 dragWorldStart;

    float targetDistance;
    Vector3 groundFocusPoint;
    float initialDistance;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cachedTransform = transform;

        groundFocusPoint = GetGroundPointAtScreenCenter();
        initialDistance = Vector3.Distance(cachedTransform.position, groundFocusPoint);
        targetDistance = initialDistance;

        zoomMaxDistance = initialDistance;
    }
 
    void Update()
    {
        UpdateZoom();
        UpdatePan();
        ApplyZoom();
    }

    void UpdateZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (scroll > 0f)
            targetDistance -= zoomStep;
        else if (scroll < 0f)
            targetDistance += zoomStep;

        targetDistance = Mathf.Clamp(targetDistance, zoomMinDistance, zoomMaxDistance);
    }

    void ApplyZoom()
    {
        groundFocusPoint = GetGroundPointAtScreenCenter();

        Vector3 dir = (cachedTransform.position - groundFocusPoint).normalized;
        Vector3 desiredPosition = groundFocusPoint + dir * targetDistance;

        cachedTransform.position = Vector3.Lerp(
            cachedTransform.position,
            desiredPosition,
            zoomLerpSpeed * Time.deltaTime
        );
    }

    void UpdatePan()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.isPressed) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = ScreenToWorldXZ(mouseScreen);

        if (Mouse.current.leftButton.wasPressedThisFrame)
            dragWorldStart = mouseWorld;
 
        Vector3 delta = dragWorldStart - mouseWorld;
        delta.y = 0f;

        Vector3 newPos = cachedTransform.position + delta * panSensitivity;
        groundFocusPoint += delta * panSensitivity;

        if (useLimits)
        {
            newPos.x = Mathf.Clamp(newPos.x, limitMin.x, limitMax.x);
            newPos.z = Mathf.Clamp(newPos.z, limitMin.y, limitMax.y);

            groundFocusPoint.x = Mathf.Clamp(groundFocusPoint.x, limitMin.x, limitMax.x);
            groundFocusPoint.z = Mathf.Clamp(groundFocusPoint.z, limitMin.y, limitMax.y);
        }

        cachedTransform.position = newPos;
        dragWorldStart = ScreenToWorldXZ(mouseScreen);
    }

    Vector3 GetGroundPointAtScreenCenter()
    {
        return ScreenToWorldXZ(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
    }

    Vector3 ScreenToWorldXZ(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
 
        if (ground.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        // fallback : point devant la caméra si jamais elle regarde mal
        return cachedTransform.position + cachedTransform.forward * 10f;
    }
}