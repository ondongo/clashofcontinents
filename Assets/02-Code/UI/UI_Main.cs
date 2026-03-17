namespace ClashOfContinents
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;

    public class UI_Main : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;
        [SerializeField] public TextMeshProUGUI _goldText = null;
        [SerializeField] public TextMeshProUGUI _elixirText = null;
        [SerializeField] public TextMeshProUGUI _darkText = null;
        [SerializeField] public TextMeshProUGUI _gemsText = null;
        [SerializeField] public TextMeshProUGUI _usernameText = null;
        [SerializeField] public TextMeshProUGUI _xpText = null;
        [SerializeField] public TextMeshProUGUI _trophiesText = null;
        [SerializeField] public TextMeshProUGUI _levelText = null;
        [SerializeField] public Image _goldBar = null;
        [SerializeField] public Image _elixirBar = null;
        [SerializeField] public Image _darkBar = null;
        [SerializeField] public Image _gemsBar = null;
        [SerializeField] public Image _xpBar = null;
        [SerializeField] private Button _shopButton = null;
        [SerializeField] private Button _battleButton = null;
        [SerializeField] public TextMeshProUGUI _buildersText = null;
        [SerializeField] public TextMeshProUGUI _shieldText = null;
        [SerializeField] public Building[] _buildingPrefabs = null;
        [SerializeField] public BuildGrid _grid = null;


        [Header("Buttons")]
        public Transform buttonsParent = null;
        public UI_Button buttonCollectGold = null;
        public UI_Button buttonCollectElixir = null;
        public UI_Button buttonCollectDarkElixir = null;
        public UI_Bar barBuild = null;
        private static UI_Main _instance = null; public static UI_Main instanse { get { return _instance; } }

        private bool _active = true; public bool isActive { get { return _active; } }
        private int workers = 0;
        private int busyWorkers = 0; public bool haveAvalibaleBuilder { get { return busyWorkers < workers; } }

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(true);
            _goldText.text = "";
            _elixirText.text = "";
            _darkText.text = "";
            _gemsText.text = "";
            _usernameText.text = "";
            _xpText.text = "";
            _trophiesText.text = "";
            _levelText.text = "";
            _goldBar.fillAmount = 0;
            _elixirBar.fillAmount = 0;
            _darkBar.fillAmount = 0;
            _gemsBar.fillAmount = 0;
            _xpBar.fillAmount = 0;
            _buildersText.text = "";
            _shieldText.text = "";
        }

        private void Start()
        {
            _shopButton.onClick.AddListener(ShopButtonClicked);
            _battleButton.onClick.AddListener(BattleButtonClicked);
            if (SoundManager.instanse != null)
                SoundManager.instanse.PlayMusic(SoundManager.instanse.mainMusic);
        }

        private void ShopButtonClicked()
        {
            if (SoundManager.instanse != null)
                SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            if (UI_Shop.instanse != null)
                UI_Shop.instanse.SetStatus(true);
            SetStatus(false);
        }

        private void BattleButtonClicked()
        {
            if (SoundManager.instanse != null)
                SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            UI_Search.instanse.SetStatus(true);
            SetStatus(false);
        }

        private void OnLeave()
        {
            UI_Build.instanse.Cancel();
        }

        public void SetStatus(bool status)
        {
            if (!status)
            {
                OnLeave();
            }
            else
            {
                if (SoundManager.instanse != null && SoundManager.instanse.musicSource != null && SoundManager.instanse.musicSource.clip != SoundManager.instanse.mainMusic)
                    SoundManager.instanse.PlayMusic(SoundManager.instanse.mainMusic);
                if (Player.instanse != null)
                {
                    Player.instanse.RushSyncRequest();
                    Player.instanse.UpdateResourcesUI();
                }
                else
                    RefreshResourcesFromShop();
            }
            _active = status;
            _elements.SetActive(status);
        }

        public void RefreshResourcesFromShop()
        {
            if (UI_Shop.instanse == null) return;
            if (_goldText != null) _goldText.text = UI_Shop.instanse.gold.ToString();
            if (_elixirText != null) _elixirText.text = UI_Shop.instanse.elixir.ToString();
            if (_darkText != null) _darkText.text = UI_Shop.instanse.darkElixir.ToString();
            if (_gemsText != null) _gemsText.text = UI_Shop.instanse.gems.ToString();
            if (_goldBar != null) _goldBar.fillAmount = UI_Shop.instanse.maxGold > 0 ? (float)UI_Shop.instanse.gold / UI_Shop.instanse.maxGold : 0f;
            if (_elixirBar != null) _elixirBar.fillAmount = UI_Shop.instanse.maxElixir > 0 ? (float)UI_Shop.instanse.elixir / UI_Shop.instanse.maxElixir : 0f;
            if (_darkBar != null) _darkBar.fillAmount = UI_Shop.instanse.maxDarkElixir > 0 ? (float)UI_Shop.instanse.darkElixir / UI_Shop.instanse.maxDarkElixir : 0f;
        }

        public (Building, Data.ServerBuilding) GetBuildingPrefab(Data.BuildingID id, Data.ServerBuilding overrideServer = null)
        {
            Data.ServerBuilding server = overrideServer;
            if (server == null && Player.instanse != null)
                server = Player.instanse.GetServerBuilding(id, 1);
            if (server == null && UI_Shop.instanse != null)
                server = UI_Shop.instanse.GetBuildingData(id, 1);
            if (server == null || _buildingPrefabs == null) return (null, null);
            for (int i = 0; i < _buildingPrefabs.Length; i++)
            {
                if (_buildingPrefabs[i] != null && _buildingPrefabs[i].id == id)
                    return (_buildingPrefabs[i], server);
            }
            return (null, null);
        }

        public List<Data.Building> GetLocalBuildings()
        {
            var list = new List<Data.Building>();
            if (_grid == null || _grid.buildings == null) return list;
            for (int i = 0; i < _grid.buildings.Count; i++)
            {
                if (_grid.buildings[i] != null && _grid.buildings[i].data != null)
                    list.Add(_grid.buildings[i].data);
            }
            return list;
        }


        private void Update()
        {


            _shieldText.text = "Aucun shield";


        }


        public void DataSynced()
        {
            int _workers = 0;
            int _busyWorkers = 0;
            if (Player.instanse.data.buildings != null && Player.instanse.data.buildings.Count > 0)
            {
                for (int i = 0; i < Player.instanse.data.buildings.Count; i++)
                {
                    bool first = false;
                    if (Player.instanse.data.buildings[i].isConstructing && Player.instanse.data.buildings[i].buildTime > 0)
                    {
                        _busyWorkers += 1;
                    }
                    Building building = _grid.GetBuilding(Player.instanse.data.buildings[i].databaseID);
                    if (building != null)
                    {
                        
                    }
                    else
                    {
                        building = _grid.GetBuilding(Player.instanse.data.buildings[i].id, Player.instanse.data.buildings[i].x, Player.instanse.data.buildings[i].y);
                        if(building != null)
                        {
                            _grid.RemoveUnidentifiedBuilding(building);
                            building.databaseID = Player.instanse.data.buildings[i].databaseID;
                            _grid.buildings.Add(building);
                        }
                        else
                        {
                            var prefab = GetBuildingPrefab(Player.instanse.data.buildings[i].id);
                            if (prefab.Item1)
                            {
                                building = Instantiate(prefab.Item1, Vector3.zero, Quaternion.identity);
                                building.rows = prefab.Item2.rows;
                                building.columns = prefab.Item2.columns;
                                building.databaseID = Player.instanse.data.buildings[i].databaseID;
                                building.lastChange = Player.instanse.lastUpdateSent.AddSeconds(-1);
                                first = true;
                                building.PlacedOnGrid(Player.instanse.data.buildings[i].x, Player.instanse.data.buildings[i].y);
                                if (building._baseArea)
                                {
                                    building._baseArea.gameObject.SetActive(false);
                                }
                                _grid.buildings.Add(building);
                            }
                            else
                            {
                                Debug.LogWarning("Building " + Player.instanse.data.buildings[i].id + " have no prefab.");
                                continue;
                            }
                        }
                    }
 
                    if (building.buildBar == null)
                    {
                        building.buildBar = Instantiate(barBuild, buttonsParent);
                        building.buildBar.gameObject.SetActive(false);
                    }
 
                    building.data = Player.instanse.data.buildings[i];
                    if(first)
                    {
                        building.lastChange = Player.instanse.lastUpdateSent.AddSeconds(-1);
                    }
 
                    switch (building.id)
                    {
                        case Data.BuildingID.goldmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectGold, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.elixirmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectElixir, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.darkelixirmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectDarkElixir, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.buildershut:
                            _workers += 1;
                            break;
                    }
                }
                _grid.RefreshBuildings();
            }
            if (Player.instanse.data.buildings != null)
            {
                for (int i = _grid.buildings.Count - 1; i >= 0; i--)
                {
                    bool found = false;
                    for (int j = 0; j < Player.instanse.data.buildings.Count; j++)
                    {
                        if (_grid.buildings[i].data.databaseID == Player.instanse.data.buildings[j].databaseID)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Destroy(_grid.buildings[i].gameObject);
                        _grid.buildings.RemoveAt(i);
                    }
                }
            }
            workers = _workers;
            busyWorkers = _busyWorkers;
            _buildersText.text = (_workers - _busyWorkers).ToString() + "/" + _workers.ToString();
        }
    }
}