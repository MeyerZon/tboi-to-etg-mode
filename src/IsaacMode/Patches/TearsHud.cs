using HarmonyLib;
using IsaacMode.Tears;

namespace IsaacMode.Patches
{
    /// <summary>
    /// Shows a tear in the HUD gun box while the Tears are held (CHR-6). Only the HUD sprite is swapped;
    /// the gun object keeps its own sprite, so hand and barrel positions in the world do not move.
    /// </summary>
    [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.UpdateUIGun))]
    internal static class TearsHud
    {
        private static void Postfix(GameUIAmmoController __instance, GunInventory guns)
        {
            if (TearsGun.HudIconCollection == null || guns == null || __instance.gunSprites == null) return;
            Gun current = guns.CurrentGun;
            if (current == null || current.PickupObjectId != TearsGun.PickupId) return;

            foreach (tk2dClippedSprite box in __instance.gunSprites)
            {
                if (box == null) continue;
                // The children are the outline sprites; they must show the same shape.
                foreach (tk2dBaseSprite sprite in box.GetComponentsInChildren<tk2dBaseSprite>(true))
                {
                    if (sprite.Collection != TearsGun.HudIconCollection || sprite.spriteId != TearsGun.HudIconId)
                        sprite.SetSprite(TearsGun.HudIconCollection, TearsGun.HudIconId);
                }
            }
        }
    }
}
