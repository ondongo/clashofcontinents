using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSystem : MonoBehaviour
{
  [Header("Build Catalog")]
  public List<GameObject> buildPrefabs = new List<GameObject>();
  public int selectedIndex = 0;
  public float gridSize = 1f;
  public float clickThreshold = 10f;

  public Key toggleBuildKey = Key.B; // touche pour activer/désactiver le mode construction
  public bool buildMode = false; // état actuel (tu peux le voir dans l’inspector)

  private GameObject ghostObject;
  private readonly HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
  private Camera cam;

  private Vector2 mousePressPosition;
  private bool isClickCandidate;
  public LayerMask buildPlaneMask; // coche uniquement BuildPlane dans l’inspector
  public float raycastMaxDistance = 1000f; // le plane dans la scène

  void Awake()
  {
    cam = Camera.main;
  }

  void Start()
  {
    CreateGhostObject();
    ApplyBuildModeState();
  }

  void Update()
  {
    if (Keyboard.current != null && Keyboard.current[toggleBuildKey].wasPressedThisFrame)
    {
      buildMode = !buildMode;
      ApplyBuildModeState();
    }

    if (!buildMode) return; // caméra continue, mais placement/ghost OFF
    HandleBuildingHotkeys();

    if (Mouse.current == null || cam == null) return;

    UpdateGhostPosition();
    HandleClick();
  }

  void ApplyBuildModeState()
  {
    if (ghostObject != null)
      ghostObject.SetActive(buildMode);

    // reset du click candidate pour éviter un placement “fantôme” au moment du toggle
    isClickCandidate = false;
  }

  void CreateGhostObject()
  {
    if (buildPrefabs == null || buildPrefabs.Count == 0)
    {
      Debug.LogWarning("GridSystem: buildPrefabs est vide.");
      return;
    }

    GameObject prefab = buildPrefabs[Mathf.Clamp(selectedIndex, 0, buildPrefabs.Count - 1)];
    ghostObject = Instantiate(prefab);

    // Désactive tous les colliders du ghost
    foreach (var c in ghostObject.GetComponentsInChildren<Collider>())
      c.enabled = false;

    SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
  }

  void HandleClick()
  {
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      mousePressPosition = Mouse.current.position.ReadValue();
      isClickCandidate = true;
    }

    if (Mouse.current.leftButton.isPressed && isClickCandidate)
    {
      float distance = Vector2.Distance(mousePressPosition, Mouse.current.position.ReadValue());
      if (distance > clickThreshold) isClickCandidate = false; // drag => pas de placement
    }

    if (Mouse.current.leftButton.wasReleasedThisFrame && isClickCandidate)
    {
      PlaceObject();
      isClickCandidate = false;
    }
  }

  void UpdateGhostPosition()
  {
    Vector2 mouse = Mouse.current.position.ReadValue();

    if (!TryGetMousePointOnBuildPlane(mouse, out Vector3 hitPoint))
    {
      ghostObject.SetActive(false);
      return;
    }

    if (!ghostObject.activeSelf) ghostObject.SetActive(true);

    Vector3 snapped = SnapToGridKeepingY(hitPoint);
    ghostObject.transform.position = snapped;

    bool occupied = occupiedPositions.Contains(snapped);
    SetGhostColor(occupied
        ? new Color(1f, 0f, 0f, 0.5f)
        : new Color(1f, 1f, 1f, 0.5f));
  }

  Vector3 SnapToGrid(Vector3 pos, float y)
  {
    return new Vector3(
        Mathf.Round(pos.x / gridSize) * gridSize,
        y,
        Mathf.Round(pos.z / gridSize) * gridSize
    );
  }

  void PlaceObject()
  {
    if (buildPrefabs == null || buildPrefabs.Count == 0) return;

    Vector3 position = ghostObject.transform.position;
    if (occupiedPositions.Contains(position)) return;

    GameObject prefab = buildPrefabs[Mathf.Clamp(selectedIndex, 0, buildPrefabs.Count - 1)];
    var go = Instantiate(prefab, position, Quaternion.identity);

    int buildingLayer = LayerMask.NameToLayer("Building");
    if (buildingLayer != -1) SetLayerRecursively(go, buildingLayer);

    if (go.GetComponent<SelectableBuilding>() == null)
      go.AddComponent<SelectableBuilding>();

    if (go.GetComponentInChildren<Collider>() == null)
    {
      var bc = go.AddComponent<BoxCollider>();
      FitBoxColliderToRenderers(go, bc);
    }

    occupiedPositions.Add(position);
  }

  void SetGhostColor(Color color)
  {
    foreach (Renderer r in ghostObject.GetComponentsInChildren<Renderer>())
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

  static void FitBoxColliderToRenderers(GameObject go, BoxCollider bc)
  {
    var renderers = go.GetComponentsInChildren<Renderer>();
    if (renderers.Length == 0) return;

    Bounds b = renderers[0].bounds;
    for (int i = 1; i < renderers.Length; i++)
      b.Encapsulate(renderers[i].bounds);

    Vector3 centerLocal = go.transform.InverseTransformPoint(b.center);
    Vector3 sizeLocal = go.transform.InverseTransformVector(b.size);

    bc.center = centerLocal;
    bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));
  }

  static void SetLayerRecursively(GameObject obj, int layer)
  {
    obj.layer = layer;
    foreach (Transform child in obj.transform)
      SetLayerRecursively(child.gameObject, layer);
  }

  bool TryGetMousePointOnBuildPlane(Vector2 screenPos, out Vector3 hitPoint)
  {
    hitPoint = Vector3.zero;

    Ray ray = cam.ScreenPointToRay(screenPos);

    if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, buildPlaneMask, QueryTriggerInteraction.Ignore))
    {
      hitPoint = hit.point;
      return true;
    }

    return false;
  }

  Vector3 SnapToGridKeepingY(Vector3 pos)
  {
    return new Vector3(
        Mathf.Round(pos.x / gridSize) * gridSize,
        pos.y,
        Mathf.Round(pos.z / gridSize) * gridSize
    );
  }

  void SelectBuilding(int index)
  {
    if (buildPrefabs == null || buildPrefabs.Count == 0) return;
    index = Mathf.Clamp(index, 0, buildPrefabs.Count - 1);
    if (selectedIndex == index) return;

    selectedIndex = index;

    if (ghostObject != null) Destroy(ghostObject);
    CreateGhostObject();
    ApplyBuildModeState();
  }

  // La méthode qui permet de sélectionner le bâtiment qu'on souhaite selon la touche appuyée
  void HandleBuildingHotkeys()
  {
    if (Keyboard.current == null) return;
    if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectBuilding(0);
    if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectBuilding(1);
    if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectBuilding(2);
    if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectBuilding(3);
    if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectBuilding(4);
    if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectBuilding(5);
    if (Keyboard.current.digit7Key.wasPressedThisFrame) SelectBuilding(6);
    if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectBuilding(7);
    if (Keyboard.current.digit9Key.wasPressedThisFrame) SelectBuilding(8);
  }
}