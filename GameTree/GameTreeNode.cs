using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GameTreeDraft.Hands;
using GameTreeDraft.Equity;
using GameTreeDraft.ICM;

namespace GameTreeDraft.GameTree
{
    public enum NodeType
    {
        Push,
        Fold
    }

    public enum EndScenario : int
    {
        F11 = 1,
        F12 = 2,
        F21 = 3,
        F111 = 10,
        F113 = 11,
        F131 = 12,
        F122 = 13,
        F123 = 14,
        F132 = 15,
        F212 = 16,
        F221 = 17,
        F213 = 18,
        F231 = 19,
        F311 = 20,
        F312 = 21,
        F321 = 22,
        //P12,
        //P21,
        //P11,
        //P111,
        //P113,
        //P131,
        //P122,
        //P123,
        //P132,
        //P212,
        //P221,
        //P213,
        //P231,
        //P311,
        //P312,
        //P321
    }

    public class GameTreeNode
    {

        public static int GlobalId = 0;
        public bool LinearRangeStablized { get; set; }
        public bool UnrestrictedRangeStablized { get; set; }

        public readonly List<PHand> LastAdditions = new List<PHand>();
        public readonly List<PHand> LastRemovals = new List<PHand>();

        public string Descripton
        {
            get
            {
                if (this.Position != null)
                {
                    return String.Format("{0}:{1}", this.Position.Positon, this.NodeId);
                }
                else
                {
                    if (PreviousAllInCount == 0)
                    {
                        return string.Format("End ({0})", this.NodeId);
                    }
                    else
                    {
                        return String.Format("AI{0}W ({1})", PreviousAllInCount, this.NodeId);
                    }
                }
            }
        }
        public GameInfo HandInfo
        {
            get;
            private set;
        }

        public int NodeId { get; private set; }

        public Position Position { get; private set; }

        public GameTreeNode FoldBranch { get; set; }

        public GameTreeNode PushBranch { get; set; }

        public int PreviousAllInCount { get; set; }

        public double PreviousMaxMallIn { get; set; }

        public IRange Range { get; set; }

        public double OriginalStack { get; private set; }
        public double PutInPot { get; internal set; }

        public double MandatoryPutInPot { get; private set; }

        public RangeEquityResult RangeEquityResult { get; set; }

        public bool IsTerminal
        {
            get { return this.Position == null; }
        }

        public Dictionary<EndScenario, double[]> PossibleEndStacks { get; private set; }

        public double[] CurrentEquity { get; private set; }


        public GameTreeNode(GameInfo gi, Position pos, int previousAllInCount, double previousMaxAllIn)
        {
            if (gi == null)
                throw new ArgumentNullException("gi");

            if (gi.Ante < 0)
            {
                throw new ArgumentException("Invalid game info");
            }
            this.NodeId = GameTreeNode.GlobalId;
            GameTreeNode.GlobalId++;

            this.HandInfo = gi;
            this.Position = pos;
            this.PreviousAllInCount = previousAllInCount;
            this.PreviousMaxMallIn = previousMaxAllIn;
            if (pos != null)
            {
                this.OriginalStack = gi.Stacks[pos.PlayerPosition];
                if (pos.PlayerPosition == gi.Stacks.Length - 1)
                {
                    this.MandatoryPutInPot = gi.Bb + gi.Ante;
                }
                else if (pos.PlayerPosition == gi.Stacks.Length - 2)
                {
                    this.MandatoryPutInPot = gi.Sb + gi.Ante;
                }
                else
                {
                    this.MandatoryPutInPot = gi.Ante;
                }

                //this.CurrentEquity = Icm.GetEquity(gi.Stacks, gi.Payouts, pos.PlayerPosition);
            }

            this.LinearRangeStablized = false;
        }

        private void PopulatePossibleEndScenarios(List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {
            if (this.IsTerminal)
            {
                this.PossibleEndStacks = GameTreeUtility.GetLeafPossibleEndScenarios(this, pushNodes, foldNodes);
            }
            else
            {
                List<GameTreeNode> pushNodesClone = new List<GameTreeNode>(pushNodes);
                List<GameTreeNode> foldNodesClone = new List<GameTreeNode>(foldNodes);
                if (this.MandatoryPutInPot > 0)
                {
                    foldNodesClone.Add(this);
                }
                this.FoldBranch.PopulatePossibleEndScenarios(pushNodesClone, foldNodesClone);

                pushNodesClone = new List<GameTreeNode>(pushNodes);
                foldNodesClone = new List<GameTreeNode>(foldNodes);
                pushNodesClone.Add(this);
                this.PushBranch.PopulatePossibleEndScenarios(pushNodesClone, foldNodesClone);
            }
        }



        //private double[] PopulateLeafEquity(double stateProbability, List<GameTreeNode> pushNodes, bool calculateOnly, bool isLinearCalc)
        //{
        //    double[] calculaedEquity;
        //    if (pushNodes.Count <= 1)
        //    {

        //        calculaedEquity = new double[this.HandInfo.Stacks.Length];
        //        for (int i = 0; i < calculaedEquity.Length; i++)
        //        {
        //            calculaedEquity[i] = this.PossibleEndStacks[EndScenario.F12][i] * stateProbability;
        //        }

        //    }
        //    else if (pushNodes.Count == 2)
        //    {
        //        // TODO
        //        //long[] win;
        //        //long[] tie;
        //        //long[] loss;
        //        long totalHand;
        //        long[] outcomes;
        //        //double[] winEq, tieEq, totalEq;

        //        if (isLinearCalc)
        //        {
        //            outcomes = PreComputedLinearRangeOutcomeCalculator.Instance.CaculateTwoWay(
        //                pushNodes[0].Range.GenerateCurrentRangeHands().ToArray(),
        //                pushNodes[1].Range.GenerateCurrentRangeHands().ToArray(),
        //                out totalHand);
        //        }
        //        else
        //        {
        //            totalHand = this.RangeEquityResult.Total;
        //            outcomes = this.RangeEquityResult.Outcomes;

        //        }

        //        calculaedEquity = new double[this.HandInfo.Stacks.Length];
        //        for (int i = 0; i < calculaedEquity.Length; i++)
        //        {
        //            calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F11][i] * stateProbability * outcomes[0] / totalHand);
        //        }
        //        for (int i = 0; i < calculaedEquity.Length; i++)
        //        {
        //            calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F12][i] * stateProbability * outcomes[1] / totalHand);
        //        }
        //        for (int i = 0; i < calculaedEquity.Length; i++)
        //        {
        //            calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F21][i] * stateProbability * outcomes[2] / totalHand);
        //        }

        //    }
        //    else if (pushNodes.Count == 3)
        //    {
        //        long totalHand;
        //        long[] outcomes;

        //        if (isLinearCalc)
        //        {
        //            outcomes = PreComputedLinearRangeOutcomeCalculator.Instance.CalculateThreeWay(
        //                pushNodes[0].Range.GenerateCurrentRangeHands().ToArray(),
        //                pushNodes[1].Range.GenerateCurrentRangeHands().ToArray(),
        //                pushNodes[2].Range.GenerateCurrentRangeHands().ToArray(),
        //                out totalHand);
        //        }
        //        else
        //        {
        //            totalHand = this.RangeEquityResult.Total;
        //            outcomes = this.RangeEquityResult.Outcomes;

        //        }

        //        calculaedEquity = new double[this.HandInfo.Stacks.Length];
        //        for (int i = 0; i < calculaedEquity.Length; i++)
        //        {
        //            if (totalHand != 0)
        //            {
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F111][i] * stateProbability * outcomes[0] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F113][i] * stateProbability * outcomes[1] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F131][i] * stateProbability * outcomes[2] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F122][i] * stateProbability * outcomes[3] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F123][i] * stateProbability * outcomes[4] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F132][i] * stateProbability * outcomes[5] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F212][i] * stateProbability * outcomes[6] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F221][i] * stateProbability * outcomes[7] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F213][i] * stateProbability * outcomes[8] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F231][i] * stateProbability * outcomes[9] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F311][i] * stateProbability * outcomes[10] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F312][i] * stateProbability * outcomes[11] / totalHand);
        //                calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F321][i] * stateProbability * outcomes[12] / totalHand);
        //            }
        //        }
        //        //for (int i = 0; i < calculaedEquity.Length; i++)
        //        //{
        //        //    calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F12][i] * stateProbability * outcomes[1] / totalHand);
        //        //}
        //        //for (int i = 0; i < calculaedEquity.Length; i++)
        //        //{
        //        //    calculaedEquity[i] += (this.PossibleEndStacks[EndScenario.F21][i] * stateProbability * outcomes[2] / totalHand);
        //        //}
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException();
        //    }
        //    if (!calculateOnly)
        //    {
        //        this.CurrentEquity = calculaedEquity;

        //    }

        //    //StringBuilder retStr = new StringBuilder();
        //    //retStr.Append(this.Descripton.PadRight(10));

        //    //retStr.Append(String.Format("{0:F3} {1:F3} {2:F3}", calculaedEquity[0], calculaedEquity[1], calculaedEquity[2]));

        //    //Console.WriteLine(retStr);

        //    return calculaedEquity;
        //}

        public void PopulatePossibleEndScenarios()
        {
            if (this.IsTerminal)
            {
                throw new InvalidOperationException("Cannot calculate equity from leaf nodes.");
            }

            PopulatePossibleEndScenarios(new List<GameTreeNode>(), new List<GameTreeNode>());
        }

        public string PrintEquity()
        {
            StringBuilder sb = new StringBuilder();
            this.PrintEquity(1, sb);
            return sb.ToString();
        }

        internal void PrintEquity(int level, StringBuilder sb)
        {
            if (this.FoldBranch != null || this.PushBranch != null)
            {
                for (int i = 0; i < level; i++)
                {
                    sb.Append("    ");
                }

                sb.AppendFormat("[{0}]: Range {1:P1}:", this.Descripton, this.Range.Percentage);
                RangeVisualizer.PrintRange(this.Range, sb);
                //Console.Write(Environment.NewLine);


                if (this.PushBranch != null)
                {
                    this.PushBranch.PrintEquity(level + 1, sb);
                }
                if (this.FoldBranch != null)
                {
                    this.FoldBranch.PrintEquity(level + 1, sb);
                }
            }
        }

        //public double[] CalculateCurrentEquityWithPrecalculatedData()
        //{
        //    //Console.WriteLine("------------------------------------------------");
        //    return this.PopulateCurrentEquity(1d, new List<GameTreeNode>(), true, false);

        //}


        //public double[] PopulateCurrentEquity()
        //{
        //    return this.PopulateCurrentEquity(1d, new List<GameTreeNode>(), false, false);
        //}

//        private double[] PopulateCurrentEquity(double stateProbability, List<GameTreeNode> pushNodes, bool calculateOnly, bool isLinear)
//        {
//            if (this.IsTerminal)
//            {
//                return PopulateLeafEquity(stateProbability, pushNodes, calculateOnly, isLinear);
//            }
//            else
//            {
//                List<GameTreeNode> pushNodesClone = new List<GameTreeNode>(pushNodes);
//                var foldEquity = this.FoldBranch.PopulateCurrentEquity((1 - this.Range.Percentage) * stateProbability, pushNodesClone, calculateOnly, isLinear);

//                pushNodesClone = new List<GameTreeNode>(pushNodes);
//                pushNodesClone.Add(this);
//                var pushEquity = this.PushBranch.PopulateCurrentEquity(this.Range.Percentage * stateProbability, pushNodesClone, calculateOnly, isLinear);

//                double[] equity = new double[this.HandInfo.Stacks.Length];
//                for (int i = 0; i < foldEquity.Length; i++)
//                {
//                    equity[i] += foldEquity[i];
//                    equity[i] += pushEquity[i];

//#if DEBUG
//                    if (equity[i] <= 0)
//                    {
//                        Console.WriteLine("Error");
//                    }

//#endif
//                }

//                if (!calculateOnly)
//                {
//                    this.CurrentEquity = equity;
//                }

//                return equity;
//            }
//        }




        //internal bool IsLinearRangeStablized()
        //{
        //    if (this.Range == null)
        //    {
        //        return true;
        //    }
        //    else if (this.LinearRangeStablized)
        //    {
        //        return this.FoldBranch.IsLinearRangeStablized() && this.PushBranch.IsLinearRangeStablized();
        //    }
        //    return false;
        //}

        //internal bool IsUnrestrictedRangeStablized()
        //{
        //    if (this.Range != null && this.UnrestrictedRangeStablized)
        //    {
        //        return this.FoldBranch.IsUnrestrictedRangeStablized() && this.PushBranch.IsUnrestrictedRangeStablized();
        //    }
        //    return false;

        //}

        //public double[] CalculateCurrentEquity(bool isHypothetical, bool isLinear)
        //{
        //    return this.PopulateCurrentEquity(1d, new List<GameTreeNode>(), true, isHypothetical, isLinear);
        //}

        //public double[] PopulateCurrentEquity(bool isHypothetical, bool isLinear)
        //{
        //    return this.PopulateCurrentEquity(1d, new List<GameTreeNode>(), false, isHypothetical, isLinear);
        //}

        //public double[] PopulateCurrentEquity(double stateProbability, List<GameTreeNode> pushNodes, bool calculateOnly, bool isHypothetical, bool isLinear)
        //{
        //    if (this.IsTerminal)
        //    {
        //        return PopulateLeafEquity(stateProbability, pushNodes, calculateOnly, isLinear);
        //    }
        //    else
        //    {
        //        List<GameTreeNode> pushNodesClone = new List<GameTreeNode>(pushNodes);
        //        var foldEquity = this.FoldBranch.PopulateCurrentEquity((1 - this.Range.Percentage) * stateProbability, pushNodesClone, calculateOnly, isHypothetical, isLinear);

        //        pushNodesClone = new List<GameTreeNode>(pushNodes);
        //        pushNodesClone.Add(this);
        //        var pushEquity = this.PushBranch.PopulateCurrentEquity(this.Range.Percentage * stateProbability, pushNodesClone, calculateOnly, isHypothetical, isLinear);

        //        double[] equity = new double[this.HandInfo.Stacks.Length];
        //        for (int i = 0; i < foldEquity.Length; i++)
        //        {
        //            equity[i] += foldEquity[i];
        //            equity[i] += pushEquity[i];
        //        }

        //        if (!calculateOnly)
        //        {
        //            this.CurrentEquity = equity;
        //        }

        //        return equity;
        //    }
        //}

        //public void CalculateOptimalLinearRange(GameTreeNode root)
        //{
        //    CalculateOptimalLinearRange(new List<GameTreeNode>(), root);
        //}

        //public void CalculateOptimalUnrestrictedRange(GameTreeNode root)
        //{
        //    CalculateOptimalUnrestrictedRange(new List<GameTreeNode>(), root);
        //}

        //private void CalculateOptimalLinearRange(List<GameTreeNode> pushNodes, GameTreeNode root)
        //{
        //    if (this.IsTerminal)
        //    {
        //        if (pushNodes.Count <= 1)
        //        {
        //            return;
        //        }
        //        else if (pushNodes.Count == 2)
        //        {
        //            // 2 way all in
        //            ComputeOptimalLinearRanges(pushNodes[0].Range, root);
        //            ComputeOptimalLinearRanges(pushNodes[1].Range, root);
        //            return;
        //        }
        //        else if (pushNodes.Count == 3)
        //        {
        //            ComputeOptimalLinearRanges(pushNodes[2].Range, root);
        //            ComputeOptimalLinearRanges(pushNodes[1].Range, root);
        //            ComputeOptimalLinearRanges(pushNodes[0].Range, root);

        //            return;
        //        }
        //        throw new Exception("invalid state");
        //    }
        //    else
        //    {
        //        double foldEquity, pushEquity = 0;

        //        List<GameTreeNode> clone = new List<GameTreeNode>(pushNodes);
        //        clone.Add(this);
        //        this.PushBranch.CalculateOptimalLinearRange(clone, root);

        //        clone = new List<GameTreeNode>(pushNodes);
        //        this.FoldBranch.CalculateOptimalLinearRange(clone, root);
        //    }
        //}

//        private void ComputeOptimalLinearRanges(IRange rangeA, GameTreeNode root)
//        {
//            if (rangeA == null)
//            {
//                throw new ArgumentException("invalid ranges");
//            }

//#if DEBUG
//            Console.WriteLine("{0}: Pre linear range {1:P2}", rangeA.ParentNode.Descripton, rangeA.ParentNode.Range.Percentage);
//#endif

//            double[] currentEquity = root.CalculateCurrentEquity(false, true);
//            string initRange = rangeA.RangeStr;

//#if DEBUG
//            if (Math.Abs(currentEquity.Sum() - this.HandInfo.Stacks.Sum()) > 0.001)
//            {
//                Debugger.Break();
//            }
//            //Console.WriteLine("{0}: Pre range {1:P2}", rangeA.ParentNode.Descripton, rangeA.ParentNode.Range.Percentage);
//#endif

//            var hands = rangeA.OrderedHands.ToArray();
//            for (int i = 0; i < hands.Length; i++)
//            {
//                if (i == 0)
//                {
//                    continue;
//                }
//                if (!rangeA.IsHandToggled(hands[i]))
//                {
//                    rangeA.ToggleHandInRange(hands[i], true);
//                    double[] newEquity = root.CalculateCurrentEquity(true, true);

//#if DEBUG
//                    if (Math.Abs(newEquity.Sum() - this.HandInfo.Stacks.Sum()) > 0.001)
//                    {
//                        Debugger.Break();
//                    }
//#endif
//                    var preIcm = Icm.GetEquity(currentEquity, this.HandInfo.Payouts,
//                        rangeA.ParentNode.Position.PlayerPosition);
//                    var postIcm = Icm.GetEquity(newEquity, this.HandInfo.Payouts,
//                        rangeA.ParentNode.Position.PlayerPosition);
//                    if (postIcm - preIcm >= 0)
//                    {
//                        currentEquity = newEquity;
//                        break;
//                    }
//                    else
//                    {
//                        rangeA.ToggleHandInRange(hands[i], false);
//                        break;
//                    }
//                }
//            }

//            for (int i = hands.Length - 1; i > 0; i--)
//            {
//                if (rangeA.IsHandToggled(hands[i]))
//                {
//                    rangeA.ToggleHandInRange(hands[i], false);
//                    double[] newEquity = root.CalculateCurrentEquity(true, true);

//#if DEBUG
//                    if (Math.Abs(newEquity.Sum() - this.HandInfo.Stacks.Sum()) > 0.001)
//                    {
//                        Debugger.Break();
//                    }
//#endif
//                    var preIcm = Icm.GetEquity(currentEquity, this.HandInfo.Payouts,
//                        rangeA.ParentNode.Position.PlayerPosition);
//                    var postIcm = Icm.GetEquity(newEquity, this.HandInfo.Payouts,
//                        rangeA.ParentNode.Position.PlayerPosition);
//                    if (postIcm - preIcm >= 0)
//                    {
//                        currentEquity = newEquity;
//                        break;
//                    }
//                    else
//                    {
//                        rangeA.ToggleHandInRange(hands[i], true);
//                        break;
//                    }
//                }
//            }


//            if (rangeA.RangeStr == initRange)
//            {
//                rangeA.ParentNode.LinearRangeStablized = true;
//            }
//            else
//            {
//                rangeA.ParentNode.LinearRangeStablized = false;
//            }

//#if DEBUG
//            Console.WriteLine("{0}: Post linear range {1:P2}", rangeA.ParentNode.Descripton, rangeA.ParentNode.Range.Percentage);
//#endif
//        }

//        private void CalculateOptimalUnrestrictedRange(List<GameTreeNode> pushNodes, GameTreeNode root)
//        {
//            if (this.IsTerminal)
//            {
//                if (pushNodes.Count <= 1)
//                {
//                    return;
//                }
//                else if (pushNodes.Count == 2)
//                {
//                    // 2 way all in

//                    //ComputeOptimalUnrestrictedRanges(pushNodes[1].Range, root);
//                    ComputeOptimalUnrestrictedRanges(pushNodes[0].Range, root);
//                    return;
//                }
//                else if (pushNodes.Count == 3)
//                {
//                    ComputeOptimalUnrestrictedRanges(pushNodes[2].Range, root);
//                    ComputeOptimalUnrestrictedRanges(pushNodes[1].Range, root);
//                    ComputeOptimalUnrestrictedRanges(pushNodes[0].Range, root);


//                    return;
//                }
//                throw new Exception("invalid state");
//            }
//            else
//            {
//                double foldEquity, pushEquity = 0;

//                List<GameTreeNode> clone = new List<GameTreeNode>(pushNodes);
//                clone.Add(this);
//                this.PushBranch.CalculateOptimalUnrestrictedRange(clone, root);

//                clone = new List<GameTreeNode>(pushNodes);
//                this.FoldBranch.CalculateOptimalUnrestrictedRange(clone, root);
//            }
//        }

//        private void ComputeOptimalUnrestrictedRanges(IRange rangeA, GameTreeNode root)
//        {
//            if (rangeA == null)
//            {
//                throw new ArgumentException("invalid ranges");
//            }


//#if DEBUG
//            Console.WriteLine("Start iteration to calculate range for {0}", rangeA.ParentNode.Descripton);
//#endif


//            bool localStablized = false;

//            string initRange = rangeA.RangeStr;

//#if DEBUG
//            Console.WriteLine("{0}: Pre unrestricted range {1:P2}", rangeA.ParentNode.Descripton, rangeA.ParentNode.Range.Percentage);
//#endif

//            double[] currentEquity = root.CalculateCurrentEquity(false, false);
//            while (!localStablized)
//            {

//                string localInitialRange = rangeA.RangeStr;
//                var hands = rangeA.OrderedHands.ToArray();

//                for (int i = 0; i < hands.Length; i++)
//                {
//                    if (i == 0)
//                        continue;

//                    if (!rangeA.IsHandToggled(hands[i]))
//                    {
//                        rangeA.ToggleHandInRange(hands[i], true);
//                        double[] newEquity = root.CalculateCurrentEquity(true, false);

//                        var preIcm = Icm.GetEquity(currentEquity, this.HandInfo.Payouts,
//                            rangeA.ParentNode.Position.PlayerPosition);
//                        var postIcm = Icm.GetEquity(newEquity, this.HandInfo.Payouts,
//                            rangeA.ParentNode.Position.PlayerPosition);
//                        if (postIcm - preIcm >= 0)
//                        {
//                            currentEquity = newEquity;
//                            Console.Write("+{0}, ", hands[i].HandGroupString);
//                            break;
//                        }
//                        else
//                        {
//                            rangeA.ToggleHandInRange(hands[i], false);
//                        }
//                    }
//                    else
//                    {
//                        rangeA.ToggleHandInRange(hands[i], false);
//                        double[] newEquity = root.CalculateCurrentEquity(true, false);

//                        var preIcm = Icm.GetEquity(currentEquity, this.HandInfo.Payouts,
//                            rangeA.ParentNode.Position.PlayerPosition);
//                        var postIcm = Icm.GetEquity(newEquity, this.HandInfo.Payouts,
//                            rangeA.ParentNode.Position.PlayerPosition);
//                        if (postIcm - preIcm >= 0)
//                        {
//                            currentEquity = newEquity;
//                            Console.Write("-{0}, ", hands[i].HandGroupString);
//                            break;
//                        }
//                        else
//                        {
//                            rangeA.ToggleHandInRange(hands[i], true);
//                        }
//                    }
//                }

//                if (rangeA.RangeStr == localInitialRange)
//                {
//                    localStablized = true;
//                }

//                if (rangeA.RangeStr == initRange)
//                {
//                    rangeA.ParentNode.UnrestrictedRangeStablized = true;
//                }
//                else
//                {
//                    rangeA.ParentNode.UnrestrictedRangeStablized = false;
//                }


//            }
//            Console.WriteLine();

//#if DEBUG
//            Console.WriteLine("{0}: Post unrestricted range {1:P2}", rangeA.ParentNode.Descripton, rangeA.ParentNode.Range.Percentage);
//#endif


//#if DEBUG
//            Console.WriteLine("End for {0}-----------------------", rangeA.ParentNode.Descripton);
//#endif


//        }
    }
}

