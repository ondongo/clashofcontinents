using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClashOfContinents;

public class BattleTestManager : MonoBehaviour
{
    [System.Serializable]
    public class TroopEntry
    {
        public string troopName;
        public GameObject prefab;
        public int maxCount = 5;
    }

    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        public int count = 5;
    }

    [Header("References")]
    [SerializeField] private Camera battleCamera;

    [Header("Player Troops")]
    [SerializeField] private List<TroopEntry> playerTroops = new List<TroopEntry>();

    [Header("Enemy Troops")]
    [SerializeField] private List<EnemyEntry> enemyTroops = new List<EnemyEntry>();

    [Header("Spawn Zones")]
    [SerializeField] private Transform playerSpawnOrigin;
    [SerializeField] private Transform enemySpawnOrigin;

    [Header("Placement")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastDistance = 1000f;
    [SerializeField] private float placementSpacingCheckRadius = 0.8f;

    [Header("Enemy Formation")]
    [SerializeField] private float enemySpacing = 2f;
    [SerializeField] private int enemyColumns = 3;

    [Header("Scene")]
    [SerializeField] private string returnSceneName = "Game";

    [Header("Battle Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button returnButton;

    private int selectedTroopIndex = 0;
    private Dictionary<int, int> deployedCounts = new Dictionary<int, int>();

    private bool battleEnded = false;
    private bool battleStarted = false;
    private int totalPlayerTroopsPlaced = 0;

    private void Awake()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        for (int i = 0; i < playerTroops.Count; i++)
            deployedCounts[i] = 0;

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void Start()
    {
        if (battleCamera == null)
            Debug.LogError("BattleTestManager: aucune caméra trouvée.");

        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToMainScene);

        SpawnEnemyArmy();
        battleStarted = true;

        if (SoundManager.instanse != null && SoundManager.instanse.battleMusic != null)
            SoundManager.instanse.PlayMusic(SoundManager.instanse.battleMusic);

        LogCurrentSelection();
    }

    private void Update()
    {
        HandleSelectionHotkeys();
        HandlePlacement();
        HandleReturn();
        CheckBattleEnd();
    }

    private void HandleSelectionHotkeys()
    {
        if (battleEnded) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectTroop(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectTroop(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectTroop(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectTroop(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectTroop(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectTroop(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SelectTroop(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectTroop(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) SelectTroop(8);
    }

    private void HandlePlacement()
    {
        if (battleEnded) return;

        if (Mouse.current == null || battleCamera == null) return;
        if (playerTroops == null || playerTroops.Count == 0) return;
        if (selectedTroopIndex < 0 || selectedTroopIndex >= playerTroops.Count) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!TryGetGroundPoint(Mouse.current.position.ReadValue(), out Vector3 hitPoint))
            return;

        TryPlacePlayerTroop(hitPoint);
    }

    public void SelectTroop(int index)
    {
        if (index < 0 || index >= playerTroops.Count) return;
        selectedTroopIndex = index;
        LogCurrentSelection();
    }

    private void LogCurrentSelection()
    {
        if (playerTroops == null || playerTroops.Count == 0) return;

        TroopEntry troop = playerTroops[selectedTroopIndex];
        int used = deployedCounts[selectedTroopIndex];
        Debug.Log($"Troupe sélectionnée: [{selectedTroopIndex + 1}] {troop.troopName} ({used}/{troop.maxCount})");
    }

    private void TryPlacePlayerTroop(Vector3 position)
    {
        TroopEntry troop = playerTroops[selectedTroopIndex];

        if (troop.prefab == null)
            return;

        if (deployedCounts[selectedTroopIndex] >= troop.maxCount)
            return;

        if (!IsInsidePlayerDeploymentZone(position))
            return;

        if (IsTooCloseToAnotherBot(position))
            return;

        GameObject go = Instantiate(troop.prefab, position, Quaternion.identity);

        BattleBot bot = go.GetComponent<BattleBot>();
        if (bot == null)
            bot = go.AddComponent<BattleBot>();

        bot.team = 0;

        deployedCounts[selectedTroopIndex]++;
        totalPlayerTroopsPlaced++; // AJOUT IMPORTANT

        if (SoundManager.instanse != null && SoundManager.instanse.placeUnitSound != null)
            SoundManager.instanse.PlaySound(SoundManager.instanse.placeUnitSound);

        LogCurrentSelection();
    }

    private bool TryGetGroundPoint(Vector2 screenPos, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Ray ray = battleCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            return true;
        }

        return false;
    }

    private bool IsInsidePlayerDeploymentZone(Vector3 position)
    {
        return true;
    }

    private bool IsTooCloseToAnotherBot(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, placementSpacingCheckRadius);

        foreach (Collider hit in hits)
        {
            if (hit.GetComponentInParent<BattleBot>() != null)
                return true;
        }

        return false;
    }

    private void SpawnEnemyArmy()
    {
        if (enemySpawnOrigin == null)
        {
            Debug.LogWarning("enemySpawnOrigin non assigné.");
            return;
        }

        int spawned = 0;

        foreach (EnemyEntry entry in enemyTroops)
        {
            if (entry == null || entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                int row = spawned / enemyColumns;
                int col = spawned % enemyColumns;

                Vector3 pos = enemySpawnOrigin.position + new Vector3(col * enemySpacing, 0f, row * enemySpacing);

                GameObject go = Instantiate(entry.prefab, pos, Quaternion.identity);

                BattleBot bot = go.GetComponent<BattleBot>();
                if (bot == null)
                    bot = go.AddComponent<BattleBot>();

                bot.team = 1;
                spawned++;
            }
        }
    }

    private void CheckBattleEnd()
    {
        if (!battleStarted || battleEnded)
            return;

        BattleBot[] allBots = FindObjectsOfType<BattleBot>();
        int playerCount = 0;
        int enemyCount = 0;

        foreach (BattleBot bot in allBots)
        {
            if (bot == null) continue;

            if (bot.team == 0) playerCount++;
            else if (bot.team == 1) enemyCount++;
        }

        // Victoire : si plus aucun ennemi
        if (enemyCount <= 0)
        {
            ShowVictory();
            return;
        }

        // Défaite : seulement si le joueur a déjà commencé à poser des troupes
        if (totalPlayerTroopsPlaced > 0 && playerCount <= 0)
        {
            ShowDefeat();
        }
    }

    private void ShowVictory()
    {
        battleEnded = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "Bravo, vous avez gagné !";

        if (SoundManager.instanse != null)
        {
            SoundManager.instanse.StopMusic();

            if (SoundManager.instanse.victoryMusic != null)
                SoundManager.instanse.PlayMusic(SoundManager.instanse.victoryMusic);

            if (SoundManager.instanse.victorySound != null)
                SoundManager.instanse.PlaySound(SoundManager.instanse.victorySound);
        }

        Debug.Log("Victoire joueur.");
    }

    private void ShowDefeat()
    {
        battleEnded = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "Défaite...";

        Debug.Log("Défaite joueur.");
    }

    private void HandleReturn()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToMainScene();
    }

    private void ReturnToMainScene()
    {
        SceneManager.LoadScene(returnSceneName, LoadSceneMode.Single);
    }
}