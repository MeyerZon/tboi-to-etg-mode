namespace IsaacMode.Core
{
    /// <summary>Outcome of a TBOI room-clear reward roll.</summary>
    public enum RoomClearReward
    {
        Nothing,
        CardPillOrTrinket,
        Coin,
        Heart,
        Key,
        Bomb,
        Chest
    }

    /// <summary>
    /// TBOI room-clear award table (docs/research/mechanics-and-ip.md, A5):
    /// value = r1 * clamp(luck, 0, 10) * 0.1 + r2, then bands
    /// &lt;0.22 nothing, 0.22-0.30 card/pill/trinket, 0.30-0.45 coin, 0.45-0.60 heart,
    /// 0.60-0.80 key, 0.80-0.95 bomb, &gt;0.95 chest.
    /// </summary>
    public static class RoomClearRewards
    {
        public static double RollValue(double random1, double random2, double luck)
        {
            return random1 * TearFormulas.ClampLuckForDrops(luck) * 0.1 + random2;
        }

        public static RoomClearReward Classify(double value)
        {
            if (value < 0.22) return RoomClearReward.Nothing;
            if (value < 0.30) return RoomClearReward.CardPillOrTrinket;
            if (value < 0.45) return RoomClearReward.Coin;
            if (value < 0.60) return RoomClearReward.Heart;
            if (value < 0.80) return RoomClearReward.Key;
            if (value <= 0.95) return RoomClearReward.Bomb;
            return RoomClearReward.Chest;
        }

        public static RoomClearReward Roll(double random1, double random2, double luck)
        {
            return Classify(RollValue(random1, random2, luck));
        }
    }
}
