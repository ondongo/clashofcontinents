namespace ClashOfContinents
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UI_Building : MonoBehaviour
    {
        [SerializeField] private Data.BuildingID _id = Data.BuildingID.townhall;
        public Data.BuildingID id { set { _id = value; } }

        [SerializeField] private Button             _button      = null;
        [SerializeField] private Button             _buttonInfo  = null;
        [SerializeField] private Image              _icon        = null;
        [SerializeField] private Image              _resourceIcon = null;
        [SerializeField] public  TextMeshProUGUI    _titleText   = null;
        [SerializeField] public  TextMeshProUGUI    _resourceText = null;
        [SerializeField] public  TextMeshProUGUI    _timeText    = null;
        [SerializeField] public  TextMeshProUGUI    _countText   = null;

        private void Start()
        {
            _button.onClick.AddListener(Clicked);
            _buttonInfo.onClick.AddListener(Info);
        }

        private void Clicked()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);

            var entry = GridSystem.instance?.GetBuildingEntry(_id);

            if (entry == null)
            {
                Debug.LogWarning($"UI_Building: '{_id}' absent de GridSystem.shopBuildings.");
                return;
            }

            if (!CanAfford(entry))
            {
                Debug.Log($"Pas assez de ressources pour '{_id}'.");
                return;
            }

            Player.instanse.gold       -= entry.costGold;
            Player.instanse.elixir     -= entry.costElixir;
            Player.instanse.darkElixir -= entry.costDarkElixir;
            Player.instanse.data.gems  -= entry.costGems;

            Player.instanse.UpdateResourcesUI();

            UI_Shop.instanse.SetStatus(false);
            UI_Main.instanse.SetStatus(true);

            if (!GridSystem.instance.StartPlacingFromShop(_id))
                Refund(entry);
        }

        public void Initialize(bool haveWorker)
        {
            if (_titleText != null)
            {
                _titleText.text = _id.ToString();
                _titleText.ForceMeshUpdate(true);
            }

            var entry = GridSystem.instance?.GetBuildingEntry(_id);
            Sprite icon = AssetsBank.GetBuildingIcon(_id);
            if (icon == null && entry != null) icon = entry.icon;
            if (icon != null && _icon != null) _icon.sprite = icon;

            if (entry != null)
            {
                if (_timeText    != null) _timeText.text  = "0";
                if (_countText   != null) _countText.text = "-";

                if (entry.costGold > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costGold.ToString();
                    if (_resourceIcon != null) _resourceIcon.sprite = AssetsBank.instanse.goldIcon;
                }
                else if (entry.costElixir > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costElixir.ToString();
                    if (_resourceIcon != null) _resourceIcon.sprite = AssetsBank.instanse.elixirIcon;
                }
                else if (entry.costDarkElixir > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costDarkElixir.ToString();
                    if (_resourceIcon != null) _resourceIcon.sprite = AssetsBank.instanse.darkIcon;
                }
                else
                {
                    if (_resourceText != null) _resourceText.text   = entry.costGems.ToString();
                    if (_resourceIcon != null) _resourceIcon.sprite = AssetsBank.instanse.gemsIcon;
                }

                bool affordable = CanAfford(entry);
                if (_resourceText != null) _resourceText.color = affordable ? Color.white : Color.red;
                if (_button != null)       _button.interactable = haveWorker && affordable;
            }
            else
            {
                if (_resourceText != null) { _resourceText.color = Color.white; _resourceText.text = "0"; }
                if (_timeText     != null) _timeText.text  = "0";
                if (_countText    != null) _countText.text = "-";
                if (_resourceIcon != null) _resourceIcon.sprite = AssetsBank.instanse.gemsIcon;
                if (_button       != null) _button.interactable = haveWorker;
            }

            if (_resourceText != null) _resourceText.ForceMeshUpdate(true);
            if (_timeText     != null) _timeText.ForceMeshUpdate(true);
            if (_countText    != null) _countText.ForceMeshUpdate(true);
        }

        private void Info()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            UI_Info.instanse.OpenBuildingInfo(_id, 1);
        }

        private bool CanAfford(GridSystem.ShopBuildingEntry entry)
        {
            return Player.instanse.gold       >= entry.costGold
                && Player.instanse.elixir     >= entry.costElixir
                && Player.instanse.darkElixir >= entry.costDarkElixir
                && Player.instanse.data.gems  >= entry.costGems;
        }

        private void Refund(GridSystem.ShopBuildingEntry entry)
        {
            Player.instanse.gold       += entry.costGold;
            Player.instanse.elixir     += entry.costElixir;
            Player.instanse.darkElixir += entry.costDarkElixir;
            Player.instanse.data.gems  += entry.costGems;
        }
    }
}
