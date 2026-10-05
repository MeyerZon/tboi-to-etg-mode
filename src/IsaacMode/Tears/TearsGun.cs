using Alexandria.ItemAPI;
using Gungeon;
using IsaacMode.Core;
using UnityEngine;

namespace IsaacMode.Tears
{
    /// <summary>
    /// Isaac's tears, built as an ETG gun so the game's firing, stats and projectile systems apply (CHR-5).
    /// The gun itself is never shown: its renderers and the player's hands are hidden while it is held (CHR-6).
    /// </summary>
    public class TearsGun : GunBehaviour
    {
        public const string ConsoleId = "isaac:tears";

        /// <summary>The vanilla Tear Jerker, whose module, projectile and sounds are the starting point.</summary>
        private const int TearJerkerId = 33;

        private const string HideReason = "isaac_tears";

        public static int PickupId = -1;

        public static void Init()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Tears", "isaac_tears");
            Game.Items.Rename("outdated_gun_mods:tears", ConsoleId);
            gun.gameObject.AddComponent<TearsGun>();
            gun.SetShortDescription("Waterworks");
            gun.SetLongDescription("Isaac has no gun. He has never needed one.");

            Gun tearJerker = PickupObjectDatabase.GetById(TearJerkerId) as Gun;
            gun.gunSwitchGroup = tearJerker.gunSwitchGroup;

            ProjectileModule module = gun.AddProjectileModuleFrom(tearJerker);
            module.shootStyle = ProjectileModule.ShootStyle.Automatic;
            module.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            module.cooldownTime = (float)GungeonScale.Cooldown(TearFormulas.BaseTearDelay);
            module.numberOfShotsInClip = -1;
            module.angleVariance = 0f;
            module.ammoCost = 0;
            module.projectiles.Clear();
            module.projectiles.Add(BuildTear(tearJerker));

            gun.reloadTime = 0f;
            gun.SetBaseMaxAmmo(1000);
            gun.InfiniteAmmo = true;
            gun.CanBeDropped = false;
            gun.CanBeSold = false;
            gun.PersistsOnDeath = true;
            gun.PreventStartingOwnerFromDropping = true;
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            gun.encounterTrackable.journalData.SuppressInAmmonomicon = true;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            PickupId = gun.PickupObjectId;
        }

        private static Projectile BuildTear(Gun source)
        {
            Projectile tear = Object.Instantiate(source.DefaultModule.projectiles[0]);
            tear.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(tear.gameObject);
            Object.DontDestroyOnLoad(tear);
            tear.name = "isaac_tear";

            tear.baseData.damage = (float)GungeonScale.Damage(TearFormulas.BaseDamage);
            tear.baseData.speed = (float)GungeonScale.ProjectileSpeed(TearFormulas.BaseShotSpeed);
            tear.baseData.range = (float)GungeonScale.Range(TearFormulas.BaseRange);
            tear.baseData.force = TearArc.BaseForce;
            tear.shouldRotate = false;
            tear.gameObject.AddComponent<TearArc>();
            return tear;
        }

        public override void OnSwitchedToPlayer(PlayerController owner, GunInventory inventory, Gun oldGun, bool isNewGun)
        {
            base.OnSwitchedToPlayer(owner, inventory, oldGun, isNewGun);
            SetHidden(owner, true);
        }

        public override void OnSwitchedAwayFromPlayer(PlayerController owner, GunInventory inventory, Gun newGun, bool isNewGun)
        {
            base.OnSwitchedAwayFromPlayer(owner, inventory, newGun, isNewGun);
            SetHidden(owner, false);
        }

        public override void Update()
        {
            base.Update();
            // The game clears renderer overrides on respawn and floor load, so keep asserting ours.
            PlayerController owner = gun != null ? gun.CurrentOwner as PlayerController : null;
            if (owner != null && owner.CurrentGun == gun)
                SetHidden(owner, true);
        }

        private void LateUpdate()
        {
            // Belt and braces: whatever re-enabled the gun's renderer this frame, switch it off again
            // before drawing. Runs after the game's own Update calls.
            PlayerController owner = gun != null ? gun.CurrentOwner as PlayerController : null;
            if (owner == null || owner.CurrentGun != gun) return;
            tk2dBaseSprite gunSprite = gun.GetSprite();
            if (gunSprite != null && gunSprite.renderer != null && gunSprite.renderer.enabled)
            {
                gunSprite.renderer.enabled = false;
                SpriteOutlineManager.ToggleOutlineRenderers(gunSprite, false);
            }
        }

        private static void SetHidden(PlayerController player, bool hidden)
        {
            if (player == null) return;
            player.ToggleGunRenderers(!hidden, HideReason);
            player.ToggleHandRenderers(!hidden, HideReason);
        }
    }
}
