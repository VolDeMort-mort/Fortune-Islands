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
        private const int LocalPlayerId = 0;

        public static GameManager Instance { get; private set; }

        [Header("Assets")]
        public GameObject islandPrefab;
        public CameraController mainCamera;
        public UIController uiController;

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

            SpawnIsland(0, Vector3.zero);
            SpawnIsland(1, new Vector3(50, 0, 0));

            StartTurns();
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
