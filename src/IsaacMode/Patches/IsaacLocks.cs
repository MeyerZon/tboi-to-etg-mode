using HarmonyLib;
using IsaacMode.Character;
using IsaacMode.Tears;
using UnityEngine;

namespace IsaacMode.Patches
{
    /// <summary>
    /// What Isaac cannot do: hold any gun but his tears (CHR-8), dodge roll or use blanks (MOD-13, HLT-5).
    /// Every patch is a no-op for other Gungeoneers.
    /// </summary>
    public static class IsaacLocks
    {
        private static bool IsForeignGun(Gun gun)
        {
            return gun != null && gun.PickupObjectId != TearsGun.PickupId;
        }

        /// <summary>Guns on the floor stay on the floor. Converting them to Isaac items comes with the loot table (MOD-7).</summary>
        [HarmonyPatch(typeof(Gun), nameof(Gun.Pickup))]
        private static class GunPickupPatch
        {
            private static bool Prefix(Gun __instance, PlayerController player)
            {
                return !(player.IsIsaac() && IsForeignGun(__instance));
            }
        }

        /// <summary>
        /// Safety net for every path that hands a gun over directly (chests, shops, other mods).
        /// Returns the held gun instead of null because vanilla callers dereference the result.
        /// </summary>
        [HarmonyPatch(typeof(GunInventory), nameof(GunInventory.AddGunToInventory))]
        private static class AddGunToInventoryPatch
        {
            private static bool Prefix(GunInventory __instance, Gun gun, ref Gun __result)
            {
                PlayerController player = __instance.Owner as PlayerController;
                if (!player.IsIsaac() || !IsForeignGun(gun)) return true;
                // The first gun added is the starting Tears; never block while the inventory is empty.
                if (__instance.CurrentGun == null) return true;
                __result = __instance.CurrentGun;
                return false;
            }
        }

        /// <summary>Isaac never holds blanks: none at the start, none per floor, none from pickups.</summary>
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Blanks), MethodType.Setter)]
        private static class BlanksSetterPatch
        {
            private static void Prefix(PlayerController __instance, ref int value)
            {
                if (__instance.IsIsaac()) value = 0;
            }
        }

        /// <summary>Losing armor fires a free blank in vanilla; not for Isaac (HLT-1).</summary>
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnLostArmor))]
        private static class LostArmorPatch
        {
            internal static bool Suppressing;

            private static void Prefix(PlayerController __instance)
            {
                Suppressing = __instance.IsIsaac();
            }

            private static void Finalizer()
            {
                Suppressing = false;
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ForceBlank))]
        private static class ForceBlankPatch
        {
            private static bool Prefix()
            {
                return !LostArmorPatch.Suppressing;
            }
        }

        /// <summary>
        /// The dodge-roll input does nothing for Isaac. Rolls the game forces through
        /// <c>ForceStartDodgeRoll</c> (scripted moments) still go through.
        /// </summary>
        [HarmonyPatch(typeof(PlayerController), "StartDodgeRoll")]
        private static class StartDodgeRollPatch
        {
            internal static int ForcedDepth;

            private static bool Prefix(PlayerController __instance, ref bool __result)
            {
                if (ForcedDepth > 0 || !__instance.IsIsaac()) return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ForceStartDodgeRoll), new System.Type[0])]
        private static class ForceStartDodgeRollPatch
        {
            private static void Prefix()
            {
                StartDodgeRollPatch.ForcedDepth++;
            }

            private static void Finalizer()
            {
                StartDodgeRollPatch.ForcedDepth--;
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ForceStartDodgeRoll), new[] { typeof(Vector2) })]
        private static class ForceStartDodgeRollVectorPatch
        {
            private static void Prefix()
            {
                StartDodgeRollPatch.ForcedDepth++;
            }

            private static void Finalizer()
            {
                StartDodgeRollPatch.ForcedDepth--;
            }
        }
    }
}
