using System;
using GameTreeDraft.Hands;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.Equity
{
    public class BrutalForceEquityCalculator : EquityCaculatorBase
    {
        public override void CaculateTwoWayConcrete(PHand handA, PHand handB, out int win1, out int win2,
            out int tie1, out int tie2, out int loss1, out int loss2, out int total)
        {
            long[] wins = new long[2];
            long[] ties = new long[2];
            long[] losses = new long[2];
            long totHand = 0;
            Hand.HandOdds(new[] { handA.FullHandStr, handB.FullHandStr }, string.Empty, string.Empty, wins, ties, losses, ref totHand);

            win1 = checked((int)wins[0]);
            win2 = checked((int)wins[1]);
            tie1 = checked((int)ties[0]);
            tie2 = checked((int)ties[1]);
            loss1 = checked((int)losses[0]);
            loss2 = checked((int)losses[1]);
            total = checked((int)totHand);
        }

        public override void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC, 
            out int win1, out int win2, out int win3, out int tie1, out int tie2, out int tie3,
            out int loss1, out int loss2, out int loss3, out int total)
        {
            long[] wins = new long[3];
            long[] ties = new long[3];
            long[] losses = new long[3];
            long totHand = 0;
            Hand.HandOdds(new[] { handA.FullHandStr, handB.FullHandStr , handC.FullHandStr}, string.Empty, string.Empty, wins, ties, losses, ref totHand);

            win1 = checked((int)wins[0]);
            win2 = checked((int)wins[1]);
            win3 = checked ((int) wins[2]);
            tie1 = checked((int)ties[0]);
            tie2 = checked((int)ties[1]);
            tie3 = checked((int)ties[2]);
            loss1 = checked((int)losses[0]);
            loss2 = checked((int)losses[1]);
            loss3 = checked((int)losses[2]);

            total = checked((int)totHand);
        }
    }
}
