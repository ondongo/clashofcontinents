using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using ClashOfContinents;

/************************************************************/
/*                      GRIDSYSTEM                          */
/*                                                          */
/*  Ce script gere TOUT le placement de batiments en 3D.   */
/*                                                          */
/*  FLUX COMPLET :                                          */
/*  1. UI_Building.Clicked() → StartPlacingFromShop(id)    */
/*  2. Un ghost (fantome) du batiment suit la souris        */
/*  3. L'utilisateur clique sur le terrain → PlaceObject() */
/*  4. Le batiment est pose definitivement                  */
/*                                                          */
/*  CONFIGURATION INSPECTOR :                               */
/*  → shopBuildings : ajouter une entree par batiment       */
/*    (id + prefab + cout en or/elixir/gems)                */
/*  → buildPlaneMask : cocher uniquement le layer du sol    */
/*  → gridSize : taille d'une case (1 = 1 unite Unity)      */
/************************************************************/

public class GridSystem : MonoBehaviour
{
    /************************************************************/
    /*                     SINGLETON                            */
    /*  Permet d'appeler GridSystem.instance depuis n'importe  */
    /*  quel script sans chercher le composant dans la scene   */
    /************************************************************/
    public static GridSystem instance { get; private set; }


    /************************************************************/
    /*               ENTREE DU CATALOGUE (SHOP)                 */
    /*                                                          */
    /*  Une entree = un batiment achetable dans le shop.        */
    /*  Tu remplis cette liste dans l'Inspector.                */
    /*                                                          */
    /*  Exemple pour le Cannon :                               */
    /*    id          = cannon                                  */
    /*    prefab      = cannon.prefab                          */
    /*    costGold    = 250                                     */
    /*    costElixir  = 0                                       */
    /*    costGems    = 0                                       */
    /*                                                          */
    /*  Liste de tous tes prefabs disponibles :                 */
    /*    airdefense, archertower, armycamp, barracks,          */
    /*    buildershut, cannon, clancastle, elixirmine,          */
    /*    elixirstorage, goldmine, goldstorage, infernotower,   */
    /*    laboratory, mortor, obstacle, spellfactory,           */
    /*    hiddentesla, townhall, wall, wizardtower              */
    /************************************************************/
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


    /************************************************************/
    /*                   PARAMETRES DE LA GRILLE                */
    /*                                                          */
    /*  gridSize       : taille d'une cellule en unites Unity   */
    /*  clickThreshold : pixels de deplacement souris toleres   */
    /*                   avant d'annuler le placement           */
    /*  toggleBuildKey : touche clavier pour activer le mode    */
    /*                   construction manuellement (defaut : B) */
    /*  buildPlaneMask : layer du sol sur lequel on raycaste    */
    /*  raycastMaxDist : distance max du raycast vers le sol    */
    /************************************************************/
    [Header("Parametres de la grille")]
    public float    gridSize            = 1f;
    public float    clickThreshold      = 10f;
    public Key      toggleBuildKey      = Key.B;
    public LayerMask buildPlaneMask;
    public float    raycastMaxDist      = 1000f;


    /************************************************************/
    /*                      ETAT INTERNE                        */
    /************************************************************/

    /* Vrai quand on est en mode placement (ghost visible) */
    public bool buildMode    = false;

    /* Index du batiment selectionne dans shopBuildings */
    public int  selectedIndex = 0;

    /* Le ghost : copie transparente du prefab qui suit la souris */
    private GameObject ghost;

    /* Positions deja occupees (HashSet = verification instantanee) */
    private readonly HashSet<Vector3> takenPositions = new HashSet<Vector3>();

    private Camera  cam;
    private Vector2 mouseStartPos;  /* position souris au moment du clic */
    private bool    clickPending;   /* vrai si un clic est en cours */


    /************************************************************/
    /*                       UNITY EVENTS                       */
    /************************************************************/

    void Awake()
    {
        /************************************************************/
        /*  Enregistre cette instance comme singleton              */
        /************************************************************/
        instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        /************************************************************/
        /*  Cree le ghost au demarrage (invisible car buildMode=false) */
        /************************************************************/
        CreateGhost();
        RefreshBuildMode();
    }

    void Update()
    {
        /************************************************************/
        /*  Touche B (ou toggleBuildKey) : bascule le mode         */
        /*  construction pour tester sans passer par le shop       */
        /************************************************************/
        if (Keyboard.current != null && Keyboard.current[toggleBuildKey].wasPressedThisFrame)
        {
            buildMode = !buildMode;
            RefreshBuildMode();
        }

        /* Si on n'est pas en mode construction, rien a faire */
        if (!buildMode) return;

        /* Gestion des touches 1-9 pour changer de batiment */
        HandleHotkeys();

        if (Mouse.current == null || cam == null) return;

        /* Met a jour la position du ghost sous la souris */
        MoveGhostToMouse();

        /* Detecte le clic pour placer le batiment */
        DetectPlacementClick();
    }


    /************************************************************/
    /*                    GESTION DU GHOST                      */
    /*                                                          */
    /*  Le ghost est une copie semi-transparente du prefab.     */
    /*  Blanc  = position libre → on peut poser               */
    /*  Rouge  = position occupee → on ne peut pas poser       */
    /************************************************************/

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

        /* Desactive tous les colliders du ghost pour qu'il ne bloque pas les raycasts */
        foreach (var col in ghost.GetComponentsInChildren<Collider>())
            col.enabled = false;

        PaintGhost(new Color(1f, 1f, 1f, 0.5f));
    }


    /************************************************************/
    /*                   DETECTION DU CLIC                      */
    /*                                                          */
    /*  On ne place QUE si :                                    */
    /*  - Le clic n'est pas sur un element UI (bouton, panel)   */
    /*  - La souris n'a pas trop bouge (pas un drag)            */
    /*  - Le bouton gauche vient d'etre relache                 */
    /************************************************************/

    void DetectPlacementClick()
    {
        /* Ignore si la souris survole un element UI */
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
                clickPending = false; /* trop de deplacement = drag, on annule */
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && clickPending)
        {
            PlaceBuilding();
            clickPending = false;
        }
    }


    /************************************************************/
    /*                    PLACEMENT FINAL                       */
    /*                                                          */
    /*  Instancie le prefab reel a la position du ghost.        */
    /*  Marque la case comme occupee.                          */
    /*  Quitte le mode construction.                            */
    /************************************************************/

    void PlaceBuilding()
    {
        if (shopBuildings == null || shopBuildings.Count == 0 || ghost == null) return;

        Vector3 pos = ghost.transform.position;

        /* Refuse si la case est deja occupee */
        if (takenPositions.Contains(pos)) return;

        int idx = Mathf.Clamp(selectedIndex, 0, shopBuildings.Count - 1);
        GameObject prefab = shopBuildings[idx].prefab;
        if (prefab == null) return;

        /* Instancie le vrai batiment */
        GameObject placed = Instantiate(prefab, pos, Quaternion.identity);

        /* Assigne le layer "Building" pour la detection ulterieure */
        int buildingLayer = LayerMask.NameToLayer("Building");
        if (buildingLayer != -1) SetLayerRecursively(placed, buildingLayer);

        /* Ajoute SelectableBuilding si pas present (pour pouvoir cliquer dessus) */
        SelectableBuilding selectable = placed.GetComponent<SelectableBuilding>();
        if (selectable == null) selectable = placed.AddComponent<SelectableBuilding>();

        /* Passe l ID et le niveau au SelectableBuilding */
        selectable.buildingID    = shopBuildings[idx].id;
        selectable.buildingLevel = 1;

        /* Ajoute un BoxCollider si le prefab n'en a pas */
        if (placed.GetComponentInChildren<Collider>() == null)
        {
            var bc = placed.AddComponent<BoxCollider>();
            FitColliderToRenderers(placed, bc);
        }

        /* Marque la position comme occupee */
        takenPositions.Add(pos);

        /* Sort du mode construction */
        buildMode = false;
        RefreshBuildMode();
    }


    /************************************************************/
    /*                  DEPLACEMENT DU GHOST                    */
    /************************************************************/

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

        /* Rouge si occupe, blanc si libre */
        bool occupied = takenPositions.Contains(snapped);
        PaintGhost(occupied
            ? new Color(1f, 0f, 0f, 0.5f)
            : new Color(1f, 1f, 1f, 0.5f));
    }


    /************************************************************/
    /*              TOUCHES RACCOURCIS (1 a 9)                  */
    /*  Permettent de changer de batiment en mode construction  */
    /************************************************************/

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


    /************************************************************/
    /*                     UTILITAIRES                          */
    /************************************************************/

    /* Snappe la position sur la grille (arrondi au gridSize le plus proche) */
    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            pos.y,
            Mathf.Round(pos.z / gridSize) * gridSize
        );
    }

    /* Raycast depuis la camera vers le sol (buildPlaneMask) */
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

    /* Applique une couleur semi-transparente a tous les renderers du ghost */
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

    /* Ajuste un BoxCollider pour encadrer exactement tous les renderers */
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

    /* Assigne un layer a un GameObject ET tous ses enfants */
    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }


    /************************************************************/
    /*                      API PUBLIQUE                        */
    /*  Ces methodes sont appelees depuis UI_Building           */
    /************************************************************/

    /* Retourne l entree de shopBuildings pour un BuildingID donne */
    public ShopBuildingEntry GetBuildingEntry(Data.BuildingID id)
    {
        foreach (var entry in shopBuildings)
            if (entry.id == id) return entry;
        return null;
    }

    /************************************************************/
    /*  StartPlacingFromShop                                    */
    /*  Appele quand le joueur achete un batiment dans le shop. */
    /*  Active le mode construction avec le bon prefab.         */
    /*  Retourne true si le batiment est trouve dans la liste.  */
    /************************************************************/
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

        /* Recrée le ghost si le batiment change */
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

    /* Valide le placement depuis UI_Build (si ce panel est utilise) */
    public void FinalizeShopPlacement()
    {
        if (!buildMode || ghost == null) return;
        if (ghost.activeSelf) takenPositions.Add(ghost.transform.position);
        buildMode = false;
        RefreshBuildMode();
    }

    /* Annule le placement et rembourse (gere dans UI_Building) */
    public void CancelShopPlacement()
    {
        buildMode = false;
        RefreshBuildMode();
    }

    /* Enregistre une position deja occupee (sync externe) */
    public void RegisterOccupiedPosition(Vector3 worldPos)
    {
        takenPositions.Add(SnapToGrid(worldPos));
    }
}
