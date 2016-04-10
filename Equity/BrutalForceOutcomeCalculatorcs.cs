using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;
using System.Diagnostics;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.Equity
{
    public class BrutalForceOutcomeCalculatorcs
    {

        private static BrutalForceOutcomeCalculatorcs instance;
        private readonly static object lockobj = new object();


        public BrutalForceOutcomeCalculatorcs()
        {
        }


        public static BrutalForceOutcomeCalculatorcs Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new BrutalForceOutcomeCalculatorcs();
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

                            int[] handOutcomes = new int[13];
                            int tot;
                            //if (handA.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handC.HandGroupIndex)
                            //{
                                //123
                                this.CalculateThreeWayConcrete(handA, handB, handC,
                                    out o111, out o113, out o131, out o122, out o123, out o132, out o212, out o221,
                                    out o213, out o231, out o311, out o312, out o321, out tot);
                            //}
                            //else if (handA.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handB.HandGroupIndex)
                            //{
                            //    //132
                            //    this.CalculateThreeWayConcrete(handA, handC, handB,
                            //        out o111, out o131, out o113, out o122, out o132, out o123, out o221, out o212,
                            //        out o231, out o213, out o311, out o321, out o312, out tot);

                            //}
                            //else if (handB.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handC.HandGroupIndex)
                            //{
                            //    //213
                            //    this.CalculateThreeWayConcrete(handB, handA, handC,
                            //        out o111, out o113, out o311, out o212, out o213, out o312, out o122, out o221,
                            //        out o123, out o321, out o131, out o132, out o231, out tot);
                            //}
                            //else if (handB.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handA.HandGroupIndex)
                            //{
                            //    //231
                            //    this.CalculateThreeWayConcrete(handB, handC, handA,
                            //        out o111, out o131, out o311, out o221, out o231, out o321, out o122, out o212,
                            //        out o132, out o312, out o113, out o123, out o213, out tot);
                            //}
                            //else if (handC.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handB.HandGroupIndex)
                            //{
                            //    //312
                            //    this.CalculateThreeWayConcrete(handC, handA, handB,
                            //        out o111, out o311, out o113, out o212, out o312, out o213, out o221, out o122,
                            //        out o321, out o123, out o131, out o231, out o132, out tot);
                            //}
                            //else if (handC.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handA.HandGroupIndex)
                            //{
                            //    //321
                            //    this.CalculateThreeWayConcrete(handC, handB, handA,
                            //        out o111, out o311, out o131, out o221, out o321, out o231, out o212, out o122,
                            //        out o312, out o132, out o113, out o213, out o123, out tot);
                            //}
                            //else
                            //{
                            //    throw new Exception("error occured here.");
                            //}


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

            long totalHands = 0;
            var outcomes = Hand.ThreeHandOutcomes(new[] {handA.FullHandStr, handB.FullHandStr, handC.FullHandStr},
                ref totalHands);

            o111 = checked((int)outcomes[0]);
            o113 = checked((int)outcomes[1]);
            o131 = checked((int)outcomes[2]);
            o122 = checked((int)outcomes[3]);
            o123 = checked((int)outcomes[4]);
            o132 = checked((int)outcomes[5]);
            o212 = checked((int)outcomes[6]);
            o221 = checked((int)outcomes[7]);
            o213 = checked((int)outcomes[8]);
            o231 = checked((int)outcomes[9]);
            o311 = checked((int)outcomes[10]);
            o312 = checked((int)outcomes[11]);
            o321 = checked((int)outcomes[12]);
            tot = checked((int) totalHands);
        }

    }
}
