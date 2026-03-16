using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevelopersHub.ClashOfWhatecer
{
    public class UI_Search : MonoBehaviour
    {
        [SerializeField] private GameObject _elements = null;
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _findButton = null;
        [SerializeField] private TextMeshProUGUI _costText = null;

        [Header("Battle Test")]
        [SerializeField] private string battleSceneName = "BattleTest";
        [SerializeField] private string buttonLabel = "Start Battle";
        [SerializeField] private string infoLabel = "Lancer une bataille de test";

        private static UI_Search _instance = null;
        public static UI_Search instanse { get { return _instance; } }

        private bool _active = false;
        public bool isActive { get { return _active; } }

        private void Awake()
        {
            _instance = this;

            if (_elements != null)
                _elements.SetActive(false);
        }

        private void Start()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_findButton != null)
                _findButton.onClick.AddListener(StartBattleScene);
        }

        public void SetStatus(bool status)
        {
            _active = status;

            if (_elements != null)
                _elements.SetActive(status);

            if (status)
                RefreshView();
        }

        private void RefreshView()
        {
            if (_costText != null)
            {
                _costText.text = infoLabel;
                _costText.color = Color.white;
                _costText.ForceMeshUpdate(true);
            }

            if (_findButton != null)
            {
                TextMeshProUGUI buttonText = _findButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = buttonLabel;
                    buttonText.ForceMeshUpdate(true);
                }

                _findButton.interactable = true;
            }
        }

        private void Close()
        {
            SetStatus(false);

            if (UI_Main.instanse != null)
                UI_Main.instanse.SetStatus(true);
        }

        private void StartBattleScene()
        {
            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Single);
        }
    }
}