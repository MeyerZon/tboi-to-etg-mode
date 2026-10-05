using IsaacMode.Core;
using UnityEngine;

namespace IsaacMode.Tears
{
    /// <summary>
    /// TBOI tear behaviour on an ETG projectile (CHR-7): the tear flies level, then falls to the ground
    /// as it reaches its range; its size follows its damage and its knockback follows its speed.
    /// The fall is visual only (the sprite is lowered); the hitbox keeps flying straight, as in TBOI.
    /// </summary>
    public class TearArc : MonoBehaviour
    {
        /// <summary>Knockback of a tear at shot speed 1.0, in ETG force units.</summary>
        public const float BaseForce = 14f;

        /// <summary>How far the tear sprite drops over its fall, in units (16 px each).</summary>
        public const float FallHeight = 0.75f;

        /// <summary>Added to <see cref="FallHeight"/> for this flight; set from the tear height stat.</summary>
        public float ExtraFallHeight;

        private static bool _warnedNoSpriteChild;

        private Projectile _projectile;
        private Transform _spriteTransform;
        private Vector3 _spriteRestPosition;
        private float _lastDistance = -1f;

        private bool _lookedForSprite;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
        }

        /// <summary>Finds the sprite once, on first use: the game may only assign it after our Awake.</summary>
        private void FindSprite()
        {
            _lookedForSprite = true;
            tk2dBaseSprite sprite = _projectile.sprite;
            if (sprite == null) sprite = GetComponentInChildren<tk2dBaseSprite>();
            if (sprite != null && sprite.transform != transform)
            {
                _spriteTransform = sprite.transform;
                _spriteRestPosition = _spriteTransform.localPosition;
            }
        }

        private void LateUpdate()
        {
            if (_projectile == null) return;
            if (!_lookedForSprite) FindSprite();

            float distance = _projectile.GetElapsedDistance();
            // Projectiles are pooled, so a distance that went backwards means a new flight.
            if (_lastDistance < 0f || distance < _lastDistance)
                BeginFlight();
            _lastDistance = distance;

            if (_spriteTransform == null) return;
            float range = _projectile.baseData.range;
            float progress = range > 0f ? distance / range : 1f;
            float drop = (float)TearFormulas.ArcDrop(progress) * Mathf.Max(0f, FallHeight + ExtraFallHeight);
            _spriteTransform.localPosition = _spriteRestPosition + Vector3.down * drop;
        }

        /// <summary>Applies size and knockback once the game has put the player's stats on the projectile.</summary>
        private void BeginFlight()
        {
            ProjectileData data = _projectile.baseData;

            double tboiDamage = data.damage / GungeonScale.DamageScale;
            double scale = TearFormulas.TearScale(tboiDamage) / TearFormulas.TearScale(TearFormulas.BaseDamage);
            if (Mathf.Abs((float)scale - 1f) > 0.01f)
                _projectile.RuntimeUpdateScale((float)scale);

            double baseSpeed = GungeonScale.ProjectileSpeed(TearFormulas.BaseShotSpeed);
            data.force = BaseForce * (float)(data.speed / baseSpeed);

            if (_spriteTransform == null && !_warnedNoSpriteChild)
            {
                _warnedNoSpriteChild = true;
                ETGModConsole.Log("Isaac Mode: the tear sprite is not a child object, so the arc cannot be shown.");
            }
        }
    }
}
