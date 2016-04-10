using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public abstract class EquityCaculatorBase : IEquityCalculator
    {
        public void CaculateTwoWay(PHand[] handRangeA, PHand[] handRangeB,
                    out long[] win, out long[] tie, out long[] loss, out long total, out double[] winEquity, out double[] tieEquity,
                    out double[] totalEquity)
        {
            if (handRangeA == null || handRangeA.Length == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Length == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }
            win = new long[2] { 0, 0 };
            tie = new long[2] { 0, 0 };
            loss = new long[2] { 0, 0 };
            total = 0;

            foreach (PHand handA in handRangeA)
            {
                foreach (PHand handB in handRangeB)
                {
                    if (!HandUtility.HasConflict(handA, handB))
                    {
                        Debug.Assert(handA.HandIndex != handB.HandIndex);

                        int w1, w2, t1, t2, l1, l2, tot = 0;
                        if (handA.HandIndex < handB.HandIndex)
                        {
                            this.CaculateTwoWayConcrete(handA, handB, out w1, out w2, out t1, out t2, out l1, out l2, out tot);
                        }
                        else
                        {
                            this.CaculateTwoWayConcrete(handB, handA, out w2, out w1, out t2, out t1, out l2, out l1, out tot);
                        }

                        win[0] += w1;
                        win[1] += w2;
                        tie[0] += t1;
                        tie[1] += t2;
                        loss[0] += l1;
                        loss[1] += l2;
                        total += tot;
                    }
                }
            }
            this.CalculateEquity(2, win, tie, loss, total, out winEquity, out tieEquity, out totalEquity);
        }


        public void CalculateThreeWay(PHand[] handRangeA, PHand[] handRangeB, PHand[] handRangeC,
                                    out long[] win, out long[] tie, out long[] loss, out long total,
                                out double[] winEquity, out double[] tieEquity, out double[] totalEquity)
        {
            if (handRangeA == null || handRangeA.Length == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Length == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }

            if (handRangeC == null || handRangeC.Length == 0)
            {
                throw new ArgumentException("handRangeC cannot be null or empty.");
            }
            win = new long[3] { 0, 0, 0 };
            tie = new long[3] { 0, 0, 0 };
            loss = new long[3] { 0, 0, 0 };
            total = 0;

            foreach (PHand handA in handRangeA)
            {
                foreach (PHand handB in handRangeB)
                {
                    foreach (PHand handC in handRangeC)
                    {
                        if (!HandUtility.HasConflict(handA, handB, handC))
                        {
                            Debug.Assert(handA.HandIndex != handB.HandIndex);

                            int w1, w2, w3, t1, t2, t3, l1, l2, l3, tot = 0;
                            if (handA.HandIndex <= handB.HandIndex && handB.HandIndex <= handC.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handA, handB, handC, out w1, out w2, out w3, out t1, out t2, out t3, out l1, out l2, out l3, out tot);
                            }
                            else if (handA.HandIndex <= handC.HandIndex && handC.HandIndex <= handB.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handA, handC, handB, out w1, out w3, out w2, out t1, out t3, out t2, out l1, out l3, out l2, out tot);
                            }
                            else if (handB.HandIndex <= handA.HandIndex && handA.HandIndex <= handC.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handB, handA, handC, out w2, out w1, out w3, out t2, out t1, out t3, out l2, out l1, out l3, out tot);
                            }
                            else if (handB.HandIndex <= handC.HandIndex && handC.HandIndex <= handA.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handB, handC, handA, out w2, out w3, out w1, out t2, out t3, out t1, out l2, out l3, out l1, out tot);
                            }
                            else if (handC.HandIndex <= handA.HandIndex && handA.HandIndex <= handB.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handC, handA, handB, out w3, out w1, out w2, out t3, out t1, out t2, out l3, out l1, out l2, out tot);
                            }
                            else //if (handC.HandIndex <= handB.HandIndex && handB.HandIndex <= handA.HandIndex)
                            {
                                this.CalculateThreeWayConcrete(handC, handB, handA, out w3, out w2, out w1, out t3, out t2, out t1, out l3, out l2, out l1, out tot);
                            }

                            win[0] += w1;
                            win[1] += w2;
                            win[2] += w3;
                            tie[0] += t1;
                            tie[1] += t2;
                            tie[2] += t3;
                            loss[0] += l1;
                            loss[1] += l2;
                            loss[2] += l3;
                            total += tot;
                        }
                    }
                }
            }
            this.CalculateEquity(3, win, tie, loss, total, out winEquity, out tieEquity, out totalEquity);
        }

        public abstract void CaculateTwoWayConcrete(PHand handA, PHand handB, out int win1, out int win2,
            out int tie1, out int tie2, out int loss1, out int loss2, out int total);

        public abstract void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC,
            out int win1, out int win2, out int win3, out int tie1, out int tie2, out int tie3,
            out int loss1, out int loss2, out int loss3, out int total);


        private void CalculateEquity(int player, long[] win, long[] tie, long[] loss, long total, out double[] winEquity,
            out double[] tieEquity, out double[] totalEquity)
        {
            winEquity = new double[player];
            tieEquity = new double[player];
            totalEquity = new double[player];

            if (player == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    winEquity[i] = win[i] / (double)total;
                    tieEquity[i] = tie[i] / (double)total / 2;
                    totalEquity[i] = winEquity[i] + tieEquity[i];
                }
            }
            else if (player == 3)
            {
                long[] tieToOtherTwo = new long[3];
                long[] tieToAllThree = new long[3];
                tieToOtherTwo[0] = loss[0] - win[1] - win[2];
                tieToOtherTwo[1] = loss[1] - win[0] - win[2];
                tieToOtherTwo[2] = loss[2] - win[0] - win[1];

                tieToAllThree[0] = tie[0] - tieToOtherTwo[1] - tieToOtherTwo[2];
                tieToAllThree[1] = tie[1] - tieToOtherTwo[0] - tieToOtherTwo[2];
                tieToAllThree[2] = tie[2] - tieToOtherTwo[0] - tieToOtherTwo[1];

                tieEquity[0] = (tieToOtherTwo[1] / (double)2 + tieToOtherTwo[2] / (double)2 + tieToAllThree[0] / (double)3) /
                               total;
                tieEquity[1] = (tieToOtherTwo[0] / (double)2 + tieToOtherTwo[2] / (double)2 + tieToAllThree[1] / (double)3) /
                               total;
                tieEquity[2] = (tieToOtherTwo[0] / (double)2 + tieToOtherTwo[1] / (double)2 + tieToAllThree[2] / (double)3) /
                               total;

                winEquity[0] = win[0] / (double)total;
                winEquity[1] = win[1] / (double)total;
                winEquity[2] = win[2] / (double)total;

                totalEquity[0] = winEquity[0] + tieEquity[0];
                totalEquity[1] = winEquity[1] + tieEquity[1];
                totalEquity[2] = winEquity[2] + tieEquity[2];
            }
            else
            {
                throw new Exception("unsupported");
            }
        }


    }
}
