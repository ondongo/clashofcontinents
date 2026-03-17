namespace ClashOfContinents
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /************************************************************/
    /*                      UI_BUILDING                         */
    /*                                                          */
    /*  Represente UNE carte de batiment dans le shop.          */
    /*  Le shop instancie autant de UI_Building qu'il y a de    */
    /*  batiments dans _buildingsAvailable (UI_Shop).           */
    /*                                                          */
    /*  FLUX QUAND LE JOUEUR CLIQUE SUR UNE CARTE :             */
    /*  1. Clicked() verifie si le joueur a assez de ressources */
    /*  2. Si oui : deduit les ressources                       */
    /*  3. Ferme le shop, affiche le HUD principal              */
    /*  4. Appelle GridSystem.StartPlacingFromShop(id)          */
    /*     → le ghost du batiment apparait sous la souris       */
    /*  5. Le joueur deplace la souris puis clique pour poser   */
    /************************************************************/

    public class UI_Building : MonoBehaviour
    {
        /************************************************************/
        /*  IMPORTANT : les noms des champs [SerializeField] ne     */
        /*  doivent PAS changer, sinon Unity perd les references    */
        /*  assignees dans le prefab.                               */
        /************************************************************/

        /* ID du batiment que cette carte represente */
        [SerializeField] private Data.BuildingID _id = Data.BuildingID.townhall;
        public Data.BuildingID id { set { _id = value; } }

        [SerializeField] private Button             _button      = null; /* bouton Acheter */
        [SerializeField] private Button             _buttonInfo  = null; /* bouton Info    */
        [SerializeField] private Image              _icon        = null; /* icone batiment */
        [SerializeField] private Image              _resourceIcon = null; /* icone ressource */
        [SerializeField] public  TextMeshProUGUI    _titleText   = null;
        [SerializeField] public  TextMeshProUGUI    _resourceText = null;
        [SerializeField] public  TextMeshProUGUI    _timeText    = null;
        [SerializeField] public  TextMeshProUGUI    _countText   = null;


        /************************************************************/
        /*                      UNITY START                         */
        /************************************************************/

        private void Start()
        {
            _button.onClick.AddListener(Clicked);
            _buttonInfo.onClick.AddListener(Info);
        }


        /************************************************************/
        /*                   CLIC SUR "ACHETER"                     */
        /*                                                          */
        /*  → Verifie les ressources via l entree GridSystem        */
        /*  → Deduit les ressources (or, elixir, gems...)           */
        /*  → Ferme le shop                                         */
        /*  → Active le placement sur GridSystem (ghost 3D)         */
        /************************************************************/

        private void Clicked()
        {
            if (SoundManager.instanse != null)
                SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);

            /* Recupere la config du batiment dans GridSystem.shopBuildings */
            if (GridSystem.instance == null)
            {
                Debug.LogWarning("UI_Building: GridSystem.instance est null. Verifie la scene.");
                return;
            }
            var entry = GridSystem.instance.GetBuildingEntry(_id);

            if (entry == null)
            {
                Debug.LogWarning($"UI_Building: '{_id}' absent de GridSystem.shopBuildings. Ajoute une entree dans l'Inspector de GridSystem.");
                return;
            }

            if (entry.prefab == null)
            {
                Debug.LogWarning($"UI_Building: le prefab de '{_id}' n'est pas assigne dans GridSystem.shopBuildings. Assignez le champ 'prefab' dans l'Inspector.");
                return;
            }

            if (!CanAfford(entry))
            {
                Debug.Log($"Pas assez de ressources pour '{_id}'.");
                return;
            }

            /* Deduit les ressources (Player ou shop local) */
            if (Player.instanse != null && Player.instanse.data != null)
            {
                Player.instanse.gold       -= entry.costGold;
                Player.instanse.elixir     -= entry.costElixir;
                Player.instanse.darkElixir -= entry.costDarkElixir;
                Player.instanse.data.gems  -= entry.costGems;
                if (Player.instanse != null)
                    Player.instanse.UpdateResourcesUI();
            }
            else if (UI_Shop.instanse != null)
            {
                UI_Shop.instanse.gold       -= entry.costGold;
                UI_Shop.instanse.elixir     -= entry.costElixir;
                UI_Shop.instanse.darkElixir -= entry.costDarkElixir;
                UI_Shop.instanse.gems      -= entry.costGems;
            }

            /* Ferme le shop et affiche le HUD */
            if (UI_Shop.instanse != null) UI_Shop.instanse.SetStatus(false);
            if (UI_Main.instanse != null) UI_Main.instanse.SetStatus(true);

            /* Active le ghost sur le GridSystem → joueur clique pour poser */
            if (!GridSystem.instance.StartPlacingFromShop(_id))
                Refund(entry); /* rembourse si le prefab est manquant ou erreur */
        }


        /************************************************************/
        /*                INITIALISATION DE LA CARTE                */
        /*                                                          */
        /*  Appelee par UI_Shop.SetStatus(true) pour chaque carte.  */
        /*  Affiche le nom, l icone, le cout et active/desactive    */
        /*  le bouton selon les ressources du joueur.               */
        /************************************************************/

        public void Initialize(bool haveWorker)
        {
            /* --- Nom du batiment --- */
            if (_titleText != null)
            {
                _titleText.text = _id.ToString();
                _titleText.ForceMeshUpdate(true);
            }

            /* --- Icone batiment --- */
            /* Priorite : icone dans GridSystem.shopBuildings puis AssetsBank (pour fonctionner meme si AssetsBank est null) */
            var entry = GridSystem.instance != null ? GridSystem.instance.GetBuildingEntry(_id) : null;
            Sprite icon = (entry != null && entry.icon != null) ? entry.icon : AssetsBank.GetBuildingIcon(_id);
            if (icon != null && _icon != null)
                _icon.sprite = icon;

            /* --- Cout et bouton --- */
            if (entry != null)
            {
                /* Affiche la ressource principale */
                if (_timeText    != null) _timeText.text  = "0";
                if (_countText   != null) _countText.text = "-";

                if (entry.costGold > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costGold.ToString();
                    if (_resourceIcon != null && AssetsBank.instanse != null) _resourceIcon.sprite = AssetsBank.instanse.goldIcon;
                }
                else if (entry.costElixir > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costElixir.ToString();
                    if (_resourceIcon != null && AssetsBank.instanse != null) _resourceIcon.sprite = AssetsBank.instanse.elixirIcon;
                }
                else if (entry.costDarkElixir > 0)
                {
                    if (_resourceText != null) _resourceText.text   = entry.costDarkElixir.ToString();
                    if (_resourceIcon != null && AssetsBank.instanse != null) _resourceIcon.sprite = AssetsBank.instanse.darkIcon;
                }
                else
                {
                    if (_resourceText != null) _resourceText.text   = entry.costGems.ToString();
                    if (_resourceIcon != null && AssetsBank.instanse != null) _resourceIcon.sprite = AssetsBank.instanse.gemsIcon;
                }

                bool affordable = CanAfford(entry);
                if (_resourceText != null) _resourceText.color = affordable ? Color.white : Color.red;
                if (_button != null)       _button.interactable = haveWorker && affordable;
            }
            else
            {
                /* Pas de config GridSystem : affichage neutre, bouton actif */
                if (_resourceText != null) { _resourceText.color = Color.white; _resourceText.text = "0"; }
                if (_timeText     != null) _timeText.text  = "0";
                if (_countText    != null) _countText.text = "-";
                if (_resourceIcon != null && AssetsBank.instanse != null) _resourceIcon.sprite = AssetsBank.instanse.gemsIcon;
                if (_button       != null) _button.interactable = haveWorker;
            }

            if (_resourceText != null) _resourceText.ForceMeshUpdate(true);
            if (_timeText     != null) _timeText.ForceMeshUpdate(true);
            if (_countText    != null) _countText.ForceMeshUpdate(true);
        }


        /************************************************************/
        /*                   BOUTON INFO                            */
        /************************************************************/

        private void Info()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            UI_Info.instanse.OpenBuildingInfo(_id, 1);
        }


        /************************************************************/
        /*                     HELPERS PRIVES                       */
        /************************************************************/

        private bool CanAfford(GridSystem.ShopBuildingEntry entry)
        {
            if (Player.instanse == null || Player.instanse.data == null)
            {
                /* Mode sans Player : utiliser les ressources locales du shop si disponible */
                if (UI_Shop.instanse != null)
                    return UI_Shop.instanse.gold >= entry.costGold
                        && UI_Shop.instanse.elixir >= entry.costElixir
                        && UI_Shop.instanse.darkElixir >= entry.costDarkElixir
                        && UI_Shop.instanse.gems >= entry.costGems;
                return false;
            }
            return Player.instanse.gold       >= entry.costGold
                && Player.instanse.elixir     >= entry.costElixir
                && Player.instanse.darkElixir >= entry.costDarkElixir
                && Player.instanse.data.gems  >= entry.costGems;
        }

        private void Refund(GridSystem.ShopBuildingEntry entry)
        {
            if (Player.instanse != null && Player.instanse.data != null)
            {
                Player.instanse.gold       += entry.costGold;
                Player.instanse.elixir     += entry.costElixir;
                Player.instanse.darkElixir += entry.costDarkElixir;
                Player.instanse.data.gems  += entry.costGems;
                Player.instanse.UpdateResourcesUI();
            }
            else if (UI_Shop.instanse != null)
            {
                UI_Shop.instanse.gold       += entry.costGold;
                UI_Shop.instanse.elixir     += entry.costElixir;
                UI_Shop.instanse.darkElixir += entry.costDarkElixir;
                UI_Shop.instanse.gems      += entry.costGems;
            }
        }
    }
}
