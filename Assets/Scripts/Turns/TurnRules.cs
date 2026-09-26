using System;

namespace FortuneIslands.Turns
{
    // Tunable rules of the turn cycle, edited in the inspector.
    // A time limit of 0 means the phase has no timer and waits until every player is ready.
    [Serializable]
    public class TurnRules
    {
        // War happens on rounds 3, 6, 9... when this is 3.
        public int warEveryNRounds = 3;

        public float buildTimeLimit = 0f;
        public float warPlanningTimeLimit = 60f;

        // Time to look at the dice results before the next round starts.
        public float rollTimeLimit = 5f;

        public void Validate()
        {
            if (warEveryNRounds < 1)
                throw new ArgumentOutOfRangeException(nameof(warEveryNRounds), warEveryNRounds, "War must happen at most once per round (use 1 or more).");

            ThrowIfNegative(buildTimeLimit, nameof(buildTimeLimit));
            ThrowIfNegative(warPlanningTimeLimit, nameof(warPlanningTimeLimit));
            ThrowIfNegative(rollTimeLimit, nameof(rollTimeLimit));
        }

        private static void ThrowIfNegative(float value, string name)
        {
            if (value < 0f || float.IsNaN(value))
                throw new ArgumentOutOfRangeException(name, value, "Time limit cannot be negative (use 0 for no limit).");
        }
    }
}
