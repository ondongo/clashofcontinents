using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    /* Références vers les composants du même GameObject */
    Camera cam;
    Transform cachedTransform;

    /* Valeurs de zoom */
    public float zoomMin = 5f;
    public float zoomMax = 20f;
    public float zoomSpeed = 10f;

    /* Vitesse du glissement (Pour le drag) */
    public float panSensitivity = 1f;

    /* Limites de la carte : la caméra ne sort pas de cette zone (X et Z) que l'ont défini. */
    public bool useLimits;
    public Vector2 limitMin = Vector2.zero;
    public Vector2 limitMax = Vector2.zero;

    float targetOrthoSize;
    Vector3 dragWorldStart;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cachedTransform = transform;

        /* Si la caméra est en mode Orthographic, on garde sa taille actuelle pour le zoom. */
        if (cam.orthographic)
            targetOrthoSize = cam.orthographicSize;
    }

    void Update()
    {
        UpdateZoom();
        UpdatePan();
    }

    /*
     * Zoom avec la molette.
     * En mode orthographique : modifie orthographicSize.
     * En mode perspective : modifie la position Y de la caméra.
     */
    void UpdateZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y / 1200f;
        if (Mathf.Approximately(scroll, 0f)) return;

        if (cam.orthographic)
        {
            targetOrthoSize -= scroll * zoomSpeed;
            targetOrthoSize = Mathf.Clamp(targetOrthoSize, zoomMin, zoomMax);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, 15f * Time.deltaTime);
        }
        else
        {
            Vector3 pos = cachedTransform.position;
            pos.y = Mathf.Clamp(pos.y - scroll * zoomSpeed, zoomMin, zoomMax);
            cachedTransform.position = pos;
        }
    }

    /* Clic + glisser = déplacer la caméra. */
    void UpdatePan()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.isPressed) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = ScreenToWorldXZ(new Vector3(mouseScreen.x, mouseScreen.y, 0f));

        if (Mouse.current.leftButton.wasPressedThisFrame)
            dragWorldStart = mouseWorld;

        Vector3 delta = dragWorldStart - mouseWorld;
        delta.y = 0f;

        Vector3 pos = cachedTransform.position + delta * panSensitivity;

        if (useLimits)
        {
            pos.x = Mathf.Clamp(pos.x, limitMin.x, limitMax.x);
            pos.z = Mathf.Clamp(pos.z, limitMin.y, limitMax.y);
        }

        cachedTransform.position = pos;
        dragWorldStart = ScreenToWorldXZ(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
    }

    /*
     * Convertit une position à l'écran en position dans le monde, dans le plan horizontal (Y = 0).
     * Utilise un rayon (Ray) depuis la caméra et un plan (Plane) pour trouver l'intersection.
     */
    Vector3 ScreenToWorldXZ(Vector3 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return cachedTransform.position;
    }
}
