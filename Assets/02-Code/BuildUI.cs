using System.Collections.Generic;
using UnityEngine;

public class BuildUI : MonoBehaviour
{
    [Header("Build menu")]
    public GameObject buildMenuRoot;
    public Transform listContainer;
    public BuildMenuItemUI itemPrefab;

    [Header("Info sélection")]
    public GameObject infoRoot;
    public TMPro.TextMeshProUGUI infoText;

    readonly List<BuildMenuItemUI> items = new();

    public void RebuildMenu(List<BuildingData> catalog, List<int> remaining, int selectedIndex, System.Action<int> onSelect)
    {
        // clear
        for (int i = 0; i < items.Count; i++)
            Destroy(items[i].gameObject);
        items.Clear();

        bool anyLeft = false;

        for (int i = 0; i < catalog.Count; i++)
        {
            int left = remaining[i];
            if (left > 0) anyLeft = true;

            var it = Instantiate(itemPrefab, listContainer);
            bool interactable = left > 0;

            int idx = i;
            it.Set(
                key: (i + 1).ToString(),
                name: catalog[i].displayName,
                count: left,
                selected: i == selectedIndex,
                interactable: interactable,
                onClick: () => onSelect?.Invoke(idx)
            );

            items.Add(it);
        }

        if (buildMenuRoot) buildMenuRoot.SetActive(anyLeft);
    }

    public void RefreshMenu(List<BuildingData> catalog, List<int> remaining, int selectedIndex, System.Action<int> onSelect)
    {
        // Si le catalogue change peu, tu peux faire un Refresh au lieu de rebuild
        // Mais vu ta taille, rebuild = simple et ok.
        RebuildMenu(catalog, remaining, selectedIndex, onSelect);
    }

    public void ShowBuildingInfo(SelectableBuilding b)
    {
        if (infoRoot == null || infoText == null) return;

        if (b == null)
        {
            infoRoot.SetActive(false);
            return;
        }

        infoRoot.SetActive(true);
        infoText.text = $"{b.buildingName} - Niveau {b.level}";
    }
}