using UnityEngine;
using UnityEngine.EventSystems;
using ClashOfContinents;

public class SelectableBuilding : MonoBehaviour
{
    public Data.BuildingID  buildingID    = Data.BuildingID.townhall;
    public int              buildingLevel = 1;

    public static SelectableBuilding selected { get; private set; }

    public Color selectedEmissionColor = Color.cyan;
    public float emissionIntensity     = 2f;

    Renderer[]          renderers;
    MaterialPropertyBlock mpb;
    bool                isSelected;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb       = new MaterialPropertyBlock();

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
        if (Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Select();
                    return;
                }
            }

            if (isSelected) Deselect();
        }
    }

    public void Select()
    {
        if (selected != null && selected != this)
            selected.Deselect();

        selected    = this;
        isSelected  = true;
        ApplyVisual();

        OpenSelectedUI();
        OpenUpgradeUI();
    }

    public void Deselect()
    {
        isSelected = false;
        if (selected == this) selected = null;
        ApplyVisual();

        if (UI_SelectedBuilding.instance != null)
            UI_SelectedBuilding.instance.SetStatus(false);
    }

    void OpenSelectedUI()
    {
        if (UI_SelectedBuilding.instance == null) return;

        UI_SelectedBuilding.instance.OpenForGridBuilding(this);
    }

    void OpenUpgradeUI()
    {
        if (UI_BuildingUpgrade.instanse == null) return;

        UI_BuildingUpgrade.instanse.OpenMock(buildingID, buildingLevel);
    }

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
