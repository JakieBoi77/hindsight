using System;

namespace EyeCanDo.Core.Games
{
    /// <summary>Tuning for one Drop Catch level.</summary>
    public sealed class DropCatchSettings
    {
        public int Level { get; private set; }

        public float RoundSeconds { get; private set; }

        /// <summary>Seconds between drops from one bottle.</summary>
        public float DropInterval { get; private set; }

        /// <summary>Seconds a drop takes to fall from a bottle to the patient's eyes.</summary>
        public float FallSeconds { get; private set; }

        /// <summary>How fast the bottles glide, in stage widths per second.</summary>
        public float BottleSpeed { get; private set; }

        public int Bottles { get; private set; }

        /// <summary>Chance (0..1) that a drop is a golden drop worth more points.</summary>
        public float GoldenChance { get; private set; }

        /// <summary>Catches that fill the eyes, after which blinking earns a bonus.</summary>
        public int CatchesUntilBlink { get; private set; }

        /// <summary>How far (reference pixels) a drop may land from the eyes and still count.</summary>
        public float CatchRadius { get; private set; }

        public static DropCatchSettings ForLevel(int level)
        {
            switch (Levels.Clamp(level))
            {
                case 1:
                    return new DropCatchSettings { Level = 1, RoundSeconds = 30f, DropInterval = 1.6f, FallSeconds = 2.3f, BottleSpeed = 0.22f, Bottles = 1, GoldenChance = 0.12f, CatchesUntilBlink = 5, CatchRadius = 90f };
                case 2:
                    return new DropCatchSettings { Level = 2, RoundSeconds = 35f, DropInterval = 1.1f, FallSeconds = 1.8f, BottleSpeed = 0.32f, Bottles = 1, GoldenChance = 0.15f, CatchesUntilBlink = 5, CatchRadius = 75f };
                default:
                    return new DropCatchSettings { Level = 3, RoundSeconds = 40f, DropInterval = 1.25f, FallSeconds = 1.45f, BottleSpeed = 0.36f, Bottles = 2, GoldenChance = 0.2f, CatchesUntilBlink = 4, CatchRadius = 62f };
            }
        }
    }

    /// <summary>
    /// Rules of Drop Catch: the patient leans back in the exam chair with eyes open, and the child
    /// slides the chair to catch drops from friendly drop bottles. Catching rehearses head back and
    /// eyes open; each catch feels cold (a shiver) and is fine; when the eyes fill up, blinking
    /// spreads the drops and earns a bonus. A missed drop only splashes; nothing is taken away.
    /// The runtime moves the drops and reports catches and misses.
    /// </summary>
    public sealed class DropCatchRound
    {
        public const int PointsPerDrop = 1;
        public const int PointsPerGolden = 3;
        public const int BlinkBonus = 3;
        public const int BlinkTaps = 2;

        private readonly Random random;
        private readonly float[] nextDrop;
        private int blinkTaps;

        public DropCatchRound(DropCatchSettings settings, int seed = 0)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            random = new Random(seed);
            nextDrop = new float[settings.Bottles];
            for (var i = 0; i < nextDrop.Length; i++)
            {
                // The first drop comes quickly; a second bottle starts half a beat later.
                nextDrop[i] = 0.8f + i * settings.DropInterval * 0.5f;
            }
        }

        /// <summary>Raised when a bottle lets go of a drop: bottle index, and whether it is golden.</summary>
        public event Action<int, bool> DropReleased;

        public DropCatchSettings Settings { get; }

        public float Elapsed { get; private set; }

        public float TimeLeft => Math.Max(0f, Settings.RoundSeconds - Elapsed);

        /// <summary>True once time is up. Drops already falling still count.</summary>
        public bool TimeUp => Elapsed >= Settings.RoundSeconds;

        public int Released { get; private set; }

        public int Caught { get; private set; }

        public int GoldenCaught { get; private set; }

        public int Missed { get; private set; }

        public int Blinks { get; private set; }

        public int Score { get; private set; }

        public int Combo { get; private set; }

        public int BestCombo { get; private set; }

        public int CatchesSinceBlink { get; private set; }

        /// <summary>The eyes are full of drops: blinking now spreads them and earns a bonus.</summary>
        public bool EyesFull => CatchesSinceBlink >= Settings.CatchesUntilBlink;

        /// <summary>Share of released drops that were caught, 0..1.</summary>
        public double CatchRatio => Released == 0 ? 0.0 : (double)Caught / Released;

        /// <summary>
        /// Three stars only for a perfect round (every drop caught); two for catching at least
        /// three in four; one for finishing.
        /// </summary>
        public int Stars => Released > 0 && Missed == 0 && Caught >= Released ? 3 : CatchRatio >= 0.75 ? 2 : 1;

        /// <summary>Advances the clock and releases drops that are due.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || TimeUp)
            {
                return;
            }

            Elapsed += deltaSeconds;
            if (TimeUp)
            {
                return;
            }

            for (var i = 0; i < nextDrop.Length; i++)
            {
                nextDrop[i] -= deltaSeconds;
                if (nextDrop[i] > 0f)
                {
                    continue;
                }

                // A little randomness in the beat keeps the rhythm lively but fair.
                var jitter = (float)(random.NextDouble() * 0.4 - 0.2) * Settings.DropInterval;
                nextDrop[i] += Settings.DropInterval * Settings.Bottles + jitter;
                var golden = random.NextDouble() < Settings.GoldenChance;
                Released++;
                DropReleased?.Invoke(i, golden);
            }
        }

        /// <summary>A drop landed in the patient's eyes. Returns the points it earned.</summary>
        public int Catch(bool golden)
        {
            Caught++;
            Combo++;
            BestCombo = Math.Max(BestCombo, Combo);
            CatchesSinceBlink++;
            var points = golden ? PointsPerGolden : PointsPerDrop;
            if (golden)
            {
                GoldenCaught++;
            }

            Score += points;
            return points;
        }

        /// <summary>A drop missed and splashed. Only the combo resets; nothing is taken away.</summary>
        public void Miss()
        {
            Missed++;
            Combo = 0;
        }

        /// <summary>
        /// The child tapped the eyes to blink. Two taps while the eyes are full spread the drops
        /// and earn a bonus. Returns the bonus when that happens, otherwise 0.
        /// </summary>
        public int Blink()
        {
            if (!EyesFull)
            {
                return 0;
            }

            blinkTaps++;
            if (blinkTaps < BlinkTaps)
            {
                return 0;
            }

            blinkTaps = 0;
            CatchesSinceBlink = 0;
            Blinks++;
            Score += BlinkBonus;
            return BlinkBonus;
        }
    }
}
