using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.HtmlControls;
using GameTreeDraft.GameTree;
using GameTreeDraft.Hands;
using System.Collections.Generic;

namespace GameTreeDraft.Equity
{
    public enum DifferentialHandOp
    {
        Add,
        Remove
    }

    public class DifferentialRangeEquityCalculator
    {

        private readonly Dictionary<GameTreeNode, int> mapper;
        private bool isBaseSet = false;

        private SortedSet<PHand>[] sortedRanges;
        private List<PHand[]> sortedRangesArr;

        private RangeEquityResult currentResult;


        private long[] lastOutcomeDiff = null;
        private long totalHandDiff = 0;

        private DifferentialHandOp? lastHandOp = null;
        private PHand[] lastDiffHands = null;
        private int lastRangeIndex = 0;

        public DifferentialRangeEquityCalculator(Dictionary<GameTreeNode, int> rangeNodeIndexMapper)
        {
            this.mapper = rangeNodeIndexMapper;
        }

        public RangeEquityResult CaculateTwoWayBaseEquity(SortedSet<PHand> handRangeA, SortedSet<PHand> handRangeB)
        {
            if (isBaseSet)
            {
                throw new InvalidOperationException("base equity result already set");
            }
            isBaseSet = true;

            if (handRangeA == null || handRangeA.Count == 0)
            {
                throw new ArgumentException("handRangeA cannot be null or empty.");
            }

            if (handRangeB == null || handRangeB.Count == 0)
            {
                throw new ArgumentException("handRangeB cannot be null or empty.");
            }

            this.sortedRanges = new SortedSet<PHand>[2];
            this.sortedRanges[0] = handRangeA;
            this.sortedRanges[1] = handRangeB;

            this.sortedRangesArr = new List<PHand[]>();
            this.sortedRangesArr.Add(handRangeA.ToArray());
            this.sortedRangesArr.Add(handRangeB.ToArray());

            long totalHand;

            long[] outcomes = PreComputedOutcomeCaculator2Way.Instance.CaculateTwoWay(
                        handRangeA,
                        handRangeB,
                        out totalHand
                        );


            RangeEquityResult result = RangeEquityResult.CreateNew(outcomes, totalHand);
            this.currentResult = result;
            return result;
        }


        public RangeEquityResult CaculateThreeWayBaseEquity(SortedSet<PHand> handRangeA, SortedSet<PHand> handRangeB, SortedSet<PHand> handRangeC)
        {
            if (isBaseSet)
            {
                throw new InvalidOperationException("base equity result already set");
            }
            isBaseSet = true;

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

            this.sortedRanges = new SortedSet<PHand>[3];
            this.sortedRanges[0] = handRangeA;
            this.sortedRanges[1] = handRangeB;
            this.sortedRanges[2] = handRangeC;

            this.sortedRangesArr = new List<PHand[]>();
            this.sortedRangesArr.Add(handRangeA.ToArray());
            this.sortedRangesArr.Add(handRangeB.ToArray());
            this.sortedRangesArr.Add(handRangeC.ToArray());


            long totalHand;

            long[] outcomes = PreComputedOutcomeCalculator3Way.Instance.CalculateThreeWay(
                        handRangeA.ToArray(),
                        handRangeB.ToArray(),
                        handRangeC.ToArray(),
                        out totalHand
                        );


            RangeEquityResult result = RangeEquityResult.CreateNew(outcomes, totalHand);
            this.currentResult = result;
            return result;
        }

        public RangeEquityResult CalculateDifferentialEquity(GameTreeNode rangeNode, PHand[] hands, DifferentialHandOp handOp)
        {
            if (!isBaseSet)
            {
                throw new Exception("Base is not set");
            }
            int rangeIndex = this.mapper[rangeNode];
            if (rangeIndex < 0 || rangeIndex > 2)
            {
                throw new ArgumentOutOfRangeException("rangeIndex");
            }
            if (hands == null)
            {
                throw new ArgumentNullException("hands");
            }
            if (!Enum.IsDefined(typeof(DifferentialHandOp), handOp))
            {
                throw new InvalidEnumArgumentException("handOp");
            }


            long[] outcomesT = new long[this.sortedRanges.Length == 2 ? 3 : 13];
            long totalHandT = 0;

            string handsstr = string.Empty;
            foreach (PHand hand in hands)
            {
                long totalHand = 0;

                handsstr += hand.HandGroupString + " ";
                switch (handOp)
                {
                    case DifferentialHandOp.Add:
                        if (this.sortedRanges[rangeIndex].Contains(hand))
                        {
                            throw new Exception("hand already exist in the range");
                        }
                        this.sortedRanges[rangeIndex].Add(hand);
                        this.sortedRangesArr[rangeIndex] = this.sortedRanges[rangeIndex].ToArray();
                        break;

                    case DifferentialHandOp.Remove:
                        if (!this.sortedRanges[rangeIndex].Contains(hand))
                        {
                            throw new Exception("hand not exist in the range");
                        }
                        this.sortedRanges[rangeIndex].Remove(hand);
                        this.sortedRangesArr[rangeIndex] = this.sortedRanges[rangeIndex].ToArray();
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                long[] outcomes;
                //TODO. improve to use 1 operation only
                if (this.sortedRanges.Length == 2)
                {
                    outcomes = PreComputedOutcomeCaculator2Way.Instance.CaculateTwoWay(
                        rangeIndex == 0 ? new SortedSet<PHand> { hand } : sortedRanges[0],
                        rangeIndex == 1 ? new SortedSet<PHand> { hand } : sortedRanges[1],
                        out totalHand
                        );

                }
                else
                {

                    outcomes = PreComputedOutcomeCalculator3Way.Instance.CalculateThreeWay(
                           rangeIndex == 0 ? new[] { hand } : sortedRangesArr[0],
                           rangeIndex == 1 ? new[] { hand } : sortedRangesArr[1],
                           rangeIndex == 2 ? new[] { hand } : sortedRangesArr[2],
                           out totalHand
                           );
                }


                for (int i = 0; i < outcomesT.Length; i++)
                {
                    outcomesT[i] += outcomes[i];
                }
                totalHandT += totalHand;


            }

            this.lastOutcomeDiff = outcomesT;
            this.totalHandDiff = totalHandT;
            this.lastHandOp = handOp;
            this.lastDiffHands = hands;
            this.lastRangeIndex = rangeIndex;


            this.currentResult.PerformDifferentialOp(outcomesT, totalHandT, handOp);

            //Console.WriteLine("{0}: {1} {2} {3} {4} {5} {6}", rangeNode.Descripton.PadRight(5), handOp.ToString().PadRight(7), handsstr.PadRight(7),
            //    this.currentResult.Total.ToString().PadRight(15), this.currentResult.Outcomes[0].ToString().PadRight(15), this.currentResult.Outcomes[1].ToString().PadRight(15),
            //    this.currentResult.Outcomes[2].ToString().PadRight(15));
            return this.currentResult;
        }


        public RangeEquityResult CalculateLinearEquity(GameTreeNode rangeNode, PHand[] hands, DifferentialHandOp handOp)
        {
            if (!isBaseSet)
            {
                throw new Exception("Base is not set");
            }
            int rangeIndex = this.mapper[rangeNode];
            if (rangeIndex < 0 || rangeIndex > 2)
            {
                throw new ArgumentOutOfRangeException("rangeIndex");
            }
            if (hands == null)
            {
                throw new ArgumentNullException("hands");
            }
            if (!Enum.IsDefined(typeof(DifferentialHandOp), handOp))
            {
                throw new InvalidEnumArgumentException("handOp");
            }


            foreach (PHand hand in hands)
            {

                switch (handOp)
                {
                    case DifferentialHandOp.Add:
                        if (this.sortedRanges[rangeIndex].Contains(hand))
                        {
                            throw new Exception("hand already exist in the range");
                        }
                        this.sortedRanges[rangeIndex].Add(hand);
                        this.sortedRangesArr[rangeIndex] = this.sortedRanges[rangeIndex].ToArray();
                        break;

                    case DifferentialHandOp.Remove:
                        if (!this.sortedRanges[rangeIndex].Contains(hand))
                        {
                            throw new Exception("hand not exist in the range");
                        }
                        this.sortedRanges[rangeIndex].Remove(hand);
                        this.sortedRangesArr[rangeIndex] = this.sortedRanges[rangeIndex].ToArray();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            long totalHand = 0;

            if (this.sortedRanges.Length == 2)
            {
                var outcomes = PreComputedLinearRangeOutcomeCalculator.Instance.CaculateTwoWay(
                          this.sortedRangesArr[0],
                          this.sortedRangesArr[1],
                          out totalHand);

                this.currentResult.Outcomes = outcomes;
                this.currentResult.Total = totalHand;

            }
            else if (this.sortedRanges.Length == 3)
            {
                var outcomes = PreComputedLinearRangeOutcomeCalculator.Instance.CalculateThreeWay(
                          this.sortedRangesArr[0],
                          this.sortedRangesArr[1],
                          this.sortedRangesArr[2],
                          out totalHand);

                this.currentResult.Outcomes = outcomes;
                this.currentResult.Total = totalHand;

            }
            else
            {
                throw new NotSupportedException();
            }


            this.lastHandOp = handOp;
            this.lastDiffHands = hands;
            this.lastRangeIndex = rangeIndex;

            return this.currentResult;
        }


        internal void RevertLastDiffOp(GameTreeNode rangeNode, PHand[] hands, DifferentialHandOp handOp)
        {
            if (this.lastHandOp == null)
            {
                throw new InvalidOperationException("Last hand op does not exist");
            }

            int rangeIndex = this.mapper[rangeNode];
            var revertOp = this.lastHandOp == DifferentialHandOp.Add
                ? DifferentialHandOp.Remove
                : DifferentialHandOp.Add;

            Debug.Assert(rangeIndex == this.lastRangeIndex);
            Debug.Assert(hands[0] == this.lastDiffHands[0]);
            Debug.Assert(handOp == revertOp);

            string handsstr = string.Empty;
            foreach (PHand hand in this.lastDiffHands)
            {
                handsstr += hand.HandGroupString + " ";
                switch (revertOp)
                {
                    case DifferentialHandOp.Add:
                        if (this.sortedRanges[this.lastRangeIndex].Contains(hand))
                        {
                            throw new Exception("hand already exist in the range");
                        }
                        this.sortedRanges[this.lastRangeIndex].Add(hand);
                        this.sortedRangesArr[this.lastRangeIndex] = this.sortedRanges[this.lastRangeIndex].ToArray();
                        break;

                    case DifferentialHandOp.Remove:
                        if (!this.sortedRanges[this.lastRangeIndex].Contains(hand))
                        {
                            throw new Exception("hand not exist in the range");
                        }
                        this.sortedRanges[this.lastRangeIndex].Remove(hand);
                        this.sortedRangesArr[this.lastRangeIndex] = this.sortedRanges[this.lastRangeIndex].ToArray();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            this.currentResult.PerformDifferentialOp(this.lastOutcomeDiff, this.totalHandDiff, revertOp);

            //Console.WriteLine("{0}: {1}R {2} {3} {4} {5} {6}", rangeNode.Descripton.PadRight(5), handOp.ToString().PadRight(6), handsstr.PadRight(7),
            //    this.currentResult.Total.ToString().PadRight(15), this.currentResult.Outcomes[0].ToString().PadRight(15), this.currentResult.Outcomes[1].ToString().PadRight(15),
            //    this.currentResult.Outcomes[2].ToString().PadRight(15));
            this.lastOutcomeDiff = null;
            this.totalHandDiff = 0;
            this.lastHandOp = null;
            this.lastDiffHands = null;
            this.lastRangeIndex = 0;
        }


        internal void RevertLastLinearOp(GameTreeNode rangeNode, PHand[] hands, DifferentialHandOp handOp)
        {
            if (this.lastHandOp == null)
            {
                throw new InvalidOperationException("Last hand op does not exist");
            }

            int rangeIndex = this.mapper[rangeNode];
            var revertOp = this.lastHandOp == DifferentialHandOp.Add
                ? DifferentialHandOp.Remove
                : DifferentialHandOp.Add;

            Debug.Assert(rangeIndex == this.lastRangeIndex);
            Debug.Assert(hands[0] == this.lastDiffHands[0]);
            Debug.Assert(handOp == revertOp);

            foreach (PHand hand in this.lastDiffHands)
            {
                switch (revertOp)
                {
                    case DifferentialHandOp.Add:
                        if (this.sortedRanges[this.lastRangeIndex].Contains(hand))
                        {
                            throw new Exception("hand already exist in the range");
                        }
                        this.sortedRanges[this.lastRangeIndex].Add(hand);
                        this.sortedRangesArr[this.lastRangeIndex] = this.sortedRanges[this.lastRangeIndex].ToArray();
                        break;

                    case DifferentialHandOp.Remove:
                        if (!this.sortedRanges[this.lastRangeIndex].Contains(hand))
                        {
                            throw new Exception("hand not exist in the range");
                        }
                        this.sortedRanges[this.lastRangeIndex].Remove(hand);
                        this.sortedRangesArr[this.lastRangeIndex] = this.sortedRanges[this.lastRangeIndex].ToArray();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            this.lastOutcomeDiff = null;
            this.totalHandDiff = 0;
            this.lastHandOp = null;
            this.lastDiffHands = null;
            this.lastRangeIndex = 0;
        }
    }
}
