namespace ClashOfContinents
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    [System.Serializable]
    public class ShopBuildingDef
    {
        public Data.BuildingID id = Data.BuildingID.townhall;
        public int level = 1;
        public int requiredGold = 0;
        public int requiredElixir = 0;
        public int requiredDarkElixir = 0;
        public int requiredGems = 0;
        public int columns = 2;
        public int rows = 2;
        public int buildTime = 0;

        public Data.ServerBuilding ToServerBuilding()
        {
            return new Data.ServerBuilding
            {
                id = id.ToString(),
                level = level,
                requiredGold = requiredGold,
                requiredElixir = requiredElixir,
                requiredDarkElixir = requiredDarkElixir,
                requiredGems = requiredGems,
                columns = columns,
                rows = rows,
                buildTime = buildTime
            };
        }
    }

    public class UI_Shop : MonoBehaviour
    {
        [Header("Ressources (utilisées si pas de Player)")]
        [SerializeField] private int _gold = 5000;
        [SerializeField] private int _elixir = 5000;
        [SerializeField] private int _darkElixir = 500;
        [SerializeField] private int _gems = 500;
        [SerializeField] private int _maxGold = 100000;
        [SerializeField] private int _maxElixir = 100000;
        [SerializeField] private int _maxDarkElixir = 10000;

        [Header("UI")]
        [SerializeField] public GameObject _elements = null;
        [SerializeField] private Button _closeButton = null;
        [SerializeField] public RectTransform _buildingsGrid = null;
        [SerializeField] public TextMeshProUGUI _goldText = null;
        [SerializeField] public TextMeshProUGUI _elixirText = null;
        [SerializeField] public TextMeshProUGUI _darkText = null;
        [SerializeField] public TextMeshProUGUI _gemsText = null;
        [SerializeField] private UI_Building _buildingPrefab = null;
        [SerializeField] private Data.BuildingID[] _buildingsAvailable = null;

        [Header("Données bâtiments (local, pas de serveur)")]
        [SerializeField] private List<ShopBuildingDef> _buildingDefs = new List<ShopBuildingDef>();

        private bool _active = false; public bool isActive { get { return _active; } }
        private static UI_Shop _instance = null; public static UI_Shop instanse { get { return _instance; } }
        private List<UI_Building> ui_buildings = new List<UI_Building>();

        public int gold { get => _gold; set => _gold = value; }
        public int elixir { get => _elixir; set => _elixir = value; }
        public int darkElixir { get => _darkElixir; set => _darkElixir = value; }
        public int gems { get => _gems; set => _gems = value; }
        public int maxGold { get => _maxGold; }
        public int maxElixir { get => _maxElixir; }
        public int maxDarkElixir { get => _maxDarkElixir; }
        public static DateTime GameNow => DateTime.UtcNow;

        private void Awake()
        {
            _instance = this;
            if (_elements != null)
                _elements.SetActive(false);
        }

        private void Start()
        {
            if (_buildingsAvailable != null && _buildingsGrid != null && _buildingPrefab != null)
            {
                Data.BuildingID[] buildingsAvailable = _buildingsAvailable.Distinct().ToArray();
                for (int i = 0; i < buildingsAvailable.Length; i++)
                {
                    UI_Building building = Instantiate(_buildingPrefab, _buildingsGrid);
                    building.id = buildingsAvailable[i];
                    ui_buildings.Add(building);
                }
            }
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseShop);
        }

        public Data.ServerBuilding GetBuildingData(Data.BuildingID id, int level)
        {
            if (_buildingDefs == null) return null;
            for (int i = 0; i < _buildingDefs.Count; i++)
            {
                if (_buildingDefs[i].id == id && _buildingDefs[i].level == level)
                    return _buildingDefs[i].ToServerBuilding();
            }
            return null;
        }

        public bool IsBuildingInShop(Data.BuildingID id)
        {
            if (_buildingsAvailable != null)
            {
                for (int i = 0; i < _buildingsAvailable.Length; i++)
                {
                    if (_buildingsAvailable[i] == id)
                        return true;
                }
            }
            return false;
        }

        public void SetStatus(bool status)
        {
            if (_elements == null)
            {
                Debug.LogWarning("UI_Shop: assignez '_elements' (le panel du shop) dans l'Inspector Unity.");
                return;
            }

            if (status)
            {
                if (Player.instanse != null)
                {
                    if (_goldText != null) _goldText.text = Player.instanse.gold.ToString();
                    if (_elixirText != null) _elixirText.text = Player.instanse.elixir.ToString();
                    if (_darkText != null) _darkText.text = Player.instanse.darkElixir.ToString();
                    if (_gemsText != null) _gemsText.text = Player.instanse.data.gems.ToString();
                }
                else
                {
                    if (_goldText != null) _goldText.text = _gold.ToString();
                    if (_elixirText != null) _elixirText.text = _elixir.ToString();
                    if (_darkText != null) _darkText.text = _darkElixir.ToString();
                    if (_gemsText != null) _gemsText.text = _gems.ToString();
                }

                if (_buildingsGrid != null)
                    _buildingsGrid.anchoredPosition = new Vector2(0, _buildingsGrid.anchoredPosition.y);

                bool haveWorker = true;
                if (Player.instanse != null && Player.instanse.data != null && Player.instanse.data.buildings != null && Player.instanse.data.buildings.Count > 0)
                {
                    int w = 0, bw = 0;
                    for (int i = 0; i < Player.instanse.data.buildings.Count; i++)
                    {
                        if (Player.instanse.data.buildings[i].isConstructing) bw++;
                        if (Player.instanse.data.buildings[i].id == Data.BuildingID.buildershut) w++;
                    }
                    haveWorker = w > 0 ? (w > bw) : true;
                }

                if (ui_buildings != null)
                {
                    for (int i = 0; i < ui_buildings.Count; i++)
                    {
                        if (ui_buildings[i] != null)
                            ui_buildings[i].Initialize(haveWorker);
                    }
                }
            }
            _active = status;
            _elements.SetActive(status);
        }

        public bool PlaceBuilding(Data.BuildingID id, int x = -1, int y = -1)
        {
            if (UI_Main.instanse == null) return false;

            Data.ServerBuilding buildingData = GetBuildingData(id, 1);
            if (buildingData == null) return false;
            var prefab = UI_Main.instanse.GetBuildingPrefab(id, buildingData);
            if (prefab.Item1 == null || prefab.Item2 == null) return false;

            if (x < 0 || y < 0)
            {
                Vector2Int point = UI_Main.instanse._grid.GetBestBuildingPlace(prefab.Item2.rows, prefab.Item2.columns);
                x = point.x;
                y = point.y;
            }

            bool haveResources = _gems >= prefab.Item2.requiredGems && _elixir >= prefab.Item2.requiredElixir
                && _gold >= prefab.Item2.requiredGold && _darkElixir >= prefab.Item2.requiredDarkElixir;
            if (!haveResources) return false;

            Data.Building data = new Data.Building();
            data.id = id;
            data.x = x;
            data.y = y;
            data.level = 1;
            data.databaseID = 0;
            data.columns = prefab.Item2.columns;
            data.rows = prefab.Item2.rows;
            data.buildTime = prefab.Item2.buildTime;
            data.radius = 0;

            _gold -= prefab.Item2.requiredGold;
            _elixir -= prefab.Item2.requiredElixir;
            _darkElixir -= prefab.Item2.requiredDarkElixir;
            _gems -= prefab.Item2.requiredGems;

            SetStatus(false);
            UI_Main.instanse.SetStatus(true);

            Building building = Instantiate(prefab.Item1, Vector3.zero, Quaternion.identity);
            building.rows = data.rows;
            building.columns = data.columns;
            building.serverIndex = 0;
            building.data = data;
            building.databaseID = 0;
            building.PlacedOnGrid(x, y);
            if (building._baseArea != null)
                building._baseArea.gameObject.SetActive(true);

            Building.buildInstanse = building;
            if (CameraController.instanse != null)
                CameraController.instanse.isPlacingBuilding = true;
            if (UI_Build.instanse != null)
                UI_Build.instanse.SetStatus(true);
            return true;
        }

        private void CloseShop()
        {
            if (SoundManager.instanse != null)
                SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            SetStatus(false);
            if (UI_Main.instanse != null)
                UI_Main.instanse.SetStatus(true);
        }
    }
}