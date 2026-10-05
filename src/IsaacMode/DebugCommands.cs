using IsaacMode.Character;
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
        }

        private static void Mode(string[] args)
        {
            ETGModConsole.Log($"Isaac registered: {IsaacCharacter.Registered}; Isaac mode: {(Plugin.IsaacModeActive ? "on" : "off")}");
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
            if (gun == null)
            {
                ETGModConsole.Log("No gun held.");
                return;
            }

            ETGModConsole.Log($"Gun: {gun.DisplayName} (id {gun.PickupObjectId}, tears: {gun.PickupObjectId == TearsGun.PickupId})");
            ProjectileModule module = gun.DefaultModule;
            Projectile shot = module.projectiles[0];
            ETGModConsole.Log($"Cooldown {module.cooldownTime:0.000}s, damage {shot.baseData.damage}, speed {shot.baseData.speed}, range {shot.baseData.range}");
        }
    }
}
