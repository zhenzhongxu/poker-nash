using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using GameTreeDraft.GameTree;
using GameTreeDraft.Hands;
using System.Diagnostics;
using GameTreeDraft.Utility;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.Equity
{
    public unsafe class PreComputedOutcomeCalculator3Way
    {

        public enum OutComeSequence
        {
            ABC,
            ACB,
            BAC,
            BCA,
            CAB,
            CBA


        }

        private static PreComputedOutcomeCalculator3Way instance;
        private readonly static object lockobj = new object();

        //private static readonly long[] threeWayOutcomesRawArray; //= new long[169 * 169 * 169 * 14];
        private static readonly int[] threeWayOutcomesRawArrayLoopA; // = new int[818805];
        private static readonly int[] threeWayOutcomesRawArrayLoopB;
        private static readonly int[] threeWayOutcomesRawArrayLoopC;

        private static readonly LoopCCombinationIndexer indexerC;
        private static readonly LoopBCombinationIndexer indexerB;
        private static readonly LoopACombinationIndexer indexerA;


        static PreComputedOutcomeCalculator3Way()
        {
            indexerC = new LoopCCombinationIndexer();
            indexerB = new LoopBCombinationIndexer();
            indexerA = new LoopACombinationIndexer();


            string defaultPath = Path.Combine(Globals.DefaultPath, @"169HandOutcomes3Way_LoopA.dat");
            FileInfo fi = new FileInfo(defaultPath);
            threeWayOutcomesRawArrayLoopA = Utils.ReadFileIntoIntArray(fi);

            defaultPath = Path.Combine(Globals.DefaultPath, @"169HandOutcomes3Way_LoopB.dat");
            fi = new FileInfo(defaultPath);
            threeWayOutcomesRawArrayLoopB = Utils.ReadFileIntoIntArray(fi);

            defaultPath = Path.Combine(Globals.DefaultPath, @"169HandOutcomes3Way_LoopC.dat");
            fi = new FileInfo(defaultPath);
            threeWayOutcomesRawArrayLoopC = Utils.ReadFileIntoIntArray(fi);
        }

        public PreComputedOutcomeCalculator3Way(string path)
        {

        }



        public static PreComputedOutcomeCalculator3Way Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockobj)
                    {
                        if (instance == null)
                        {
                            instance = new PreComputedOutcomeCalculator3Way(Globals.DefaultPath);
                        }
                    }
                }
                return instance;
            }
        }




        //TODO: reduce all possible combination by half using combnation instead of permutation
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
                        long o111, o113, o131, o122, o123, o132, o212, o221, o213, o231, o311, o312, o321, tot;

                        if (handA.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handC.HandGroupIndex)
                        {
                            //ABC

                            CalculateThreeWayConcrete(OutComeSequence.ABC, threeWayOutcomesRawArrayLoopC, handA, handB, handC,
                                out o111, out o113, out o131, out o122, out o123, out o132, out o212, out o221,
                                out o213, out o231, out o311, out o312, out o321, out tot);

                        }
                        else if (handA.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handB.HandGroupIndex)
                        {
                            //ACB

                            CalculateThreeWayConcrete(OutComeSequence.ACB, threeWayOutcomesRawArrayLoopB, handA, handC, handB,
                                out o111, out o131, out o113, out o122, out o132, out o123, out o221, out o212,
                                out o231, out o213, out o311, out o321, out o312, out tot);


                        }
                        else if (handB.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handC.HandGroupIndex)
                        {
                            //BAC

                            CalculateThreeWayConcrete(OutComeSequence.BAC, threeWayOutcomesRawArrayLoopC, handB, handA, handC,
                                out o111, out o113, out o311, out o212, out o213, out o312, out o122, out o221,
                                out o123, out o321, out o131, out o132, out o231, out tot);

                        }
                        else if (handB.HandGroupIndex <= handC.HandGroupIndex && handC.HandGroupIndex <= handA.HandGroupIndex)
                        {
                            //BCA

                            CalculateThreeWayConcrete(OutComeSequence.BCA, threeWayOutcomesRawArrayLoopB, handB, handC, handA,
                                out o111, out o311, out o113, out o212, out o312, out o213, out o221, out o122,
                                out o321, out o123, out o131, out o231, out o132, out tot);

                        }
                        else if (handC.HandGroupIndex <= handA.HandGroupIndex && handA.HandGroupIndex <= handB.HandGroupIndex)
                        {
                            //CAB

                            CalculateThreeWayConcrete(OutComeSequence.CAB, threeWayOutcomesRawArrayLoopA, handC, handA, handB,
                                out o111, out o131, out o311, out o221, out o231, out o321, out o122, out o212,
                                out o132, out o312, out o113, out o123, out o213, out tot);

                        }
                        else if (handC.HandGroupIndex <= handB.HandGroupIndex && handB.HandGroupIndex <= handA.HandGroupIndex)
                        {
                            //CBA

                            CalculateThreeWayConcrete(OutComeSequence.CBA, threeWayOutcomesRawArrayLoopA, handC, handB, handA,
                                out o111, out o311, out o131, out o221, out o321, out o231, out o212, out o122,
                                out o312, out o132, out o113, out o213, out o123, out tot);

                        }
                        else
                        {
                            throw new Exception("error occured here.");
                        }


                        total += tot;

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

                        Debug.Assert((o111 + o113 + o131 + o122 + o123 + o132 + o212 + o221 + o213 + o231 + o311 + o312 + o321) == tot);
                    }
                }
            }


            return outcomes;
        }

        //TODO: too much cast happening

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe static void CalculateThreeWayConcrete(OutComeSequence pattern, int[] array, PHand handA, PHand handB, PHand handC,
            out long o111,
            out long o113,
            out long o131,
            out long o122,
            out long o123,
            out long o132,
            out long o212,
            out long o221,
            out long o213,
            out long o231,
            out long o311,
            out long o312,
            out long o321,
            out long total)
        {

            //Console.WriteLine("{0}: {1} {2} {3}", pattern, handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);

            ////int key = handA.HandGroupIndex*28561 + handB.HandGroupIndex*169 + handC.HandGroupIndex;



            //Console.WriteLine("{0} {1} {2}", handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
            int index;
            switch (pattern)
            {
                case OutComeSequence.ABC:
                    index = indexerC.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                case OutComeSequence.BAC:
                    index = indexerC.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                case OutComeSequence.ACB:
                    index = indexerB.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                case OutComeSequence.BCA:
                    index = indexerB.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                case OutComeSequence.CAB:
                    index = indexerA.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                case OutComeSequence.CBA:
                    index = indexerA.GetIndex(handA.HandGroupIndex, handB.HandGroupIndex, handC.HandGroupIndex);
                    break;
                default:
                    throw new ArgumentException("invalid");
            }

            index = index * 13;
            total = 0;
            fixed (int* arrayPtr = array)
            {
                o111 = *(arrayPtr + index);
                total += o111;
                o113 = *(arrayPtr + index + 1);
                total += o113;
                o131 = *(arrayPtr + index + 2);
                total += o131;
                o122 = *(arrayPtr + index + 3);
                total += o122;
                o123 = *(arrayPtr + index + 4);
                total += o123;
                o132 = *(arrayPtr + index + 5);
                total += o132;
                o212 = *(arrayPtr + index + 6);
                total += o212;
                o221 = *(arrayPtr + index + 7);
                total += o221;
                o213 = *(arrayPtr + index + 8);
                total += o213;
                o231 = *(arrayPtr + index + 9);
                total += o231;
                o311 = *(arrayPtr + index + 10);
                total += o311;
                o312 = *(arrayPtr + index + 11);
                total += o312;
                o321 = *(arrayPtr + index + 12);
                total += o321;
                //total = (long)o111 + o113 + o131 + o122 + o123 + o132 + o212 + o221 + o213 + o231 + o311 + o312 + o321;
            }

            //int idx = (handA.HandGroupIndex * 28561 + handB.HandGroupIndex * 169 + handC.HandGroupIndex) * 14;
            //fixed (long* arrayPtr = threeWayOutcomesRawArray)
            //{

            //    var a111 = *(arrayPtr + idx);
            //    var a113 = *(arrayPtr + idx + 1);
            //    var a131 = *(arrayPtr + idx + 2);
            //    var a122 = *(arrayPtr + idx + 3);
            //    var a123 = *(arrayPtr + idx + 4);
            //    var a132 = *(arrayPtr + idx + 5);
            //    var a212 = *(arrayPtr + idx + 6);
            //    var a221 = *(arrayPtr + idx + 7);
            //    var a213 = *(arrayPtr + idx + 8);
            //    var a231 = *(arrayPtr + idx + 9);
            //    var a311 = *(arrayPtr + idx + 10);
            //    var a312 = *(arrayPtr + idx + 11);
            //    var a321 = *(arrayPtr + idx + 12);
            //    var atot = *(arrayPtr + idx + 13);

            //    Debug.Assert(a111 == o111);
            //    Debug.Assert(a113 == o113);
            //    Debug.Assert(a131 == o131);
            //    Debug.Assert(a122 == o122);
            //    Debug.Assert(a123 == o123);
            //    Debug.Assert(a132 == o132);
            //    Debug.Assert(a212 == o212);
            //    Debug.Assert(a221 == o221);
            //    Debug.Assert(a213 == o213);
            //    Debug.Assert(a231 == o231);
            //    Debug.Assert(a311 == o311);
            //    Debug.Assert(a312 == o312);
            //    Debug.Assert(a321 == o321);
            //    Debug.Assert(atot == total);


            //}
        }


        //private static void CalculateThreeWayConcreteAlter(PHand handA, PHand handB, PHand handC,
        //    out long o111,
        //    out long o113,
        //    out long o131,
        //    out long o122,
        //    out long o123,
        //    out long o132,
        //    out long o212,
        //    out long o221,
        //    out long o213,
        //    out long o231,
        //    out long o311,
        //    out long o312,
        //    out long o321,
        //    out long total)
        //{

        //    //int key = handA.HandGroupIndex*28561 + handB.HandGroupIndex*169 + handC.HandGroupIndex;
        //    int index = (handA.HandGroupIndex * 28561 + handB.HandGroupIndex * 169 + handC.HandGroupIndex) * 14;

        //    o111 = threeWayOutcomesRawArray[index];
        //    o113 = threeWayOutcomesRawArray[index + 1];
        //    o131 = threeWayOutcomesRawArray[index + 2];
        //    o122 = threeWayOutcomesRawArray[index + 3];
        //    o123 = threeWayOutcomesRawArray[index + 4];
        //    o132 = threeWayOutcomesRawArray[index + 5];
        //    o212 = threeWayOutcomesRawArray[index + 6];
        //    o221 = threeWayOutcomesRawArray[index + 7];
        //    o213 = threeWayOutcomesRawArray[index + 8];
        //    o231 = threeWayOutcomesRawArray[index + 9];
        //    o311 = threeWayOutcomesRawArray[index + 10];
        //    o312 = threeWayOutcomesRawArray[index + 11];
        //    o321 = threeWayOutcomesRawArray[index + 12];
        //    total = threeWayOutcomesRawArray[index + 13];

        //    Debug.Assert(index + 13 < threeWayOutcomesRawArray.Length);
        //}

    }
}
