using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace FortuneIslands.Turns.Tests
{
    public class TurnControllerTests
    {
        private static readonly int[] TwoPlayers = { 0, 1 };

        private static TurnRules Rules(int warEvery = 3, float build = 0f, float planning = 60f, float roll = 0f)
        {
            return new TurnRules
            {
                warEveryNRounds = warEvery,
                buildTimeLimit = build,
                warPlanningTimeLimit = planning,
                rollTimeLimit = roll
            };
        }

        private static TurnController Started(TurnRules rules, params int[] players)
        {
            var turns = new TurnController(rules, players);
            turns.Start();
            return turns;
        }

        private static void MarkReady(TurnController turns, IEnumerable<int> players)
        {
            foreach (int id in players) turns.SetReady(id, true);
        }

        // Ends the current phase the way players or the game would.
        private static void FinishPhase(TurnController turns, IEnumerable<int> players)
        {
            if (turns.Phase == TurnPhase.WarBattle) turns.CompletePhase();
            else MarkReady(turns, players);

            turns.Tick(0f);
        }

        [Test]
        public void Start_BeginsRoundOneWithBuild()
        {
            var turns = Started(Rules(), TwoPlayers);

            Assert.That(turns.IsRunning, Is.True);
            Assert.That(turns.Round, Is.EqualTo(1));
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
        }

        [Test]
        public void Start_RaisesRoundStartedBeforePhaseStarted()
        {
            var turns = new TurnController(Rules(), TwoPlayers);
            var log = new List<string>();
            turns.RoundStarted += round => log.Add($"round {round}");
            turns.PhaseStarted += phase => log.Add($"phase {phase} (round {turns.Round})");

            turns.Start();

            Assert.That(log, Is.EqualTo(new[] { "round 1", "phase Build (round 1)" }));
        }

        [Test]
        public void Start_Twice_Throws()
        {
            var turns = Started(Rules(), TwoPlayers);

            Assert.Throws<InvalidOperationException>(turns.Start);
        }

        [Test]
        public void Build_EndsWhenAllPlayersAreReady()
        {
            var turns = Started(Rules(), TwoPlayers);

            MarkReady(turns, TwoPlayers);
            turns.Tick(0f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void Build_WaitsWhileSomePlayerIsNotReady()
        {
            var turns = Started(Rules(), TwoPlayers);

            turns.SetReady(0, true);
            turns.Tick(1000f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
        }

        [Test]
        public void Ready_CanBeWithdrawn()
        {
            var turns = Started(Rules(), TwoPlayers);

            MarkReady(turns, TwoPlayers);
            turns.SetReady(1, false);
            turns.Tick(0f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
            Assert.That(turns.ReadyCount, Is.EqualTo(1));
        }

        [Test]
        public void Ready_IsResetWhenThePhaseChanges()
        {
            var turns = Started(Rules(), TwoPlayers);

            FinishPhase(turns, TwoPlayers);

            Assert.That(turns.ReadyCount, Is.Zero);
            Assert.That(turns.IsReady(0), Is.False);
        }

        [Test]
        public void ReadyChanged_FiresOnlyOnActualChange()
        {
            var turns = Started(Rules(), TwoPlayers);
            int calls = 0;
            turns.ReadyChanged += (id, ready) => calls++;

            turns.SetReady(0, true);
            turns.SetReady(0, true);
            turns.SetReady(0, false);

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void SetReady_ForUnknownPlayer_ReturnsFalse()
        {
            var turns = Started(Rules(), TwoPlayers);

            Assert.That(turns.SetReady(7, true), Is.False);
            Assert.That(turns.ReadyCount, Is.Zero);
        }

        [Test]
        public void SetReady_BeforeStart_ReturnsFalse()
        {
            var turns = new TurnController(Rules(), TwoPlayers);

            Assert.That(turns.SetReady(0, true), Is.False);
        }

        [Test]
        public void WarHappensEveryThirdRound()
        {
            var turns = new TurnController(Rules(warEvery: 3), TwoPlayers);
            var phases = new List<string>();
            turns.PhaseStarted += phase => phases.Add($"{turns.Round}:{phase}");
            turns.Start();

            while (turns.Round < 4) FinishPhase(turns, TwoPlayers);

            Assert.That(phases, Is.EqualTo(new[]
            {
                "1:Build", "1:Roll",
                "2:Build", "2:Roll",
                "3:Build", "3:WarPlanning", "3:WarBattle", "3:Roll",
                "4:Build"
            }));
        }

        [TestCase(1, 1, true)]
        [TestCase(3, 2, false)]
        [TestCase(3, 3, true)]
        [TestCase(3, 6, true)]
        [TestCase(3, 7, false)]
        public void IsWarRound_FollowsTheInterval(int warEvery, int round, bool expected)
        {
            var turns = new TurnController(Rules(warEvery: warEvery), TwoPlayers);

            Assert.That(turns.IsWarRound(round), Is.EqualTo(expected));
        }

        [Test]
        public void WarPlanning_EndsWhenTimeRunsOut()
        {
            var turns = Started(Rules(warEvery: 1, planning: 60f), TwoPlayers);
            FinishPhase(turns, TwoPlayers); // Build -> WarPlanning

            turns.Tick(59.9f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.WarPlanning));

            turns.Tick(0.1f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.WarBattle));
        }

        [Test]
        public void WarPlanning_EndsEarlyWhenAllPlayersAreReady()
        {
            var turns = Started(Rules(warEvery: 1, planning: 60f), TwoPlayers);
            FinishPhase(turns, TwoPlayers); // Build -> WarPlanning

            turns.Tick(5f);
            FinishPhase(turns, TwoPlayers);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.WarBattle));
        }

        [Test]
        public void TimeRemaining_CountsDownAndResetsWithTheNextPhase()
        {
            var turns = Started(Rules(warEvery: 1, planning: 60f, roll: 5f), TwoPlayers);
            FinishPhase(turns, TwoPlayers); // Build -> WarPlanning

            turns.Tick(20f);
            Assert.That(turns.TimeLimit, Is.EqualTo(60f));
            Assert.That(turns.TimeRemaining, Is.EqualTo(40f).Within(0.001f));

            turns.Tick(40f); // -> WarBattle, no timer
            Assert.That(turns.TimeLimit, Is.Zero);
            Assert.That(turns.TimeRemaining, Is.Zero);
        }

        [Test]
        public void PhaseWithoutTimeLimit_NeverEndsByTime()
        {
            var turns = Started(Rules(build: 0f), TwoPlayers);

            for (int i = 0; i < 100; i++) turns.Tick(3600f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
        }

        [Test]
        public void Build_WithTimeLimit_EndsWhenTimeRunsOut()
        {
            var turns = Started(Rules(build: 30f), TwoPlayers);

            turns.Tick(30f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void WarBattle_IgnoresReadyAndEndsOnlyWhenCompleted()
        {
            var turns = Started(Rules(warEvery: 1), TwoPlayers);
            FinishPhase(turns, TwoPlayers); // Build -> WarPlanning
            FinishPhase(turns, TwoPlayers); // WarPlanning -> WarBattle

            MarkReady(turns, TwoPlayers);
            turns.Tick(3600f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.WarBattle));
            Assert.That(turns.CanEndByReady, Is.False);

            turns.CompletePhase();
            turns.Tick(0f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void CompletePhase_CalledWhileThePhaseStarts_IsNotLost()
        {
            var turns = new TurnController(Rules(warEvery: 1), TwoPlayers);
            turns.PhaseStarted += phase =>
            {
                if (phase == TurnPhase.WarBattle) turns.CompletePhase();
            };
            turns.Start();
            FinishPhase(turns, TwoPlayers); // Build -> WarPlanning
            MarkReady(turns, TwoPlayers);
            turns.Tick(0f);                 // WarPlanning -> WarBattle, completed by the handler

            turns.Tick(0f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void ReadyMarkedWhileThePhaseStarts_IsKept()
        {
            var turns = new TurnController(Rules(), TwoPlayers);
            turns.PhaseStarted += phase => turns.SetReady(1, true); // a bot that is always ready
            turns.Start();

            Assert.That(turns.IsReady(1), Is.True);

            turns.SetReady(0, true);
            turns.Tick(0f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void OnlyOnePhaseChangePerTick()
        {
            var turns = Started(Rules(), TwoPlayers);
            turns.PhaseStarted += phase => MarkReady(turns, TwoPlayers);

            MarkReady(turns, TwoPlayers);
            turns.Tick(0f);

            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void Roll_StartsTheNextRound()
        {
            var turns = Started(Rules(), TwoPlayers);
            int startedRound = 0;
            turns.RoundStarted += round => startedRound = round;

            FinishPhase(turns, TwoPlayers); // Build -> Roll
            FinishPhase(turns, TwoPlayers); // Roll -> Build of round 2

            Assert.That(turns.Round, Is.EqualTo(2));
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
            Assert.That(startedRound, Is.EqualTo(2));
        }

        [Test]
        public void PhaseEnded_FiresForThePhaseThatIsEnding()
        {
            var turns = Started(Rules(), TwoPlayers);
            var ended = new List<TurnPhase>();
            turns.PhaseEnded += ended.Add;

            FinishPhase(turns, TwoPlayers);

            Assert.That(ended, Is.EqualTo(new[] { TurnPhase.Build }));
        }

        [Test]
        public void TenPlayers_PhaseWaitsForEveryone()
        {
            var players = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var turns = Started(Rules(), players);

            for (int id = 0; id < 9; id++) turns.SetReady(id, true);
            turns.Tick(0f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));

            turns.SetReady(9, true);
            turns.Tick(0f);
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Roll));
        }

        [Test]
        public void Stop_EndsTheCurrentPhaseAndIgnoresFurtherTicks()
        {
            var turns = Started(Rules(build: 10f), TwoPlayers);
            var ended = new List<TurnPhase>();
            turns.PhaseEnded += ended.Add;

            turns.Stop();
            turns.Tick(100f);

            Assert.That(turns.IsRunning, Is.False);
            Assert.That(ended, Is.EqualTo(new[] { TurnPhase.Build }));
            Assert.That(turns.Phase, Is.EqualTo(TurnPhase.Build));
        }

        [Test]
        public void Constructor_RejectsInvalidRules()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TurnController(Rules(warEvery: 0), TwoPlayers));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TurnController(Rules(planning: -1f), TwoPlayers));
        }

        [Test]
        public void Constructor_RejectsMissingOrDuplicatePlayers()
        {
            Assert.Throws<ArgumentException>(() => new TurnController(Rules(), new int[0]));
            Assert.Throws<ArgumentException>(() => new TurnController(Rules(), new[] { 1, 1 }));
        }

        [Test]
        public void RulesEditedAfterConstruction_DoNotAffectARunningCycle()
        {
            var rules = Rules(warEvery: 3);
            var turns = Started(rules, TwoPlayers);

            rules.warEveryNRounds = 0;

            Assert.That(turns.IsWarRound(3), Is.True);
        }
    }
}
