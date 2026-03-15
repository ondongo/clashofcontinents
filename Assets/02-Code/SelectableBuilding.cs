using UnityEngine;

public class SelectableBuilding : MonoBehaviour
{
    [Header("Info")]
    public string buildingName;
    public int level = 1;

    [Header("Selection Visual")]
    public Color selectedEmissionColor = Color.cyan;
    public float emissionIntensity = 2f;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    bool selected;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();

        foreach (var r in renderers)
            foreach (var m in r.sharedMaterials)
                if (m != null) m.EnableKeyword("_EMISSION");
    }

    public void Init(string name, int lvl)
    {
        buildingName = name;
        level = lvl;
    }

    public void SetSelected(bool value)
    {
        selected = value;
        ApplyVisual();
    }

    void ApplyVisual()
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", selected ? selectedEmissionColor * emissionIntensity : Color.black);
            r.SetPropertyBlock(mpb);
        }
    }
}