using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float clickThreshold = 10f;

    private Camera cam;

    private Vector2 pressPos;
    private bool clickCandidate;

    private SelectableBuilding current;

    void Awake()
    {
        cam = Camera.main;

        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        if (buildingLayer.value == 0)
            buildingLayer = LayerMask.GetMask("Building");
    }

    void Update()
    {
        if (gridSystem != null && gridSystem.buildMode) return;
        if (Mouse.current == null || cam == null) return;

        HandleRightClick();
    }

    void HandleRightClick()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            pressPos = Mouse.current.position.ReadValue();
            clickCandidate = true;
        }

        if (Mouse.current.rightButton.isPressed && clickCandidate)
        {
            float dist = Vector2.Distance(pressPos, Mouse.current.position.ReadValue());
            if (dist > clickThreshold) clickCandidate = false;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame && clickCandidate)
        {
            TrySelectUnderMouse();
            clickCandidate = false;
        }
    }

    void TrySelectUnderMouse()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("HIT: " + hit.collider.name + " layer=" + LayerMask.LayerToName(hit.collider.gameObject.layer));

            var selectable = hit.collider.GetComponentInParent<SelectableBuilding>();
            SetSelected(selectable);
        }
        else
        {
            Debug.Log("NO HIT");
            SetSelected(null);
        }
    }

    void SetSelected(SelectableBuilding next)
    {
        if (current == next) return;

        if (current != null) current.SetSelected(false);
        current = next;
        if (current != null) current.SetSelected(true);
    }
}