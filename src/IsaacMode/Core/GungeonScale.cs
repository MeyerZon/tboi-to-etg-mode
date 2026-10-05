namespace IsaacMode.Core
{
    /// <summary>
    /// Converts TBOI stat values into the numbers the Tears weapon uses in the Gungeon
    /// (STA-1, STA-3, STA-4). The two scale constants are the play-test tunables.
    /// </summary>
    public static class GungeonScale
    {
        /// <summary>Gungeon enemies have far more health than TBOI's, so tear damage is multiplied (STA-3).</summary>
        public const double DamageScale = 2.0;

        /// <summary>Gungeon rooms are larger than TBOI's, so range and tear speed are multiplied (STA-4).</summary>
        public const double WorldScale = 2.0;

        /// <summary>TBOI tear speed in tiles per second at shot speed 1.0.</summary>
        public const double TilesPerSecondAtShotSpeedOne = 7.5;

        /// <summary>Seconds between tears for a given tear delay.</summary>
        public static double Cooldown(double tearDelay)
        {
            return 1.0 / TearFormulas.FireRate(tearDelay);
        }

        /// <summary>Gungeon damage per tear for a TBOI effective damage value.</summary>
        public static double Damage(double tboiDamage)
        {
            return tboiDamage * DamageScale;
        }

        /// <summary>Gungeon range in units for a TBOI range in tiles.</summary>
        public static double Range(double tboiRange)
        {
            return tboiRange * WorldScale;
        }

        /// <summary>Gungeon projectile speed in units per second for a TBOI shot speed.</summary>
        public static double ProjectileSpeed(double shotSpeed)
        {
            return shotSpeed * TilesPerSecondAtShotSpeedOne * WorldScale;
        }
    }
}
