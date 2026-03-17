namespace ClashOfContinents
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System;

    public class Player : MonoBehaviour
    {

        public Data.Player data = new Data.Player();
        private static Player _instance = null; public static Player instanse { get { return _instance; } }
        public Data.InitializationData initializationData = new Data.InitializationData();

        [Header("Ressources de départ (test / offline)")]
        public int startGold = 5000;
        public int startElixir = 5000;
        public int startDarkElixir = 500;
        public int startGems = 500;
        public int startMaxGold = 100000;
        public int startMaxElixir = 100000;
        public int startMaxDarkElixir = 10000;
        private bool _inBattle = false; public static bool inBattle { get { return instanse._inBattle; } set { instanse._inBattle = value; } }

        public Data.ServerBuilding GetServerBuilding(Data.BuildingID id, int level)
        {
            for (int i = 0; i < initializationData.serverBuildings.Count; i++)
            {
                if (initializationData.serverBuildings[i].id == id.ToString() && initializationData.serverBuildings[i].level == level)
                {
                    return initializationData.serverBuildings[i];
                }
            }
            return null;
        }

        public enum RequestsID
        {
            AUTH = 1, SYNC = 2, BUILD = 3, REPLACE = 4, COLLECT = 5, PREUPGRADE = 6, UPGRADE = 7, INSTANTBUILD = 8, TRAIN = 9, CANCELTRAIN = 10, BATTLEFIND = 11, BATTLESTART = 12, BATTLEFRAME = 13, BATTLEEND = 14, OPENCLAN = 15, GETCLANS = 16, JOINCLAN = 17, LEAVECLAN = 18, EDITCLAN = 19, CREATECLAN = 20, OPENWAR = 21, STARTWAR = 22, CANCELWAR = 23, WARSTARTED = 24, WARATTACK = 25, WARREPORTLIST = 26, WARREPORT = 27, JOINREQUESTS = 28, JOINRESPONSE = 29, GETCHATS = 30, SENDCHAT = 31, SENDCODE = 32, CONFIRMCODE = 33, EMAILCODE = 34, EMAILCONFIRM = 35, LOGOUT = 36, KICKMEMBER = 37, BREW = 38, CANCELBREW = 39, RESEARCH = 40, PROMOTEMEMBER = 41, DEMOTEMEMBER = 42, SCOUT = 43, BUYSHIELD = 44, BUYGEM = 45, BYUGOLD = 46, REPORTCHAT = 47, PLAYERSRANK = 48, BOOST = 49, BUYRESOURCE = 50, BATTLEREPORTS = 51, BATTLEREPORT = 52, RENAME = 53
        }

        public enum Panel
        {
            main = 0, clan = 1
        }

        public static readonly string username_key = "username";
        public static readonly string password_key = "password";

        private int _gold = 0; public int gold { get { return _gold; } set { _gold = value; } }
        private int _maxGold = 0; public int maxGold { get { return _maxGold; } }

        private int _elixir = 0; public int elixir { get { return _elixir; } set { _elixir = value; } }
        private int _maxElixir = 0; public int maxElixir { get { return _maxElixir; } }

        private int _darkElixir = 0; public int darkElixir { get { return _darkElixir; } set { _darkElixir = value; } }
        private int _maxDarkElixir = 0; public int maxDarkElixir { get { return _maxDarkElixir; } }

        private int _townHallLevel = 1; public int townHallLevel { get { return _townHallLevel; } }
        private int _spellFactoryLevel = 0; public int spellFactoryLevel { get { return _spellFactoryLevel; } }
        private int _darkSpellFactoryLevel = 0; public int darkSpellFactoryLevel { get { return _darkSpellFactoryLevel; } }
        private int _barracksLevel = 0; public int barracksLevel { get { return _barracksLevel; } }
        private int _darkBarracksLevel = 0; public int darkBarracksLevel { get { return _townHallLevel; } }
        private bool _callDisconnectError = true;

        private void Start()
        {
            string device = SystemInfo.deviceUniqueIdentifier;
            string password = "";
            string username = "";
            if (PlayerPrefs.HasKey(password_key))
            {
                password = PlayerPrefs.GetString(password_key);
            }
            if (PlayerPrefs.HasKey(username_key))
            {
                username = PlayerPrefs.GetString(username_key);
            }

            ApplyStartResources();
        }

        public void ApplyStartResources()
        {
            _maxGold = startMaxGold;
            _maxElixir = startMaxElixir;
            _maxDarkElixir = startMaxDarkElixir;
            _gold = Mathf.Clamp(startGold, 0, _maxGold);
            _elixir = Mathf.Clamp(startElixir, 0, _maxElixir);
            _darkElixir = Mathf.Clamp(startDarkElixir, 0, _maxDarkElixir);
            data.gems = startGems;

            if (UI_Main.instanse != null)
                UpdateResourcesUI();
        }

        private void Awake()
        {
            _instance = this;
            Application.runInBackground = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void OnDestroy()
        {

        }

        private bool connected = false;
        private float timer = 0;
        private bool updating = false;
        private float syncTime = 5;
        [HideInInspector] public DateTime lastUpdate = DateTime.Now;
        [HideInInspector] public DateTime lastUpdateSent = DateTime.Now;

        private void Update()
        {
            if (connected)
            {
                if (!_inBattle)
                {
                    if (timer <= 0)
                    {

                    }
                    else
                    {
                        timer -= Time.deltaTime;
                    }
                }
                data.nowTime = data.nowTime.AddSeconds(Time.deltaTime);
            }
        }

        public void SyncData(Data.Player player)
        {
            data = player;

            _gold = 0;
            _maxGold = 0;

            _elixir = 0;
            _maxElixir = 0;

            _darkElixir = 0;
            _maxDarkElixir = 0;

            if (player.buildings != null && player.buildings.Count > 0)
            {
                for (int i = 0; i < player.buildings.Count; i++)
                {
                    switch (player.buildings[i].id)
                    {
                        case Data.BuildingID.townhall:
                            _townHallLevel = player.buildings[i].level;
                            _maxGold += player.buildings[i].goldCapacity;
                            _gold += player.buildings[i].goldStorage;
                            _maxElixir += player.buildings[i].elixirCapacity;
                            _elixir += player.buildings[i].elixirStorage;
                            _maxDarkElixir += player.buildings[i].darkCapacity;
                            _darkElixir += player.buildings[i].darkStorage;
                            break;
                        case Data.BuildingID.goldstorage:
                            _maxGold += player.buildings[i].goldCapacity;
                            _gold += player.buildings[i].goldStorage;
                            break;
                        case Data.BuildingID.elixirstorage:
                            _maxElixir += player.buildings[i].elixirCapacity;
                            _elixir += player.buildings[i].elixirStorage;
                            break;
                        case Data.BuildingID.darkelixirstorage:
                            _maxDarkElixir += player.buildings[i].darkCapacity;
                            _darkElixir += player.buildings[i].darkStorage;
                            break;
                        case Data.BuildingID.barracks:
                            _barracksLevel = player.buildings[i].level;
                            break;
                        case Data.BuildingID.darkbarracks:
                            _darkBarracksLevel = player.buildings[i].level;
                            break;
                        case Data.BuildingID.spellfactory:
                            _spellFactoryLevel = player.buildings[i].level;
                            break;
                        case Data.BuildingID.darkspellfactory:
                            _darkSpellFactoryLevel = player.buildings[i].level;
                            break;
                    }
                }
            }

            if (_maxGold == 0 && _maxElixir == 0 && _maxDarkElixir == 0)
                ApplyStartResources();

            _gold = Mathf.Clamp(_gold, 0, _maxGold);
            _elixir = Mathf.Clamp(_elixir, 0, _maxElixir);
            _darkElixir = Mathf.Clamp(_darkElixir, 0, _maxDarkElixir);

            if (UI_Main.instanse != null)
            {
                UpdateResourcesUI();
                if (UI_Main.instanse._usernameText != null)
                    UI_Main.instanse._usernameText.text = Data.DecodeString(data.name);
                if (UI_Main.instanse._trophiesText != null)
                    UI_Main.instanse._trophiesText.text = data.trophies.ToString();
                if (UI_Main.instanse._levelText != null)
                    UI_Main.instanse._levelText.text = data.level.ToString();
                if (UI_Main.instanse._xpText != null)
                    UI_Main.instanse._xpText.text = data.xp.ToString();

                int reqXp = Data.GetNexLevelRequiredXp(data.level);
                if (UI_Main.instanse._xpBar != null)
                    UI_Main.instanse._xpBar.fillAmount = (reqXp > 0 ? ((float)data.xp / (float)reqXp) : 0);

                if (UI_Main.instanse._usernameText != null)
                    UI_Main.instanse._usernameText.ForceMeshUpdate(true);
                if (UI_Main.instanse._trophiesText != null)
                    UI_Main.instanse._trophiesText.ForceMeshUpdate(true);
                if (UI_Main.instanse._levelText != null)
                    UI_Main.instanse._levelText.ForceMeshUpdate(true);
                if (UI_Main.instanse._xpText != null)
                    UI_Main.instanse._xpText.ForceMeshUpdate(true);
            }
        }

        public void UpdateResourcesUI()
        {
            if (UI_Main.instanse == null) return;
            if (UI_Main.instanse._goldText != null)
                UI_Main.instanse._goldText.text = _gold.ToString();
            if (UI_Main.instanse._elixirText != null)
                UI_Main.instanse._elixirText.text = _elixir.ToString();
            if (UI_Main.instanse._darkText != null)
                UI_Main.instanse._darkText.text = _darkElixir.ToString();
            if (UI_Main.instanse._gemsText != null)
                UI_Main.instanse._gemsText.text = data.gems.ToString();

            if (UI_Main.instanse._goldBar != null)
                UI_Main.instanse._goldBar.fillAmount = (_maxGold > 0 ? ((float)_gold / (float)_maxGold) : 0);
            if (UI_Main.instanse._elixirBar != null)
                UI_Main.instanse._elixirBar.fillAmount = (_maxElixir > 0 ? ((float)_elixir / (float)_maxElixir) : 0);
            if (UI_Main.instanse._darkBar != null)
                UI_Main.instanse._darkBar.fillAmount = (_maxDarkElixir > 0 ? ((float)_darkElixir / (float)_maxDarkElixir) : 0);
        }

        public void RushSyncRequest()
        {
            timer = 0;
        }


        private void MessageResponded(int layoutIndex, int buttonIndex)
        {
            if (layoutIndex == 0)
            {
                RestartGame();
            }
        }

        public void AssignServerSpell(ref Data.Spell spell)
        {
            if (spell != null)
            {
                for (int i = 0; i < initializationData.serverSpells.Count; i++)
                {
                    if (initializationData.serverSpells[i].id == spell.id && initializationData.serverSpells[i].level == spell.level)
                    {
                        spell.server = initializationData.serverSpells[i];
                        break;
                    }
                }
            }
        }

        public static void RestartGame()
        {
            Time.timeScale = 1f;
            if (_instance != null)
            {

            }

            SceneManager.LoadScene(0);
        }

    }
}