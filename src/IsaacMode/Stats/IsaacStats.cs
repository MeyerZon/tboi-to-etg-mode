using Alexandria.StatAPI;
using IsaacMode.Character;
using IsaacMode.Core;
using IsaacMode.Tears;
using UnityEngine;

namespace IsaacMode.Stats
{
    /// <summary>
    /// Isaac's TBOI stats that have no ETG equivalent, kept as Alexandria StatAPI custom stats (STA-2, ITM-4),
    /// and the values derived from them. Lives on the player; refreshed on every stat recalculation.
    /// </summary>
    public class IsaacStats : MonoBehaviour
    {
        public const string Prefix = "isaac";
        public const string TearsStat = "tears";
        public const string LuckStat = "luck";
        public const string DamageUpsStat = "damageups";
        public const string FlatDamageStat = "flatdamage";
        public const string TearHeightStat = "tearheight";

        public static readonly string[] AllStats = { TearsStat, LuckStat, DamageUpsStat, FlatDamageStat, TearHeightStat };

        public float Tears;
        public float Luck;
        public float DamageUps;
        public float FlatDamage;
        /// <summary>Extra fall height in units on top of <see cref="TearArc.FallHeight"/>.</summary>
        public float TearHeight;

        /// <summary>TBOI tear delay from the tears stat (STA-1).</summary>
        public float TearDelay = TearFormulas.BaseTearDelay;
        /// <summary>Tear damage relative to Isaac's base damage (STA-3); ETG's own Damage stat multiplies on top.</summary>
        public float DamageFactor = 1f;

        /// <summary>Call once at load, before any player exists: StatAPI defaults unknown custom stats to 1.</summary>
        public static void Init()
        {
            foreach (string stat in AllStats)
                StatAPIManager.baseStatValues[new Tuple<string, string>(Prefix, stat)] = 0f;
            StatAPIManager.FinalPostProcessing += OnStatsRecalculated;
        }

        public static IsaacStats For(PlayerController player)
        {
            return player != null ? player.gameObject.GetOrAddComponent<IsaacStats>() : null;
        }

        private static void OnStatsRecalculated(PlayerStats stats, PlayerController owner, CustomStatValues values, ref float healAmount)
        {
            if (!owner.IsIsaac()) return;

            IsaacStats isaac = For(owner);
            isaac.Tears = values[Prefix, TearsStat];
            isaac.Luck = values[Prefix, LuckStat];
            isaac.DamageUps = values[Prefix, DamageUpsStat];
            isaac.FlatDamage = values[Prefix, FlatDamageStat];
            isaac.TearHeight = values[Prefix, TearHeightStat];

            isaac.TearDelay = (float)TearFormulas.TearDelayFromTearsStat(isaac.Tears);
            isaac.DamageFactor = (float)(TearFormulas.Damage(TearFormulas.BaseDamage, isaac.DamageUps, isaac.FlatDamage, 1.0) / TearFormulas.BaseDamage);

            TearsGun.ApplyCooldown(owner, (float)GungeonScale.Cooldown(isaac.TearDelay));
        }
    }
}
