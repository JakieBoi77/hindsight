using System;
using System.Collections.Generic;

namespace EyeCanDo.Core.Games
{
    /// <summary>Tuning for one Blurry Spy level.</summary>
    public sealed class BlurrySpySettings
    {
        public int Level { get; private set; }

        /// <summary>How many things to find.</summary>
        public int Targets { get; private set; }

        /// <summary>How many things are in the room.</summary>
        public int Items { get; private set; }

        /// <summary>Blur at the start of the round, in reference pixels.</summary>
        public float StartBlur { get; private set; }

        /// <summary>Things drift slowly around the room.</summary>
        public bool ItemsDrift { get; private set; }

        /// <summary>Size of each thing in the room, in reference pixels.</summary>
        public float ItemSize { get; private set; }

        /// <summary>Average seconds per find for three and two stars.</summary>
        public float ThreeStarSeconds { get; private set; }

        public float TwoStarSeconds { get; private set; }

        public static BlurrySpySettings ForLevel(int level)
        {
            switch (Levels.Clamp(level))
            {
                case 1:
                    return new BlurrySpySettings { Level = 1, Targets = 5, Items = 10, StartBlur = 12f, ItemsDrift = false, ItemSize = 150f, ThreeStarSeconds = 5f, TwoStarSeconds = 10f };
                case 2:
                    return new BlurrySpySettings { Level = 2, Targets = 7, Items = 14, StartBlur = 16f, ItemsDrift = false, ItemSize = 130f, ThreeStarSeconds = 4.5f, TwoStarSeconds = 9f };
                default:
                    return new BlurrySpySettings { Level = 3, Targets = 9, Items = 18, StartBlur = 20f, ItemsDrift = true, ItemSize = 118f, ThreeStarSeconds = 4f, TwoStarSeconds = 8f };
            }
        }
    }

    public enum SpyTapResult
    {
        Found,
        NotThisOne,
        Ignored,
    }

    /// <summary>
    /// Rules of Blurry Spy. After eye drops things can look blurry and bright for a while, so the
    /// room starts blurry and bright, and the child finds the pictured things one at a time. Each
    /// find clears the blur a little, showing that it goes away. A wrong tap only wiggles the
    /// thing, but a perfect round (three stars) needs no wrong taps.
    /// </summary>
    public sealed class BlurrySpyRound
    {
        private readonly List<int> targets = new List<int>();

        public BlurrySpyRound(BlurrySpySettings settings, int seed = 0)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            var random = new Random(seed);
            var order = new List<int>();
            for (var i = 0; i < settings.Items; i++)
            {
                order.Add(i);
            }

            for (var i = order.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }

            targets.AddRange(order.GetRange(0, Math.Min(settings.Targets, order.Count)));
        }

        public BlurrySpySettings Settings { get; }

        /// <summary>The item index to find next, or -1 when the round is done.</summary>
        public int CurrentTarget => Found < targets.Count ? targets[Found] : -1;

        public IReadOnlyList<int> Targets => targets;

        public int Found { get; private set; }

        public int WrongTaps { get; private set; }

        public float Elapsed { get; private set; }

        public bool Done => Found >= targets.Count;

        /// <summary>Share of the things found so far, 0..1.</summary>
        public float Cleared => (float)Found / Math.Max(1, targets.Count);

        /// <summary>Blur now, in reference pixels: it fades with every find and is gone at the end.</summary>
        public float Blur => Settings.StartBlur * (1f - Cleared);

        /// <summary>Glare now (0..1): bright at first and dimmer as the eyes settle.</summary>
        public float Glare => 0.35f + 0.65f * (1f - Cleared);

        public float SecondsPerFind => Elapsed / Math.Max(1, Found);

        /// <summary>
        /// Three stars only for a perfect round: no wrong taps and quick finds. Two for at most two
        /// wrong taps and steady finds. One for finishing.
        /// </summary>
        public int Stars
        {
            get
            {
                if (WrongTaps == 0 && SecondsPerFind <= Settings.ThreeStarSeconds)
                {
                    return 3;
                }

                return WrongTaps <= 2 && SecondsPerFind <= Settings.TwoStarSeconds ? 2 : 1;
            }
        }

        /// <summary>Points for the score: ten per find, plus a speed bonus.</summary>
        public int Score => Found * 10 + (int)Math.Max(0.0, Math.Round(Settings.Targets * Settings.TwoStarSeconds - Elapsed));

        public void Tick(float deltaSeconds)
        {
            if (!Done && deltaSeconds > 0f)
            {
                Elapsed += deltaSeconds;
            }
        }

        public SpyTapResult Tap(int item)
        {
            if (Done || item < 0 || item >= Settings.Items)
            {
                return SpyTapResult.Ignored;
            }

            if (item != CurrentTarget)
            {
                WrongTaps++;
                return SpyTapResult.NotThisOne;
            }

            Found++;
            return SpyTapResult.Found;
        }
    }
}
