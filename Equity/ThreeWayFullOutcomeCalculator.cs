using System.IO;
using System.Web.UI;
using GameTreeDraft.Hands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GameTreeDraft.Equity
{
    public class HandOutcomes
    {
        public long[] Outcomes { get; set; }

        public long Total { get; set; }

    }

    public class ThreeWayFullOutcomeCalculator
    {

        private static ThreeWayFullOutcomeCalculator instance;
        private readonly static object lockobj = new object();

        private readonly Dictionary<PHand, Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>> threeWayOutcomeLut =
            new Dictionary<PHand, Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>>();

        private readonly int[] threeWayRawData = new int[227214624];

        public ThreeWayFullOutcomeCalculator(string path)
        {
            string defaultPath = Path.Combine(path, @"3WayFullCollisonOutcomes.dat");
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
                this.threeWayOutcomeLut.Add(handA, new Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>());
                for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                {
                    PHand handB = HandRange.Instance.Hand169RangeAll[j];
                    this.threeWayOutcomeLut[handA].Add(handB, new Dictionary<PHand, Dictionary<int, int>>());

                    for (int k = j; k < HandRange.Instance.Hand169RangeAll.Count; k++)
                    {
                        PHand handC = HandRange.Instance.Hand169RangeAll[k];
                        this.threeWayOutcomeLut[handA][handB].Add(handC, new Dictionary<int, int>());

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
                                this.threeWayOutcomeLut[handA][handB][handC].Add(hash, index);

                                index++;
                            }
                        }
                    }
                }
            }
        }

        public static string DefaultPath { get; set; }

        public static ThreeWayFullOutcomeCalculator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new ThreeWayFullOutcomeCalculator(DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }

        public long[] CalculateThreeWay(PHand[] handRangeA, PHand[] handRangeB, PHand[] handRangeC, out long total)
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
            long[] outcomes = new long[13];
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

                            int o111, o113, o131, o122, o123, o132, o212, o221, o213, o231, o311, o312, o321;

                            int tot;
                            if (handA.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handC.HandGroupIndex)
                            {
                                //ABC
                                this.CalculateThreeWayConcrete(handA, handB, handC,
                                    out o111, out o113, out o131, out o122, out o123, out o132, out o212, out o221,
                                    out o213, out o231, out o311, out o312, out o321, out tot);
                            }
                            else if (handA.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handB.HandGroupIndex)
                            {
                                //ACB
                                this.CalculateThreeWayConcrete(handA, handC, handB,
                                    out o111, out o131, out o113, out o122, out o132, out o123, out o221, out o212,
                                    out o231, out o213, out o311, out o321, out o312, out tot);

                            }
                            else if (handB.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handC.HandGroupIndex)
                            {
                                //BAC
                                this.CalculateThreeWayConcrete(handB, handA, handC,
                                    out o111, out o113, out o311, out o212, out o213, out o312, out o122, out o221,
                                    out o123, out o321, out o131, out o132, out o231, out tot);
                            }
                            else if (handB.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handA.HandGroupIndex)
                            {
                                //BCA
                                this.CalculateThreeWayConcrete(handB, handC, handA,
                                    out o111, out o311, out o113, out o212, out o312, out o213, out o221, out o122,
                                    out o321, out o123, out o131, out o231, out o132, out tot);
                            }
                            else if (handC.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handB.HandGroupIndex)
                            {
                                //CAB
                                this.CalculateThreeWayConcrete(handC, handA, handB,
                                    out o111, out o131, out o311, out o221, out o231, out o321, out o122, out o212,
                                    out o132, out o312, out o113, out o123, out o213, out tot);
                            }
                            else if (handC.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handA.HandGroupIndex)
                            {
                                //321
                                this.CalculateThreeWayConcrete(handC, handB, handA,
                                    out o111, out o311, out o131, out o221, out o321, out o231, out o212, out o122,
                                    out o312, out o132, out o113, out o213, out o123, out tot);
                            }
                            else
                            {
                                throw new Exception("error occured here.");
                            }


                            outcomes[0] += o111;
                            outcomes[1] += o113;
                            outcomes[2] += o131;
                            outcomes[3] += o122;
                            outcomes[4] += o123;
                            outcomes[5] += o132;
                            outcomes[6] += o212;
                            outcomes[7] += o221;
                            outcomes[8] += o213;
                            outcomes[9] += o231;
                            outcomes[10] += o311;
                            outcomes[11] += o312;
                            outcomes[12] += o321;
                            total += tot;
                        }
                    }
                }
            }

            return outcomes;
        }


        private void CalculateThreeWayConcrete(PHand handA, PHand handB, PHand handC,
            out int o111, out int o113, out int o131, out int o122, out int o123, out int o132,
            out int o212, out int o221, out int o213, out int o231, out int o311, out int o312,
             out int o321, out int tot)
        {
            var hash = ThreeWayHandHash.Instance.GetHash(handA, handB, handC);

            int index =
                this.threeWayOutcomeLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][
                    handC.HandGroupRepresentative][hash] * 14;

            o111 = this.threeWayRawData[index];
            o113 = this.threeWayRawData[index + 1];
            o131 = this.threeWayRawData[index + 2];
            o122 = this.threeWayRawData[index + 3];
            o123 = this.threeWayRawData[index + 4];
            o132 = this.threeWayRawData[index + 5];
            o212 = this.threeWayRawData[index + 6];
            o221 = this.threeWayRawData[index + 7];
            o213 = this.threeWayRawData[index + 8];
            o231 = this.threeWayRawData[index + 9];
            o311 = this.threeWayRawData[index + 10];
            o312 = this.threeWayRawData[index + 11];
            o321 = this.threeWayRawData[index + 12];
            tot = this.threeWayRawData[index + 13];
        }

    }
}
