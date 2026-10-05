using System;

namespace IsaacMode.Core
{
    /// <summary>
    /// The Binding of Isaac (Repentance) stat formulas, kept free of any game or Unity types
    /// so they can be unit tested on any .NET runtime. Sources: docs/research/mechanics-and-ip.md, A1.
    /// </summary>
    public static class TearFormulas
    {
        /// <summary>Isaac's base values.</summary>
        public const double BaseDamage = 3.5;
        public const int BaseTearDelay = 10;
        public const double BaseRange = 6.5;
        public const double BaseShotSpeed = 1.0;

        /// <summary>Tear delay at which stat-ups stop improving fire rate (5 tears per second).</summary>
        public const double TearDelayCap = 5.0;

        /// <summary>
        /// Below this tears stat the square-root term vanishes (1.3 * T + 1 = 0) and the formula
        /// becomes purely linear. The wiki quotes it rounded as -0.77.
        /// </summary>
        public const double NegativeTearsBreakpoint = -1.0 / 1.3;

        /// <summary>Fire rate in tears per second for a given tear delay (frames at 30 fps).</summary>
        public static double FireRate(double tearDelay)
        {
            if (tearDelay < 1.0) tearDelay = 1.0;
            return 30.0 / (tearDelay + 1.0);
        }

        /// <summary>
        /// Converts the summed "tears" stat (base 0) into a tear delay.
        /// Positive tears follow 16 - 6*sqrt(T*1.3 + 1) down to the cap of 5;
        /// small negatives add a linear penalty; large negatives are purely linear.
        /// </summary>
        public static double TearDelayFromTearsStat(double tears)
        {
            double delay;
            if (tears >= 0.0)
            {
                delay = 16.0 - 6.0 * Math.Sqrt(tears * 1.3 + 1.0);
                if (delay < TearDelayCap) delay = TearDelayCap;
            }
            else if (tears > NegativeTearsBreakpoint)
            {
                double inner = tears * 1.3 + 1.0;
                if (inner < 0.0) inner = 0.0;
                delay = 16.0 - 6.0 * Math.Sqrt(inner) - 6.0 * tears;
            }
            else
            {
                delay = 16.0 - 6.0 * tears;
            }
            return delay;
        }

        /// <summary>
        /// Effective damage: (base * sqrt(damageUps * 1.2 + 1) + flatUps) * multiplier.
        /// Callers are responsible for the "x1.5 items do not stack" rule when building the multiplier.
        /// </summary>
        public static double Damage(double baseDamage, double damageUps, double flatUps, double multiplier)
        {
            double inner = damageUps * 1.2 + 1.0;
            if (inner < 0.0) inner = 0.0;
            return (baseDamage * Math.Sqrt(inner) + flatUps) * multiplier;
        }

        /// <summary>
        /// Tear sprite scale for an effective damage value: sqrt(d) * 0.23 + d * 0.01 + 0.55,
        /// which is almost exactly 1 at Isaac's base damage.
        /// </summary>
        public static double TearScale(double damage)
        {
            if (damage < 0.0) damage = 0.0;
            return Math.Sqrt(damage) * 0.23 + damage * 0.01 + 0.55;
        }

        /// <summary>Fraction of the range a tear flies level before it starts to fall.</summary>
        public const double ArcLevelFraction = 0.6;

        /// <summary>
        /// How far a tear has fallen, from 0 (still at firing height) to 1 (on the ground), for a
        /// flight progress of 0..1 along its range. Level at first, then an accelerating drop.
        /// </summary>
        public static double ArcDrop(double progress)
        {
            if (progress <= ArcLevelFraction) return 0.0;
            if (progress >= 1.0) return 1.0;
            double t = (progress - ArcLevelFraction) / (1.0 - ArcLevelFraction);
            return t * t;
        }

        /// <summary>Luck as used for room-clear drop weighting is clamped to 0..10.</summary>
        public static double ClampLuckForDrops(double luck)
        {
            if (luck < 0.0) return 0.0;
            if (luck > 10.0) return 10.0;
            return luck;
        }
    }
}
