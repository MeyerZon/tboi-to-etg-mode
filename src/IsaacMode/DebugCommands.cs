using Alexandria.StatAPI;
using IsaacMode.Character;
using IsaacMode.Stats;
using IsaacMode.Tears;

namespace IsaacMode
{
    /// <summary>F2 console group <c>isaac</c> for play-testing (UI-3).</summary>
    public static class DebugCommands
    {
        public static void Init()
        {
            ETGModConsole.Commands.AddGroup("isaac");
            ConsoleCommandGroup group = ETGModConsole.Commands.GetGroup("isaac");
            group.AddUnit("mode", Mode);
            group.AddUnit("stats", Stats);
            group.AddUnit("addstat", AddStat);
        }

        private static void Mode(string[] args)
        {
            ETGModConsole.Log($"Isaac registered: {IsaacCharacter.Registered}; Isaac mode: {(Plugin.IsaacModeActive ? "on" : "off")}");
        }

        /// <summary><c>isaac addstat tears 1</c>: adds to a custom stat for the rest of the run, for testing.</summary>
        private static void AddStat(string[] args)
        {
            PlayerController player = GameManager.Instance != null ? GameManager.Instance.PrimaryPlayer : null;
            float amount;
            if (player == null || args.Length != 2 || System.Array.IndexOf(IsaacStats.AllStats, args[0].ToLower()) < 0
                || !float.TryParse(args[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out amount))
            {
                ETGModConsole.Log("Usage, during a run: isaac addstat <" + string.Join("|", IsaacStats.AllStats) + "> <amount>");
                return;
            }

            player.ownerlessStatModifiers.Add(StatAPIManager.CreateCustomStatModifier(IsaacStats.Prefix, args[0].ToLower(), amount));
            player.stats.RecalculateStats(player, true);
            Stats(args);
        }

        private static void Stats(string[] args)
        {
            PlayerController player = GameManager.Instance != null ? GameManager.Instance.PrimaryPlayer : null;
            if (player == null)
            {
                ETGModConsole.Log("No player yet; start a run first.");
                return;
            }

            Gun gun = player.CurrentGun;
            ETGModConsole.Log($"Character: {player.characterIdentity} (Isaac: {player.IsIsaac()})");
            ETGModConsole.Log($"Health: {player.healthHaver.GetCurrentHealth()}/{player.healthHaver.GetMaxHealth()}, armor {player.healthHaver.Armor}, blanks {player.Blanks}");
            if (player.IsIsaac())
            {
                IsaacStats isaac = IsaacStats.For(player);
                ETGModConsole.Log($"Tears {isaac.Tears} (delay {isaac.TearDelay:0.00}), damage ups {isaac.DamageUps}, flat damage {isaac.FlatDamage} (factor {isaac.DamageFactor:0.00}), luck {isaac.Luck}, tear height {isaac.TearHeight}");
            }
            if (gun == null)
            {
                ETGModConsole.Log("No gun held.");
                return;
            }

            ETGModConsole.Log($"Gun: {gun.DisplayName} (id {gun.PickupObjectId}, tears: {gun.PickupObjectId == TearsGun.PickupId})");
            ProjectileModule module = gun.DefaultModule;
            Projectile shot = module.projectiles[0];
            ETGModConsole.Log($"Cooldown {module.cooldownTime:0.000}s, base damage {shot.baseData.damage}, speed {shot.baseData.speed}, range {shot.baseData.range}");
        }
    }
}
