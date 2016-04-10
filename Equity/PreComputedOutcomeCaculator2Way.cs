using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class PreComputedOutcomeCaculator2Way
    {

        private static PreComputedOutcomeCaculator2Way instance;
        private readonly static object lockobj = new object();

        private readonly HandOutcomes[] twoWayOutcomeArray = new HandOutcomes[169 * 169];

        public PreComputedOutcomeCaculator2Way(string path)
        {
            string defaultPath = Path.Combine(path, @"169HandOutcomes2Way.dat");
            FileInfo fi = new FileInfo(defaultPath);
            using (FileStream fs = fi.OpenRead())
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                    {
                        for (int j = 0; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                        {
                            this.twoWayOutcomeArray[i * 169 + j] = new HandOutcomes()
                            {

                                Outcomes = new[] { br.ReadInt64(), br.ReadInt64(), br.ReadInt64() },
                                Total = br.ReadInt64()
                            };
                        }
                    }
                }
            }
        }


        public static string DefaultPath { get; set; }

        public static PreComputedOutcomeCaculator2Way Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new PreComputedOutcomeCaculator2Way(DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }




        //TODO: reduce all possible combination by half using combnation instead of permutation
        public long[] CaculateTwoWay(SortedSet<PHand> handRangeA, SortedSet<PHand> handRangeB, out long total)
        {
            if (handRangeA == null || handRangeA.Count == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Count == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }
            long[] outcomes = new long[] { 0, 0, 0 };
            total = 0;

            foreach (PHand handA in handRangeA)
            {
                foreach (PHand handB in handRangeB)
                {
                    long o11, o12, o21, tot = 0;
                    if (handA.HandIndex <= handB.HandIndex)
                    {
                        this.CaculateTwoWayConcrete(handA, handB, out o11, out o12, out o21, out tot);
                    }
                    else
                    {
                        this.CaculateTwoWayConcrete(handB, handA, out o11, out o21, out o12, out tot);
                    }

                    total += tot;

                    outcomes[0] += o11;
                    outcomes[1] += o12;
                    outcomes[2] += o21;

                    Debug.Assert((o11 + o12 + o21) == tot);
                }
            }


            return outcomes;
        }

        //TODO: too much cast happening
        private void CaculateTwoWayConcrete(PHand handA, PHand handB, out long o11, out long o12, out long o21, out long total)
        {

            int index = handA.HandGroupIndex * 169  + handB.HandGroupIndex;
            o11 = this.twoWayOutcomeArray[index].Outcomes[0];
            o12 = this.twoWayOutcomeArray[index].Outcomes[1];
            o21 = this.twoWayOutcomeArray[index].Outcomes[2];
            total = this.twoWayOutcomeArray[index].Total;

        }
    }
}
