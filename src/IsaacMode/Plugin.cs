using BepInEx;
using HarmonyLib;
using Alexandria.Misc;

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
            CustomActions.OnRunStart += OnRunStart;
            ETGModConsole.Log($"{NAME} v{VERSION} loaded");
        }

        private static void OnRunStart(PlayerController player1, PlayerController player2, GameManager.GameMode mode)
        {
            // Placeholder until the Isaac character exists (CHR-1): any custom character
            // identity sits above the last vanilla Gungeoneer in the PlayableCharacters enum.
            IsaacModeActive = player1 != null && player1.characterIdentity > PlayableCharacters.Gunslinger;
            ETGModConsole.Log($"{NAME}: run started, Isaac mode {(IsaacModeActive ? "on" : "off")}");
        }
    }
}
