using UnityEngine;
using UnityEngine.EventSystems;
using ClashOfContinents;

/************************************************************/
/*                  SELECTABLE BUILDING                     */
/*                                                          */
/*  Attache automatiquement sur chaque batiment pose via    */
/*  GridSystem.PlaceBuilding().                             */
/*                                                          */
/*  FONCTIONNEMENT :                                        */
/*  - Stocke l ID et le niveau du batiment                  */
/*  - Detecte le clic souris (via son Collider)             */
/*  - S enregistre comme batiment selectionne               */
/*  - Ouvre UI_SelectedBuilding (nom) et UI_BuildingUpgrade */
/*  - Surligne en cyan quand selectionne                    */
/*                                                          */
/*  DESELECTION : clic n'importe ou = deselectionne tout    */
/************************************************************/

public class SelectableBuilding : MonoBehaviour
{
    /************************************************************/
    /*  Donnees du batiment place                               */
    /************************************************************/
    public Data.BuildingID  buildingID    = Data.BuildingID.townhall;
    public int              buildingLevel = 1;

    /************************************************************/
    /*  Batiment actuellement selectionne (accessible partout)  */
    /************************************************************/
    public static SelectableBuilding selected { get; private set; }

    /************************************************************/
    /*  Visuel de selection                                     */
    /************************************************************/
    public Color selectedEmissionColor = Color.cyan;
    public float emissionIntensity     = 2f;

    Renderer[]          renderers;
    MaterialPropertyBlock mpb;
    bool                isSelected;


    /************************************************************/
    /*                      UNITY EVENTS                        */
    /************************************************************/

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb       = new MaterialPropertyBlock();

        /* Active l emission sur les materiaux pour le surlignage */
        foreach (var r in renderers)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m != null) m.EnableKeyword("_EMISSION");
            }
        }
    }

    void Update()
    {
        /************************************************************/
        /*  Clic gauche : selectionne ce batiment                   */
        /*  Ignore si le clic est sur un element UI                 */
        /************************************************************/
        if (Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                /* Le raycast a touche ce batiment ou un de ses enfants */
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Select();
                    return;
                }
            }

            /* Clic ailleurs → deselectionne */
            if (isSelected) Deselect();
        }
    }


    /************************************************************/
    /*                  SELECTION / DESELECTION                 */
    /************************************************************/

    public void Select()
    {
        /* Deselectionne l ancien batiment si different */
        if (selected != null && selected != this)
            selected.Deselect();

        selected    = this;
        isSelected  = true;
        ApplyVisual();

        /* Ouvre les panels UI */
        OpenSelectedUI();
        OpenUpgradeUI();
    }

    public void Deselect()
    {
        isSelected = false;
        if (selected == this) selected = null;
        ApplyVisual();

        /* Ferme les panels UI */
        if (UI_SelectedBuilding.instance != null)
            UI_SelectedBuilding.instance.SetStatus(false);
    }


    /************************************************************/
    /*                  OUVERTURE DES PANELS UI                 */
    /************************************************************/

    void OpenSelectedUI()
    {
        if (UI_SelectedBuilding.instance == null) return;

        /* Passe la reference a UI_SelectedBuilding pour qu il          */
        /* sache quelle position monde afficher                         */
        UI_SelectedBuilding.instance.OpenForGridBuilding(this);
    }

    void OpenUpgradeUI()
    {
        if (UI_BuildingUpgrade.instanse == null) return;

        /* Ouvre le panel upgrade en mode mock (sans donnees serveur)   */
        UI_BuildingUpgrade.instanse.OpenMock(buildingID, buildingLevel);
    }


    /************************************************************/
    /*                      VISUEL SURLIGNAGE                   */
    /************************************************************/

    void ApplyVisual()
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor",
                isSelected ? selectedEmissionColor * emissionIntensity : Color.black);
            r.SetPropertyBlock(mpb);
        }
    }

    public void SetSelected(bool value)
    {
        if (value) Select();
        else       Deselect();
    }
}
