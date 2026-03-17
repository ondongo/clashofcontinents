using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using ClashOfContinents;

public class GridSystem : MonoBehaviour
{
    public static GridSystem instance { get; private set; }

    [System.Serializable]
    public class ShopBuildingEntry
    {
        public Data.BuildingID  id;
        public GameObject       prefab;

        [Header("Cout d achat")]
        public int costGold;
        public int costElixir;
        public int costDarkElixir;
        public int costGems;

        [Header("Affichage shop")]
        public Sprite icon;
        public string buildingName;
    }

    [Header("Catalogue des batiments (remplis dans l Inspector)")]
    public List<ShopBuildingEntry> shopBuildings = new List<ShopBuildingEntry>();

    [Header("Parametres de la grille")]
    public float    gridSize            = 1f;
    public float    clickThreshold      = 10f;
    public Key      toggleBuildKey      = Key.B;
    public LayerMask buildPlaneMask;
    public float    raycastMaxDist      = 1000f;

    public bool buildMode    = false;
    public int  selectedIndex = 0;
    private GameObject ghost;
    private readonly HashSet<Vector3> takenPositions = new HashSet<Vector3>();
    private Camera  cam;
    private Vector2 mouseStartPos;
    private bool    clickPending;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        CreateGhost();
        RefreshBuildMode();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleBuildKey].wasPressedThisFrame)
        {
            buildMode = !buildMode;
            RefreshBuildMode();
        }

        if (!buildMode) return;

        HandleHotkeys();

        if (Mouse.current == null || cam == null) return;

        MoveGhostToMouse();
        DetectPlacementClick();
    }

    void RefreshBuildMode()
    {
        if (ghost != null)
            ghost.SetActive(buildMode);
        clickPending = false;
    }

    void CreateGhost()
    {
        if (shopBuildings == null || shopBuildings.Count == 0)
        {
            Debug.LogWarning("GridSystem: shopBuildings est vide. Ajoute tes batiments dans l Inspector.");
            return;
        }

        int idx    = Mathf.Clamp(selectedIndex, 0, shopBuildings.Count - 1);
        GameObject prefab = shopBuildings[idx].prefab;

        if (prefab == null)
        {
            Debug.LogWarning($"GridSystem: prefab manquant pour l entree [{idx}].");
            return;
        }

        ghost = Instantiate(prefab);

        foreach (var col in ghost.GetComponentsInChildren<Collider>())
            col.enabled = false;

        PaintGhost(new Color(1f, 1f, 1f, 0.5f));
    }

    void DetectPlacementClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseStartPos = Mouse.current.position.ReadValue();
            clickPending  = true;
        }

        if (Mouse.current.leftButton.isPressed && clickPending)
        {
            float drag = Vector2.Distance(mouseStartPos, Mouse.current.position.ReadValue());
            if (drag > clickThreshold)
                clickPending = false;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && clickPending)
        {
            PlaceBuilding();
            clickPending = false;
        }
    }

    void PlaceBuilding()
    {
        if (shopBuildings == null || shopBuildings.Count == 0 || ghost == null) return;

        Vector3 pos = ghost.transform.position;

        if (takenPositions.Contains(pos)) return;

        int idx = Mathf.Clamp(selectedIndex, 0, shopBuildings.Count - 1);
        GameObject prefab = shopBuildings[idx].prefab;
        if (prefab == null) return;

        GameObject placed = Instantiate(prefab, pos, Quaternion.identity);

        int buildingLayer = LayerMask.NameToLayer("Building");
        if (buildingLayer != -1) SetLayerRecursively(placed, buildingLayer);

        SelectableBuilding selectable = placed.GetComponent<SelectableBuilding>();
        if (selectable == null) selectable = placed.AddComponent<SelectableBuilding>();

        selectable.buildingID    = shopBuildings[idx].id;
        selectable.buildingLevel = 1;

        if (placed.GetComponentInChildren<Collider>() == null)
        {
            var bc = placed.AddComponent<BoxCollider>();
            FitColliderToRenderers(placed, bc);
        }

        takenPositions.Add(pos);

        buildMode = false;
        RefreshBuildMode();
    }

    void MoveGhostToMouse()
    {
        if (ghost == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        if (!RaycastToGround(mouseScreen, out Vector3 hitPoint))
        {
            ghost.SetActive(false);
            return;
        }

        if (!ghost.activeSelf) ghost.SetActive(true);

        Vector3 snapped = SnapToGrid(hitPoint);
        ghost.transform.position = snapped;

        bool occupied = takenPositions.Contains(snapped);
        PaintGhost(occupied
            ? new Color(1f, 0f, 0f, 0.5f)
            : new Color(1f, 1f, 1f, 0.5f));
    }

    void HandleHotkeys()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchBuilding(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchBuilding(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchBuilding(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchBuilding(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchBuilding(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SwitchBuilding(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SwitchBuilding(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) SwitchBuilding(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) SwitchBuilding(8);
    }

    void SwitchBuilding(int index)
    {
        if (shopBuildings == null || shopBuildings.Count == 0) return;
        index = Mathf.Clamp(index, 0, shopBuildings.Count - 1);
        if (selectedIndex == index) return;

        selectedIndex = index;
        if (ghost != null) Destroy(ghost);
        CreateGhost();
        RefreshBuildMode();
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            pos.y,
            Mathf.Round(pos.z / gridSize) * gridSize
        );
    }

    bool RaycastToGround(Vector2 screenPos, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        Ray ray  = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDist, buildPlaneMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            return true;
        }
        return false;
    }

    void PaintGhost(Color color)
    {
        if (ghost == null) return;
        foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
        {
            var mat = r.material;
            mat.color = color;
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    static void FitColliderToRenderers(GameObject go, BoxCollider bc)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        bc.center = go.transform.InverseTransformPoint(b.center);
        Vector3 size = go.transform.InverseTransformVector(b.size);
        bc.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public ShopBuildingEntry GetBuildingEntry(Data.BuildingID id)
    {
        foreach (var entry in shopBuildings)
            if (entry.id == id) return entry;
        return null;
    }

    public bool StartPlacingFromShop(Data.BuildingID id)
    {
        int index = -1;
        for (int i = 0; i < shopBuildings.Count; i++)
        {
            if (shopBuildings[i].id == id) { index = i; break; }
        }

        if (index < 0)
        {
            Debug.LogWarning($"GridSystem: '{id}' introuvable dans shopBuildings. Verifie l Inspector.");
            return false;
        }

        if (selectedIndex != index)
        {
            selectedIndex = index;
            if (ghost != null) Destroy(ghost);
            CreateGhost();
        }

        buildMode = true;
        RefreshBuildMode();
        return true;
    }

    public void FinalizeShopPlacement()
    {
        if (!buildMode || ghost == null) return;
        if (ghost.activeSelf) takenPositions.Add(ghost.transform.position);
        buildMode = false;
        RefreshBuildMode();
    }

    public void CancelShopPlacement()
    {
        buildMode = false;
        RefreshBuildMode();
    }

    public void RegisterOccupiedPosition(Vector3 worldPos)
    {
        takenPositions.Add(SnapToGrid(worldPos));
    }
}
