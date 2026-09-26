using System;
using System.Collections.Generic;

namespace FortuneIslands.Turns
{
    // Owns the turn cycle: the current round and phase, the phase timer and who is ready.
    //
    // It is plain C# with no Unity types: the game drives it by calling Tick() every frame.
    // That makes it testable without a scene and lets the same code run on a server later.
    //
    // Phase changes happen only inside Tick(). SetReady() and CompletePhase() just record a request,
    // so an event handler can never trigger a phase change in the middle of another one.
    public sealed class TurnController
    {
        // Round number, fired before PhaseStarted(Build) of that round.
        public event Action<int> RoundStarted;
        public event Action<TurnPhase> PhaseStarted;
        public event Action<TurnPhase> PhaseEnded;

        // Player id and the new ready state. Ready flags are reset silently when a phase starts.
        public event Action<int, bool> ReadyChanged;

        private readonly int _warEveryNRounds;
        private readonly float _buildTimeLimit;
        private readonly float _warPlanningTimeLimit;
        private readonly float _rollTimeLimit;

        private readonly HashSet<int> _players = new HashSet<int>();
        private readonly HashSet<int> _readyPlayers = new HashSet<int>();

        private float _elapsed;
        private bool _completeRequested;

        public TurnController(TurnRules rules, IEnumerable<int> playerIds)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (playerIds == null) throw new ArgumentNullException(nameof(playerIds));

            rules.Validate();

            // Copied so that editing the inspector during play cannot break a running cycle.
            _warEveryNRounds = rules.warEveryNRounds;
            _buildTimeLimit = rules.buildTimeLimit;
            _warPlanningTimeLimit = rules.warPlanningTimeLimit;
            _rollTimeLimit = rules.rollTimeLimit;

            foreach (int id in playerIds)
            {
                if (!_players.Add(id))
                    throw new ArgumentException($"Player {id} is listed more than once.", nameof(playerIds));
            }

            if (_players.Count == 0)
                throw new ArgumentException("At least one player is required.", nameof(playerIds));
        }

        public bool IsRunning { get; private set; }
        public int Round { get; private set; }
        public TurnPhase Phase { get; private set; }

        public int PlayerCount => _players.Count;
        public int ReadyCount => _readyPlayers.Count;

        // 0 when the current phase has no timer.
        public float TimeLimit => GetTimeLimit(Phase);
        public float TimeRemaining => TimeLimit > 0f ? Math.Max(0f, TimeLimit - _elapsed) : 0f;

        // False for phases that only the game can finish (the battle), so the UI can disable "Ready".
        public bool CanEndByReady => EndsWhenAllReady(Phase);

        public bool IsWarRound(int round) => round % _warEveryNRounds == 0;

        public void Start()
        {
            if (IsRunning) throw new InvalidOperationException("The turn cycle is already running.");

            IsRunning = true;
            StartRound(1);
        }

        // Ends the current phase (PhaseEnded fires) and stops the cycle. Further ticks do nothing.
        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            PhaseEnded?.Invoke(Phase);
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            if (deltaTime > 0f) _elapsed += deltaTime;

            if (ShouldEndPhase()) Advance();
        }

        // Returns false if the cycle is not running or the player is unknown.
        public bool SetReady(int playerId, bool ready)
        {
            if (!IsRunning || !_players.Contains(playerId)) return false;

            bool changed = ready ? _readyPlayers.Add(playerId) : _readyPlayers.Remove(playerId);
            if (changed) ReadyChanged?.Invoke(playerId, ready);
            return true;
        }

        public bool IsReady(int playerId) => _readyPlayers.Contains(playerId);

        // Called by game logic when the phase has done its job, for example when the battle is resolved.
        // The phase ends on the next Tick(). Safe to call from a PhaseStarted handler.
        public void CompletePhase()
        {
            if (IsRunning) _completeRequested = true;
        }

        private bool ShouldEndPhase()
        {
            if (_completeRequested) return true;
            if (EndsWhenAllReady(Phase) && _readyPlayers.Count == _players.Count) return true;

            float limit = TimeLimit;
            return limit > 0f && _elapsed >= limit;
        }

        private void Advance()
        {
            PhaseEnded?.Invoke(Phase);
            if (!IsRunning) return; // a PhaseEnded handler stopped the cycle

            switch (Phase)
            {
                case TurnPhase.Build:
                    EnterPhase(IsWarRound(Round) ? TurnPhase.WarPlanning : TurnPhase.Roll);
                    break;
                case TurnPhase.WarPlanning:
                    EnterPhase(TurnPhase.WarBattle);
                    break;
                case TurnPhase.WarBattle:
                    EnterPhase(TurnPhase.Roll);
                    break;
                case TurnPhase.Roll:
                    StartRound(Round + 1);
                    break;
            }
        }

        private void StartRound(int round)
        {
            Round = round;
            ResetPhaseState(TurnPhase.Build);

            RoundStarted?.Invoke(round);
            if (IsRunning) PhaseStarted?.Invoke(Phase);
        }

        private void EnterPhase(TurnPhase phase)
        {
            ResetPhaseState(phase);
            PhaseStarted?.Invoke(phase);
        }

        // Runs before PhaseStarted fires, so a handler may already mark players ready or complete the phase.
        private void ResetPhaseState(TurnPhase phase)
        {
            Phase = phase;
            _elapsed = 0f;
            _completeRequested = false;
            _readyPlayers.Clear();
        }

        private float GetTimeLimit(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Build: return _buildTimeLimit;
                case TurnPhase.WarPlanning: return _warPlanningTimeLimit;
                case TurnPhase.Roll: return _rollTimeLimit;
                default: return 0f; // the battle lasts until it is resolved
            }
        }

        // Players cannot skip the battle by pressing "Ready": it has to play out.
        private static bool EndsWhenAllReady(TurnPhase phase) => phase != TurnPhase.WarBattle;
    }
}
