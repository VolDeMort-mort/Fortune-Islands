using System.Collections.Generic;
using UnityEngine;

using FortuneIslands.Core;
using FortuneIslands.CameraControl;
using FortuneIslands.Game.Phases;
using FortuneIslands.Turns;
using FortuneIslands.UI;

namespace FortuneIslands.Game
{
    // Composition root of a match: spawns the islands, creates the TurnController and connects
    // its phases to the phase handlers and the HUD. It holds no turn rules of its own.
    public class GameManager : MonoBehaviour
    {
        public const int MaxPlayers = 10;
        private const int LocalPlayerId = 0;

        public static GameManager Instance { get; private set; }

        [Header("Assets")]
        public GameObject islandPrefab;
        public CameraController mainCamera;
        public UIController uiController;

        [Header("Players")]
        [Range(1, MaxPlayers)] public int playerCount = 2;

        [Tooltip("Distance between neighbouring islands. Should be at least the island map size.")]
        [Min(1f)] public float islandSpacing = 50f;

        [Header("Turn rules")]
        public TurnRules turnRules = new TurnRules();

        private readonly List<IslandController> _islands = new List<IslandController>();
        private readonly Dictionary<TurnPhase, PhaseHandler> _phaseHandlers = new Dictionary<TurnPhase, PhaseHandler>();
        private TurnController _turns;

        public IReadOnlyList<IslandController> AllIslands => _islands;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Log.Error($"A second {nameof(GameManager)} was found on '{name}'. It has been removed.");
                Destroy(this);
                return;
            }
            Instance = this;

            // Handlers see the live island list, so they always act on the islands of the current match.
            var context = new PhaseContext(_islands, () => _turns?.CompletePhase());
            _phaseHandlers[TurnPhase.Build] = new BuildPhase(context);
            _phaseHandlers[TurnPhase.WarPlanning] = new WarPlanningPhase(context);
            _phaseHandlers[TurnPhase.WarBattle] = new WarBattlePhase(context);
            _phaseHandlers[TurnPhase.Roll] = new RollPhase(context);
        }

        private void Update()
        {
            _turns?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            // The scene is being torn down and the islands may already be gone, so no phase handler runs here.
            DetachTurns();
            Instance = null;
        }

        public void StartSinglePlayerGame()
        {
            Log.Info("Starting Single Player...");

            ClearOldGame();

            for (int id = 0; id < playerCount; id++)
            {
                SpawnIsland(id, GetIslandPosition(id));
            }

            StartTurns();
        }

        // Islands form a near-square grid: 2 players sit side by side, 10 players make a 4x3 grid.
        private Vector3 GetIslandPosition(int index)
        {
            int columns = Mathf.CeilToInt(Mathf.Sqrt(playerCount));
            return new Vector3(index % columns, 0f, index / columns) * islandSpacing;
        }

        private void SpawnIsland(int id, Vector3 pos)
        {
            GameObject obj = Instantiate(islandPrefab, pos, Quaternion.identity);
            IslandController island = obj.GetComponent<IslandController>();

            island.isLocalPlayer = id == LocalPlayerId;
            island.Initialize(id);
            _islands.Add(island);

            if (island.isLocalPlayer)
            {
                if (mainCamera != null) mainCamera.FocusOnTarget(obj.transform.position);
                if (uiController != null) uiController.Initialize(island.resourceManager, island.buildManager);
            }
        }

        private void StartTurns()
        {
            var playerIds = new List<int>(_islands.Count);
            foreach (var island in _islands) playerIds.Add(island.PlayerID);

            _turns = new TurnController(turnRules, playerIds);
            _turns.PhaseStarted += HandlePhaseStarted;
            _turns.PhaseEnded += HandlePhaseEnded;

            if (uiController != null) uiController.BindTurns(_turns, LocalPlayerId);

            _turns.Start();
        }

        // Stop() fires PhaseEnded, so the current phase handler switches its systems off
        // before the islands are destroyed.
        private void StopTurns()
        {
            if (_turns == null) return;

            _turns.Stop();
            DetachTurns();
        }

        private void DetachTurns()
        {
            if (_turns == null) return;

            _turns.PhaseStarted -= HandlePhaseStarted;
            _turns.PhaseEnded -= HandlePhaseEnded;
            _turns = null;
        }

        private void HandlePhaseStarted(TurnPhase phase)
        {
            _phaseHandlers[phase].Enter();

            // Only the local player can press Ready for now. Every other player is ready at once
            // so they never hold up a phase; an AI or a network player will take this over.
            foreach (var island in _islands)
            {
                if (!island.isLocalPlayer) _turns.SetReady(island.PlayerID, true);
            }
        }

        private void HandlePhaseEnded(TurnPhase phase)
        {
            _phaseHandlers[phase].Exit();
        }

        private void ClearOldGame()
        {
            StopTurns();

            foreach (var island in _islands)
            {
                if (island != null) Destroy(island.gameObject);
            }
            _islands.Clear();
        }
    }
}
