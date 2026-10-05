using System;
using BepInEx;
using HarmonyLib;
using Alexandria.Misc;
using IsaacMode.Character;
using IsaacMode.Stats;
using IsaacMode.Tears;

namespace IsaacMode
{
    /// <summary>
    /// BepInEx entry point. Game-dependent initialisation must wait for the GameManager,
    /// so everything except Harmony patching happens in <see cref="GMStart"/>.
    /// </summary>
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency(Alexandria.Alexandria.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "meyerzon.etg.isaacmode";
        public const string NAME = "Isaac Mode";
        public const string VERSION = "0.1.0";

        /// <summary>True while the current run is an Isaac run (see REQUIREMENTS.md, MOD-1).</summary>
        public static bool IsaacModeActive { get; private set; }

        private Harmony _harmony;

        public void Start()
        {
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager gameManager)
        {
            try
            {
                IsaacStats.Init();
                // The gun must exist before the character: the loadout is resolved by console ID.
                TearsGun.Init();
                IsaacCharacter.Init();
                DebugCommands.Init();
                CustomActions.OnRunStart += OnRunStart;
                ETGModConsole.Log($"{NAME} v{VERSION} loaded");
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"{NAME} v{VERSION} failed to load: {e}");
                Logger.LogError(e);
            }
        }

        private static void OnRunStart(PlayerController player1, PlayerController player2, GameManager.GameMode mode)
        {
            IsaacModeActive = player1.IsIsaac();
            ETGModConsole.Log($"{NAME}: run started, Isaac mode {(IsaacModeActive ? "on" : "off")}");
        }
    }
}
