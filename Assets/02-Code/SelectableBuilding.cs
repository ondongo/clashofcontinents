using UnityEngine;

public class SelectableBuilding : MonoBehaviour
{
    public Color selectedEmissionColor = Color.cyan;
    public float emissionIntensity = 2f;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    bool selected;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();

        // Active l’émission sur les matériaux (sinon _EmissionColor peut être ignoré)
        foreach (var r in renderers)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m == null) continue;
                m.EnableKeyword("_EMISSION");
            }
        }
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