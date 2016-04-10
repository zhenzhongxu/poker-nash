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
    public class TwoWayFullOutcomeCalculator
    {

        private static TwoWayFullOutcomeCalculator instance;
        private readonly static object lockobj = new object();


        private readonly Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>> twoWayOutcomeLut = 
            new Dictionary<PHand, Dictionary<PHand, Dictionary<int, int>>>();

        private readonly int[] twoWayRawData = new int[203632];

        public TwoWayFullOutcomeCalculator(string path)
        {
            string defaultPath = Path.Combine(path, @"2WayFullCollisionOutcomes.dat");
            FileInfo fi = new FileInfo(defaultPath);

            int index = 0;
            try
            {
                using (FileStream fs = fi.OpenRead())
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        while (index < this.twoWayRawData.Length)
                        {
                            twoWayRawData[index] = br.ReadInt32();
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
                this.twoWayOutcomeLut.Add(handA, new Dictionary<PHand, Dictionary<int, int>>());
                for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                {
                    PHand handB = HandRange.Instance.Hand169RangeAll[j];
                    this.twoWayOutcomeLut[handA].Add(handB, new Dictionary<int, int>());

                        var hashes = TwoWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB);

                        foreach (int hash in hashes)
                        {
                            PHand testHandA, testHandB, testHandC;

                            TwoWayHandHash.Instance.GenerateHandFromHash(handA.FirstRank, handA.SecondRank,
                                        handB.FirstRank,
                                        handB.SecondRank,
                                        hash, out testHandA, out testHandB);


                            if (!HandUtility.HasConflict(testHandA, testHandB))
                            {
                                this.twoWayOutcomeLut[handA][handB].Add(hash, index);

                                index++;
                            }
                        }
                }
            }
        }

        public static string DefaultPath { get; set; }

        public static TwoWayFullOutcomeCalculator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new TwoWayFullOutcomeCalculator(DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }

        public long[] CalculateTwoWay(PHand[] handRangeA, PHand[] handRangeB, out long total)
        {
            if (handRangeA == null || handRangeA.Length == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Length == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }

           
            long[] outcomes = new long[3];
            total = 0;

            foreach (PHand handA in handRangeA)
            {
                foreach (PHand handB in handRangeB)
                {
                   
                        if (!HandUtility.HasConflict(handA, handB))
                        {
                            Debug.Assert(handA.HandIndex != handB.HandIndex);

                            int o11, o12, o21;
                            int tot;
                            if (handA.HandIndex <= handB.HandIndex )
                            {
                                //12
                                this.CalculateTwoWayConcrete(handA, handB, out o11, out o12, out o21, out tot);
                            }
                            else 
                            {
                                //21
                                this.CalculateTwoWayConcrete(handB, handA, out o11, out o21, out o12, out tot);

                            }
                            

                            outcomes[0] += o11;
                            outcomes[1] += o12;
                            outcomes[2] += o21;
                            total += tot;
                        }
                }
            }

            return outcomes;
        }


        private void CalculateTwoWayConcrete(PHand handA, PHand handB, 
            out int o11, out int o12, out int o21, out int tot)
        {
            var hash = TwoWayHandHash.Instance.GetHash(handA, handB);

            int index =
                this.twoWayOutcomeLut[handA.HandGroupRepresentative][handB.HandGroupRepresentative][hash] * 4;

            o11 = this.twoWayRawData[index];
            o12 = this.twoWayRawData[index + 1];
            o21 = this.twoWayRawData[index + 2];
            tot = this.twoWayRawData[index + 3];
        }

    }
}
