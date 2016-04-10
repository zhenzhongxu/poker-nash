using System;
using System.Collections.Generic;
using System.IO;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class PreComputedEquityCalculator3Way : EquityCaculatorBase
    {

        private static PreComputedEquityCalculator3Way instance;
        private readonly static object lockobj = new object();

        private readonly Dictionary<PHand, Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>> threeWayEquityLut =
            new Dictionary<PHand, Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>>();

        private readonly int[] threeWayRawData = new int[162296160];

        public PreComputedEquityCalculator3Way(string path)
        {
            string defaultPath = Path.Combine(path, @"3waycollisionfullequity_1.dat");
            FileInfo fi = new FileInfo(defaultPath);

            int index = 0;
            try
            {
                using (FileStream fs = fi.OpenRead())
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        while (index < this.threeWayRawData.Length)
                        {
                            threeWayRawData[index] = br.ReadInt32();
                            index++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Please try again later, data file is still being downloaded.");
            }

            index = 0;
            for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
            {
                PHand handA = HandRange.Instance.Hand169RangeAll[i];
                this.threeWayEquityLut.Add(handA, new Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>());
                for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                {
                    PHand handB = HandRange.Instance.Hand169RangeAll[j];
                    this.threeWayEquityLut[handA].Add(handB, new Dictionary<PHand, Dictionary<int, int>>());

                    for (int k = j; k < HandRange.Instance.Hand169RangeAll.Count; k++)
                    {
                        PHand handC = HandRange.Instance.Hand169RangeAll[k];
                        this.threeWayEquityLut[handA][handB].Add(handC, new Dictionary<int, int>());

                        var hashes = ThreeWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB, handC);

                        foreach (int hash in hashes)
                        {
                            PHand testHandA, testHandB, testHandC;

                            ThreeWayHandHash.Instance.GenerateHandFromHash(handA.FirstRank, handA.SecondRank,
                                        handB.FirstRank,
                                        handB.SecondRank,
                                        handC.FirstRank,
                                        handC.SecondRank,
                                        hash, out testHandA, out testHandB, out testHandC);


                            if (!HandUtility.HasConflict(testHandA, testHandB, testHandC))
                            {
                                this.threeWayEquityLut[handA][handB][handC].Add(hash, index);

                                index++;
                            }
                        }
                    }
                }
            }
        }

        public static string DefaultPath { get; set; }

        public static PreComputedEquityCalculator3Way Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new PreComputedEquityCalculator3Way(DefaultPath);
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
            throw new NotImplementedException();
        }

        public override void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC,
            out int win1, out int win2, out int win3, out int tie1, out int tie2, out int tie3,
            out int loss1, out int loss2, out int loss3, out int total)
        {
            var hash = ThreeWayHandHash.Instance.GetHash(handA, handB, handC);

            int index =
                this.threeWayEquityLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][
                    handC.HandGroupRepresentative][hash] * 10;

            win1 = this.threeWayRawData[index];
            win2 = this.threeWayRawData[index + 1];
            win3 = this.threeWayRawData[index + 2];
            tie1 = this.threeWayRawData[index + 3];
            tie2 = this.threeWayRawData[index + 4];
            tie3 = this.threeWayRawData[index + 5];
            loss1 = this.threeWayRawData[index + 6];
            loss2 = this.threeWayRawData[index + 7];
            loss3 = this.threeWayRawData[index + 8];
            total = this.threeWayRawData[index + 9];
        }

    }
}
