using TMPro;
using UnityEngine;
using UnityEngine.UI;

using FortuneIslands.Turns;

namespace FortuneIslands.UI
{
    // Shows the round, the phase and the time left, and lets the local player toggle "Ready".
    // It only reads TurnController and sends SetReady(); it never changes phases itself.
    public class TurnHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text phaseText = null;
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private Button readyButton = null;

        private TMP_Text _readyLabel;
        private TurnController _turns;
        private int _localPlayerId;
        private int _shownSeconds = int.MinValue;

        // Call before TurnController.Start(): the first PhaseStarted fills in the texts.
        public void Bind(TurnController turns, int localPlayerId)
        {
            Unbind();

            _turns = turns;
            _localPlayerId = localPlayerId;

            _turns.PhaseStarted += HandlePhaseStarted;
            _turns.ReadyChanged += HandleReadyChanged;

            if (readyButton != null)
            {
                _readyLabel = readyButton.GetComponentInChildren<TMP_Text>(true);
                readyButton.onClick.AddListener(ToggleReady);
            }
        }

        private void Unbind()
        {
            if (_turns != null)
            {
                _turns.PhaseStarted -= HandlePhaseStarted;
                _turns.ReadyChanged -= HandleReadyChanged;
                _turns = null;
            }

            if (readyButton != null) readyButton.onClick.RemoveListener(ToggleReady);
        }

        private void OnDestroy() => Unbind();

        private void Update()
        {
            if (_turns == null || !_turns.IsRunning || timerText == null) return;

            // Rebuild the text only when the whole second changes, not every frame.
            int seconds = _turns.TimeLimit > 0f ? Mathf.CeilToInt(_turns.TimeRemaining) : -1;
            if (seconds == _shownSeconds) return;

            _shownSeconds = seconds;
            timerText.text = seconds < 0 ? string.Empty : $"{seconds / 60}:{seconds % 60:00}";
        }

        private void ToggleReady()
        {
            if (_turns == null) return;
            _turns.SetReady(_localPlayerId, !_turns.IsReady(_localPlayerId));
        }

        private void HandlePhaseStarted(TurnPhase phase)
        {
            if (phaseText != null) phaseText.text = $"Round {_turns.Round}: {PhaseName(phase)}";

            _shownSeconds = int.MinValue; // force the timer to redraw for the new phase
            RefreshReady();
        }

        private void HandleReadyChanged(int playerId, bool ready) => RefreshReady();

        private void RefreshReady()
        {
            if (readyButton != null) readyButton.interactable = _turns.CanEndByReady;
            if (_readyLabel == null) return;

            string action = _turns.IsReady(_localPlayerId) ? "Cancel" : "Ready";
            _readyLabel.text = $"{action} ({_turns.ReadyCount}/{_turns.PlayerCount})";
        }

        private static string PhaseName(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Build: return "Build";
                case TurnPhase.WarPlanning: return "War - planning";
                case TurnPhase.WarBattle: return "War - battle";
                case TurnPhase.Roll: return "Roll";
                default: return phase.ToString();
            }
        }
    }
}
