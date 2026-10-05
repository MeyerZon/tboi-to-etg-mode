using IsaacMode.Core;
using Xunit;

namespace IsaacMode.Tests
{
    public class GungeonScaleTests
    {
        [Fact]
        public void BaseIsaacFiresAboutEveryThirdOfASecond()
        {
            // Tear delay 10 is 30 / 11 = 2.73 tears per second.
            Assert.Equal(11.0 / 30.0, GungeonScale.Cooldown(TearFormulas.BaseTearDelay), 6);
        }

        [Fact]
        public void CooldownAtTheTearDelayCapIsFiveTearsPerSecond()
        {
            Assert.Equal(0.2, GungeonScale.Cooldown(TearFormulas.TearDelayCap), 6);
        }

        [Fact]
        public void BaseStatsScaleByTheTunables()
        {
            Assert.Equal(3.5 * GungeonScale.DamageScale, GungeonScale.Damage(TearFormulas.BaseDamage), 6);
            Assert.Equal(6.5 * GungeonScale.WorldScale, GungeonScale.Range(TearFormulas.BaseRange), 6);
            Assert.Equal(7.5 * GungeonScale.WorldScale, GungeonScale.ProjectileSpeed(TearFormulas.BaseShotSpeed), 6);
        }
    }
}
