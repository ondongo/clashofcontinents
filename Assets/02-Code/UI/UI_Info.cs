namespace ClashOfContinents
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_Info : MonoBehaviour
    {

        [SerializeField] private GameObject _elements = null;
        [SerializeField] private Button _closeButton = null;
        [SerializeField] public TextMeshProUGUI _titleText = null;
        [SerializeField] public TextMeshProUGUI _descriptionText = null;
        [SerializeField] public Image _icon = null;

        private static UI_Info _instance = null; public static UI_Info instanse { get { return _instance; } }
        private bool _active = false; public bool isActive { get { return _active; } }

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        private void Start()
        {
            _closeButton.onClick.AddListener(Close);
        }

        private void Close()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            _active = false;
            _elements.SetActive(false);
        }

        public void OpenUnitInfo(Data.UnitID id)
        {
            Sprite icon = AssetsBank.GetUnitIcon(id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            _titleText.text = id.ToString();
            _descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            switch (id)
            {
                case Data.UnitID.barbarian:
                    _descriptionText.text = "Ces unités sont des soldats de mêlée à cible unique. Ce sont les premières troupes débloquées pour le combat et elles ne sont pas aussi puissantes que les autres. Cependant, avec la bonne stratégie, elles peuvent servir de bouclier pendant que vos autres troupes attaquent.";
                    break;
                case Data.UnitID.archer:
                    _descriptionText.text = "Ces unités occupent un seul emplacement de logement. Elles attaquent une cible unique, aérienne ou terrestre. Leur faible PV est compensé par leur excellente portée. Ces tireurs d'élite aiment garder leurs distances sur le champ de bataille.";
                    break;
                case Data.UnitID.goblin:
                    _descriptionText.text = "Ces unités ciblent en priorité les bâtiments de ressources par-dessus tout autre objectif, et ignorent tous les autres bâtiments ennemis tant qu'il reste des bâtiments de ressources sur le champ de bataille.";
                    break;
                case Data.UnitID.healer:
                    _descriptionText.text = "Une unité volante sans capacité d'attaque, mais capable de soigner toutes les troupes terrestres à portée.";
                    break;
                case Data.UnitID.wallbreaker:
                    _descriptionText.text = "Cette unité cible les Murs et leur inflige des dégâts massifs. Elle localise le bâtiment protégé le plus proche, détruit les murs qui le protègent, puis s'autodétruit dans le processus.";
                    break;
                case Data.UnitID.giant:
                    _descriptionText.text = "Ces unités ciblent en priorité les défenses ennemies, ce qui en fait des troupes idéales pour les neutraliser rapidement. Cependant, leurs faibles dégâts d'attaque les rendent plus efficaces en grands groupes.";
                    break;
                case Data.UnitID.miner:
                    _descriptionText.text = "Ces unités peuvent creuser sous terre et surgir n'importe où sur le champ de bataille.";
                    break;
                case Data.UnitID.balloon:
                    _descriptionText.text = "Ces unités infligent de lourds dégâts pour leur taille, mais ont une faible portée et cadence d'attaque. Elles ne peuvent pas toucher les unités aériennes. Leur priorité est d'attaquer les défenses ennemies.";
                    break;
                case Data.UnitID.wizard:
                    _descriptionText.text = "Ce sont des unités terrestres fragiles avec de forts dégâts de zone. Elles sont généralement utilisées en grands groupes pour le soutien au feu ou comme multiplicateur de force, mais peuvent aussi être efficaces en plus petit nombre.";
                    break;
                case Data.UnitID.dragon:
                    _descriptionText.text = "Ce sont de redoutables unités volantes capables d'attaquer aussi bien les unités terrestres qu'aériennes, avec une santé et des dégâts élevés.";
                    break;
                case Data.UnitID.pekka:
                    _descriptionText.text = "Ce sont des troupes de mêlée lentes à cible unique qui occupent un grand nombre d'emplacements de logement, mais disposent de points de vie et de dégâts considérables.";
                    break;
                case Data.UnitID.babydragon:
                    _descriptionText.text = "Ce sont d'excellentes troupes de pillage grâce à leurs dégâts élevés et leur santé correcte. Elles peuvent rapidement détruire les collecteurs extérieurs. Cependant, les défenses anti-aériennes peuvent les abattre facilement.";
                    break;
                default:
                    _descriptionText.text = "";
                    break;
            }
            _active = true;
            transform.SetAsLastSibling();
            _elements.SetActive(true);
            _titleText.ForceMeshUpdate(true);
            _descriptionText.ForceMeshUpdate(true);
        }

        public void OpenBuildingInfo(Data.BuildingID id, int level)
        {
            Sprite icon = AssetsBank.GetBuildingIcon(id, level);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            _titleText.text = id.ToString();
            _descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            switch (id)
            {
                case Data.BuildingID.townhall:
                    _descriptionText.text = "C'est le bâtiment principal de votre village. L'améliorer débloque de nouveaux bâtiments, troupes et sorts. Sa protection est cruciale : les forces ennemies obtiennent une étoile rien qu'en le détruisant au combat.";
                    break;
                case Data.BuildingID.goldmine:
                    _descriptionText.text = "Ce bâtiment collecte de l'Or depuis une réserve souterraine illimitée et le stocke jusqu'à ce que le joueur le récupère. Lorsqu'il est plein, la production s'arrête jusqu'à collecte ou pillage par un ennemi.";
                    break;
                case Data.BuildingID.goldstorage:
                    _descriptionText.text = "Tout votre précieux Or est stocké ici. Ce bâtiment permet de conserver l'Or pour les futures améliorations. Sa protection est essentielle car les forces ennemies peuvent voler l'Or à l'intérieur durant le combat.";
                    break;
                case Data.BuildingID.elixirmine:
                    _descriptionText.text = "Ce bâtiment collecte de l'Élixir depuis une réserve souterraine illimitée et le stocke jusqu'à ce que le joueur le récupère. Lorsqu'il est plein, la production s'arrête jusqu'à collecte ou pillage par un ennemi.";
                    break;
                case Data.BuildingID.elixirstorage:
                    _descriptionText.text = "Tout votre précieux Élixir est stocké ici. Ce bâtiment permet de conserver l'Élixir pour les futures améliorations. Sa protection est essentielle car les forces ennemies peuvent voler l'Élixir à l'intérieur durant le combat.";
                    break;
                case Data.BuildingID.buildershut:
                    _descriptionText.text = "Rien ne se fait sans les Constructeurs. Chacune de ces cabanes ne peut héberger qu'un seul Constructeur à la fois. Vous ne pouvez rien construire ni améliorer sans avoir un Constructeur disponible.";
                    break;
                case Data.BuildingID.armycamp:
                    _descriptionText.text = "Vos troupes sont stationnées ici. Ce bâtiment débloque de la capacité d'armée. Une capacité plus élevée vous permet de constituer une armée plus grande.";
                    break;
                case Data.BuildingID.barracks:
                    _descriptionText.text = "Ce bâtiment vous permet d'entraîner des troupes pour attaquer vos ennemis. Améliorez-le pour débloquer des unités avancées. Chaque niveau débloque une nouvelle troupe. Les troupes en file d'attente peuvent être annulées à tout moment, mais les ressources ne sont pas remboursées.";
                    break;
                case Data.BuildingID.wall:
                    _descriptionText.text = "Les Murs servent principalement à entraver les troupes terrestres ennemies, permettant aux défenses de les blesser pendant qu'elles tentent de percer. Une fois percés, les attaquants peuvent s'en prendre librement aux défenses et bâtiments à l'intérieur.";
                    break;
                case Data.BuildingID.cannon:
                    _descriptionText.text = "C'est une défense à cible unique qui inflige des dégâts modérés. C'est la première structure défensive construite par un joueur. Elle est économique et se met à niveau rapidement aux niveaux inférieurs.";
                    break;
                case Data.BuildingID.archertower:
                    _descriptionText.text = "C'est une défense à cible unique dans le jeu. C'est la deuxième défense disponible et la première capable d'attaquer les troupes aériennes. Ces tours sont très polyvalentes : elles peuvent cibler aussi bien les unités terrestres qu'aériennes avec une excellente portée.";
                    break;
                case Data.BuildingID.mortor:
                    _descriptionText.text = "Il tire des obus explosifs à longue portée infligeant de lourds dégâts de zone à chaque unité terrestre dans un petit rayon. Ses dégâts de zone combinés à sa longue portée en font une arme redoutable contre les groupes d'ennemis faibles.";
                    break;
                case Data.BuildingID.airdefense:
                    _descriptionText.text = "C'est une tourelle puissante qui cible exclusivement les ennemis aériens avec de très lourds dégâts. Elle a une bonne portée et des points de vie corrects. Elle ne peut cibler et tirer que sur une seule troupe aérienne à la fois.";
                    break;
                case Data.BuildingID.wizardtower:
                    _descriptionText.text = "Ce bâtiment peut infliger de puissants dégâts de zone aux unités terrestres et aériennes, bien qu'il soit limité à une portée relativement courte.";
                    break;
                case Data.BuildingID.infernotower:
                    _descriptionText.text = "C'est l'une des défenses les plus redoutées du jeu. Ce bâtiment projette un jet de flammes capable de brûler même les armures les plus épaisses.";
                    break;
                case Data.BuildingID.clancastle:
                    _descriptionText.text = "Ce bâtiment est nécessaire pour créer ou rejoindre un Clan. En faisant partie d'un Clan, vous pouvez participer aux guerres et progresser dans le jeu avec vos coéquipiers.";
                    break;
                case Data.BuildingID.spellfactory:
                    _descriptionText.text = "Ce bâtiment permet au joueur de créer des sorts. Vous pouvez utiliser les capacités des sorts pour prendre l'avantage au combat. Les utiliser au bon moment et sur la bonne cible peut changer le cours d'une bataille.";
                    break;
                case Data.BuildingID.laboratory:
                    _descriptionText.text = "C'est ici que vous pouvez améliorer vos Troupes et vos Sorts en augmentant leurs statistiques comme les dégâts et les points de vie.";
                    break;
                case Data.BuildingID.obstacle:
                    _descriptionText.text = "";
                    break;
                default:
                    _descriptionText.text = "";
                    break;
            }
            _active = true;
            transform.SetAsLastSibling();
            _elements.SetActive(true);
            _titleText.ForceMeshUpdate(true);
            _descriptionText.ForceMeshUpdate(true);
        }

        public void OpenSpellInfo(Data.SpellID id)
        {
            Sprite icon = AssetsBank.GetSpellIcon(id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            _titleText.text = id.ToString();
            _descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            switch (id)
            {
                case Data.SpellID.lightning:
                    _descriptionText.text = "Ce sort inflige des dégâts aux bâtiments et troupes ennemis dans un petit rayon.";
                    break;
                case Data.SpellID.healing:
                    _descriptionText.text = "Ce sort crée un anneau sur le champ de bataille qui soigne toutes les troupes à l'intérieur.";
                    break;
                case Data.SpellID.rage:
                    _descriptionText.text = "Ce sort crée un anneau sur le champ de bataille qui augmente la vitesse de déplacement et les dégâts de toutes les troupes alliées à l'intérieur.";
                    break;
                case Data.SpellID.freeze:
                    _descriptionText.text = "Ce sort est utilisé pour désactiver temporairement les défenses ennemies dans un petit rayon.";
                    break;
                case Data.SpellID.invisibility:
                    _descriptionText.text = "Ce sort rend vos troupes dans son rayon invisibles aux défenses ennemies. Les projectiles déjà tirés avant l'invisibilité atteignent tout de même leur cible.";
                    break;
                default:
                    _descriptionText.text = "";
                    break;
            }
            _active = true;
            transform.SetAsLastSibling();
            _elements.SetActive(true);
            _titleText.ForceMeshUpdate(true);
            _descriptionText.ForceMeshUpdate(true);
        }

    }
}