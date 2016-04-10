using System;
using System.Collections.Generic;
using System.IO;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class HandEquity
    {
        public long[] Wins { get; set; }

        public long[] Ties { get; set; }

        public long[] Losses { get; set; }

        public long Total { get; set; }

    }


    public class PreComputedEquityCalculator : EquityCaculatorBase
    {

        private static PreComputedEquityCalculator instance;
        private readonly static object lockobj = new object();

        private readonly Dictionary<PHand, Dictionary<PHand, Dictionary<int, HandEquity>>> twoWayEquityLut =
            new Dictionary<PHand, Dictionary<PHand, Dictionary<int, HandEquity>>>();

        public PreComputedEquityCalculator(string path)
        {
            string defaultPath = Path.Combine(path, @"2wayCollisonFullEquity.dat");
            FileInfo fi = new FileInfo(defaultPath);
            using (FileStream fs = fi.OpenRead())
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                    {
                        PHand handA = HandRange.Instance.Hand169RangeAll[i];
                        this.twoWayEquityLut.Add(handA, new Dictionary<PHand, Dictionary<int, HandEquity>>());
                        for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                        {
                            PHand handB = HandRange.Instance.Hand169RangeAll[j];
                            this.twoWayEquityLut[handA].Add(handB, new Dictionary<int, HandEquity>());

                            var hashes = TwoWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB);

                            foreach (int hash in hashes)
                            {
                                this.twoWayEquityLut[handA][handB].Add(hash, new HandEquity()
                                {
                                    Wins = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Ties = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Losses = new[] { br.ReadInt64(), br.ReadInt64() },
                                    Total = br.ReadInt64()
                                });
                            }
                        }
                    }
                }
            }

        }

        public static string DefaultPath { get; set; }

        public static PreComputedEquityCalculator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new PreComputedEquityCalculator(DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }

        public override void CaculateTwoWayConcrete(PHand handA, PHand handB,
             out int win1, out int win2,
            out int tie1, out int tie2, out int loss1, out int loss2, out int total)
        {
            long[] wins = new long[2];
            long[] ties = new long[2];
            long[] losses = new long[2];
            long totHand = 0;
            var hash = TwoWayHandHash.Instance.GetHash(handA, handB);

            wins = this.twoWayEquityLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][hash].Wins;
            ties = this.twoWayEquityLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][hash].Ties;
            losses = this.twoWayEquityLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][hash].Losses;
            totHand = this.twoWayEquityLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][hash].Total;

            win1 = (int)wins[0];
            win2 = (int)wins[1];
            tie1 = (int)ties[0];
            tie2 = (int)ties[1];
            loss1 = (int)losses[0];
            loss2 = (int)losses[1];
            total = (int)totHand;
        }

        public override void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC,
            out int win1, out int win2, out int win3, out int tie1, out int tie2, out int tie3,
            out int loss1, out int loss2, out int loss3, out int total)
        {
            throw new NotImplementedException();
        }

    }
}
