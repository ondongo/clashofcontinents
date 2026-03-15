using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuItemUI : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public Button button;

    Color normalBg = new Color(1,1,1,0.08f);
    Color selectedBg = new Color(1,1,1,0.20f);
    Color disabledBg = new Color(1,1,1,0.03f);

    public void Set(string key, string name, int count, bool selected, bool interactable, System.Action onClick)
    {
        if (keyText) keyText.text = key;
        if (nameText) nameText.text = name;
        if (countText) countText.text = "x" + count;

        if (button)
        {
            button.interactable = interactable;
            button.onClick.RemoveAllListeners();
            if (interactable && onClick != null)
                button.onClick.AddListener(() => onClick());
        }

        if (background)
            background.color = !interactable ? disabledBg : (selected ? selectedBg : normalBg);

        float alpha = interactable ? 1f : 0.45f;
        if (nameText) nameText.alpha = alpha;
        if (countText) countText.alpha = alpha;
        if (keyText) keyText.alpha = alpha;
    }
}