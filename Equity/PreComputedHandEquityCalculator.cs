using System;
using System.Collections.Generic;
using System.IO;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class PreComputedHandEquityCalculator
    {

        private static PreComputedHandEquityCalculator instance;
        private readonly static object lockobj = new object();

        private readonly HandEquity[] twoWayEquityArray = new HandEquity[169 * 169];

        private readonly HandEquity[] threeWayEquityArray = new HandEquity[169 * 169 * 169];

        public PreComputedHandEquityCalculator(string path)
        {
            string defaultPath = Path.Combine(path, @"169HandEquity2Way.dat");
            FileInfo fi = new FileInfo(defaultPath);
            using (FileStream fs = fi.OpenRead())
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                    {
                        for (int j = 0; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                        {
                            this.twoWayEquityArray[i * 169 + j] = new HandEquity()
                                {
                                    Wins = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Ties = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Losses = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Total = br.ReadInt64()
                                };
                        }
                    }
                }
            }

            defaultPath = Path.Combine(path, @"169HandEquity3Way.dat");
            fi = new FileInfo(defaultPath);
            byte[] buffer = null;
            long[] longArray = null;
            using (FileStream fs = fi.OpenRead())
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    buffer = br.ReadBytes(checked((int)fi.Length));
                    longArray = new long[buffer.Length / 8];

                    for (int i = 0; i < buffer.Length; i += 8)

                        longArray[i / 8] = BitConverter.ToInt64(buffer, i);


                }
            }

            int index = 0;
            for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
            {
                int baseI = 169 * 169 * i;
                for (int j = 0; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                {
                    int baseJ = 169 * j;
                    for (int k = 0; k < HandRange.Instance.Hand169RangeAll.Count; k++)
                    {
                        int baseK = baseI + baseJ;
                        this.threeWayEquityArray[baseK + k] = new HandEquity()
                        {

                            Wins = new[] { longArray[index++], longArray[index++], longArray[index++] },
                            Ties = new[] { longArray[index++], longArray[index++], longArray[index++] },
                            Losses = new[] { longArray[index++], longArray[index++], longArray[index++] },
                            Total = longArray[index++]
                        };
                    }
                }
            }
        }


        public static string DefaultPath { get; set; }

        public static PreComputedHandEquityCalculator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new PreComputedHandEquityCalculator(DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }




        //TODO: reduce all possible combination by half using combnation instead of permutation
        public void CaculateTwoWay(HashSet<PHand> handRangeA, HashSet<PHand> handRangeB,
            out long[] win, out long[] tie, out long[] loss, out long total)
        {
            if (handRangeA == null || handRangeA.Count == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Count == 0)
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

        public void CalculateThreeWay(HashSet<PHand> handRangeA, HashSet<PHand> handRangeB, HashSet<PHand> handRangeC,
                                 out long[] win, out long[] tie, out long[] loss, out long total)
        {
            if (handRangeA == null || handRangeA.Count == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Count == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }

            if (handRangeC == null || handRangeC.Count == 0)
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



        //TODO: too much cast happening
        private void CaculateTwoWayConcrete(PHand handA, PHand handB,
             out int win1, out int win2,
            out int tie1, out int tie2, out int loss1, out int loss2, out int total)
        {
            long[] wins = new long[2];
            long[] ties = new long[2];
            long[] losses = new long[2];
            long totHand = 0;

            int index = handA.HandGroupIndex * 169 + handB.HandGroupIndex;
            wins = this.twoWayEquityArray[index].Wins;
            ties = this.twoWayEquityArray[index].Ties;
            losses = this.twoWayEquityArray[index].Losses;
            totHand = this.twoWayEquityArray[index].Total;

            win1 = (int)wins[0];
            win2 = (int)wins[1];
            tie1 = (int)ties[0];
            tie2 = (int)ties[1];
            loss1 = (int)losses[0];
            loss2 = (int)losses[1];
            total = (int)totHand;
        }

        public void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC,
            out int win1, out int win2, out int win3, out int tie1, out int tie2, out int tie3,
            out int loss1, out int loss2, out int loss3, out int total)
        {
            long[] wins = new long[3];
            long[] ties = new long[3];
            long[] losses = new long[3];
            long totHand = 0;

            int index = handA.HandGroupIndex * 169 + handB.HandGroupIndex * 169 + handC.HandGroupIndex;
            wins = this.threeWayEquityArray[index].Wins;
            ties = this.threeWayEquityArray[index].Ties;
            losses = this.threeWayEquityArray[index].Losses;
            totHand = this.threeWayEquityArray[index].Total;

            win1 = (int)wins[0];
            win2 = (int)wins[1];
            win3 = (int)wins[2];
            tie1 = (int)ties[0];
            tie2 = (int)ties[1];
            tie3 = (int)ties[2];
            loss1 = (int)losses[0];
            loss2 = (int)losses[1];
            loss3 = (int)losses[2];
            total = (int)totHand;
        }

    }
}
