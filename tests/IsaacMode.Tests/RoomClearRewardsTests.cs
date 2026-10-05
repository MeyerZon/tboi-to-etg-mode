using IsaacMode.Core;
using Xunit;

namespace IsaacMode.Tests
{
    public class RoomClearRewardsTests
    {
        [Theory]
        [InlineData(0.00, RoomClearReward.Nothing)]
        [InlineData(0.21, RoomClearReward.Nothing)]
        [InlineData(0.22, RoomClearReward.CardPillOrTrinket)]
        [InlineData(0.30, RoomClearReward.Coin)]
        [InlineData(0.45, RoomClearReward.Heart)]
        [InlineData(0.60, RoomClearReward.Key)]
        [InlineData(0.80, RoomClearReward.Bomb)]
        [InlineData(0.95, RoomClearReward.Bomb)]
        [InlineData(0.96, RoomClearReward.Chest)]
        public void BandsMatchTheWikiTable(double value, RoomClearReward expected)
        {
            Assert.Equal(expected, RoomClearRewards.Classify(value));
        }

        [Fact]
        public void ZeroLuckUsesOnlyTheSecondRoll()
        {
            Assert.Equal(0.5, RoomClearRewards.RollValue(1.0, 0.5, 0.0), 9);
        }

        [Fact]
        public void MaxLuckCanAddUpToOneWholePoint()
        {
            Assert.Equal(1.5, RoomClearRewards.RollValue(1.0, 0.5, 10.0), 9);
            Assert.Equal(1.5, RoomClearRewards.RollValue(1.0, 0.5, 99.0), 9);
        }

        [Fact]
        public void HighLuckTurnsNothingIntoSomething()
        {
            Assert.Equal(RoomClearReward.Nothing, RoomClearRewards.Roll(1.0, 0.10, 0.0));
            Assert.NotEqual(RoomClearReward.Nothing, RoomClearRewards.Roll(1.0, 0.10, 10.0));
        }
    }
}
