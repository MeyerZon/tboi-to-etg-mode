using IsaacMode.Core;
using Xunit;

namespace IsaacMode.Tests
{
    public class TearFormulasTests
    {
        [Fact]
        public void IsaacBaseFireRateIsAbout2_73TearsPerSecond()
        {
            Assert.Equal(2.727, TearFormulas.FireRate(TearFormulas.BaseTearDelay), 3);
        }

        [Fact]
        public void ZeroTearsStatGivesBaseTearDelay()
        {
            Assert.Equal(TearFormulas.BaseTearDelay, TearFormulas.TearDelayFromTearsStat(0.0), 6);
        }

        [Fact]
        public void TearDelayFromStatUpsNeverGoesBelowTheCap()
        {
            // 1.816 is where the curve meets the cap (16 - 6*sqrt(1.816*1.3 + 1) = 5.001).
            Assert.Equal(TearFormulas.TearDelayCap, TearFormulas.TearDelayFromTearsStat(1.816), 2);
            Assert.Equal(TearFormulas.TearDelayCap, TearFormulas.TearDelayFromTearsStat(1.9), 6);
            Assert.Equal(TearFormulas.TearDelayCap, TearFormulas.TearDelayFromTearsStat(50.0), 6);
            Assert.Equal(5.0, TearFormulas.FireRate(TearFormulas.TearDelayCap), 6);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-2.0)]
        public void NegativeTearsStatIncreasesDelay(double tears)
        {
            Assert.True(TearFormulas.TearDelayFromTearsStat(tears) > TearFormulas.BaseTearDelay);
        }

        [Fact]
        public void TearDelayIsContinuousAtTheNegativeBreakpoint()
        {
            double bp = TearFormulas.NegativeTearsBreakpoint;
            double left = TearFormulas.TearDelayFromTearsStat(bp - 1e-9);
            double right = TearFormulas.TearDelayFromTearsStat(bp + 1e-9);
            Assert.False(double.IsNaN(left));
            Assert.False(double.IsNaN(right));
            Assert.True(System.Math.Abs(left - right) < 1e-3, $"left={left} right={right}");
        }

        [Fact]
        public void TearDelayIsNeverNaNAcrossTheWholeRange()
        {
            for (double t = -5.0; t <= 5.0; t += 0.01)
            {
                Assert.False(double.IsNaN(TearFormulas.TearDelayFromTearsStat(t)), $"NaN at tears={t}");
            }
        }

        [Fact]
        public void BaseDamageWithNoItemsIs3_5()
        {
            Assert.Equal(3.5, TearFormulas.Damage(TearFormulas.BaseDamage, 0, 0, 1.0), 6);
        }

        [Fact]
        public void OneDamageUpWithMagicMushroomMatchesWikiExample()
        {
            // (3.5 * sqrt(1*1.2 + 1)) * 1.5 = 3.5 * 1.4832 * 1.5 = 7.787
            Assert.Equal(7.787, TearFormulas.Damage(TearFormulas.BaseDamage, 1, 0, 1.5), 3);
        }

        [Theory]
        [InlineData(-3, 0)]
        [InlineData(4, 4)]
        [InlineData(25, 10)]
        public void LuckIsClampedForDrops(double luck, double expected)
        {
            Assert.Equal(expected, TearFormulas.ClampLuckForDrops(luck));
        }
    }
}
