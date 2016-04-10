
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using GameTreeDraft.Equity;
using GameTreeDraft.Hands;
using GameTreeDraft.ICM;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.GameTree
{
    using System;
    using System.Collections.Generic;

    public class TerminalCondition
    {

        public GameTreeNode TerminalNode { get; private set; }

        public DifferentialRangeEquityCalculator DifferentialCalc { get; set; }

        private TerminalCondition()
        {
        }

        public TerminalCondition(GameTreeNode terminalNode, DifferentialRangeEquityCalculator diffCalc)
        {
            this.TerminalNode = terminalNode;
            this.DifferentialCalc = diffCalc;
        }
    }

    public class NashOptimizer
    {
        private List<GameTreeNode> nodesWithRange;

        // range node and their associated terminal nodes
        private Dictionary<GameTreeNode, List<TerminalCondition>> rangeNodeMap;

        // leaf nodes and their associated push nodes in order
        private Dictionary<GameTreeNode, List<GameTreeNode>> leafNodePushMap;

        // leaf nodes and their associated fold nodes in order
        private Dictionary<GameTreeNode, List<GameTreeNode>> leafNodeFoldMap;


        private GameTreeNode rootNode;

        public NashOptimizer(GameTreeNode root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            this.rootNode = root;
            nodesWithRange = new List<GameTreeNode>();
            rangeNodeMap = new Dictionary<GameTreeNode, List<TerminalCondition>>();
            leafNodePushMap = new Dictionary<GameTreeNode, List<GameTreeNode>>();
            leafNodeFoldMap = new Dictionary<GameTreeNode, List<GameTreeNode>>();

            // initialize push node ranges to prevent uncalculable situation
            this.PrepopulateInitialRange(root, new List<GameTreeNode>());

            // prepare tree
            this.PrepGameTree(root, new List<GameTreeNode>(), new List<GameTreeNode>());
        }

        public double[] GetCurrentEquity()
        {
            var result = GetCurrentEquity(this.rootNode, 1);
#if DEBUG

            Debug.Assert(Math.Abs(result.Sum() - rootNode.HandInfo.Stacks.Sum()) < 0.001);
#endif

            return result;
        }

        private double[] GetCurrentEquity(GameTreeNode node, double stateProbability)
        {
            if (node.IsTerminal)
            {
                var pushNodes = this.leafNodePushMap[node];

                return this.GetLeafEquity(node, stateProbability);
            }
            else
            {
                var foldEquity = this.GetCurrentEquity(node.FoldBranch, (1 - node.Range.Percentage) * stateProbability);
                var pushEquity = this.GetCurrentEquity(node.PushBranch, node.Range.Percentage * stateProbability);

                double[] equity = new double[node.HandInfo.Stacks.Length];
                for (int i = 0; i < foldEquity.Length; i++)
                {
                    equity[i] += foldEquity[i];
                    equity[i] += pushEquity[i];
                }

                return equity;
            }
        }


        private double[] GetLeafEquity(GameTreeNode leaf, double stateProbability)
        {
            double[] calculatedEquity;
            var pushNodes = this.leafNodePushMap[leaf];

            if (pushNodes.Count <= 1)
            {
                calculatedEquity = new double[leaf.HandInfo.Stacks.Length];
                for (int i = 0; i < calculatedEquity.Length; i++)
                {
                    calculatedEquity[i] = leaf.PossibleEndStacks[EndScenario.F12][i] * stateProbability;
                }

            }
            else if (pushNodes.Count == 2)
            {
                long totalHand = leaf.RangeEquityResult.Total;
                long[] outcomes = leaf.RangeEquityResult.Outcomes;


                calculatedEquity = new double[leaf.HandInfo.Stacks.Length];
                for (int i = 0; i < calculatedEquity.Length; i++)
                {
                    calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F11][i] * stateProbability * outcomes[0] / totalHand);
                    calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F12][i] * stateProbability * outcomes[1] / totalHand);
                    calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F21][i] * stateProbability * outcomes[2] / totalHand);
                }

            }
            else if (pushNodes.Count == 3)
            {
                long totalHand = leaf.RangeEquityResult.Total;
                long[] outcomes = leaf.RangeEquityResult.Outcomes;


                calculatedEquity = new double[leaf.HandInfo.Stacks.Length];
                for (int i = 0; i < calculatedEquity.Length; i++)
                {
                    if (totalHand != 0)
                    {
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F111][i] * stateProbability * outcomes[0] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F113][i] * stateProbability * outcomes[1] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F131][i] * stateProbability * outcomes[2] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F122][i] * stateProbability * outcomes[3] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F123][i] * stateProbability * outcomes[4] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F132][i] * stateProbability * outcomes[5] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F212][i] * stateProbability * outcomes[6] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F221][i] * stateProbability * outcomes[7] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F213][i] * stateProbability * outcomes[8] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F231][i] * stateProbability * outcomes[9] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F311][i] * stateProbability * outcomes[10] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F312][i] * stateProbability * outcomes[11] / totalHand);
                        calculatedEquity[i] += (leaf.PossibleEndStacks[EndScenario.F321][i] * stateProbability * outcomes[12] / totalHand);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            return calculatedEquity;
        }




        public bool CalculateOptimalLinearRange(bool test)
        {
            return CalculateOptimalRange(this.ApplyLinearRangeChange, this.ApplyLinearRangeChange, true, test);
        }

        public bool CalculateOptimalUnrestrictedRange(bool test)
        {
            return CalculateOptimalRange(this.ApplyUnrestrictedRangeChange, this.RevertUnrestrictedRangeChange, false, test);
        }

        private bool CalculateOptimalRange(
            Action<GameTreeNode, PHand, DifferentialHandOp> applyRangeChangeAction,
            Action<GameTreeNode, PHand, DifferentialHandOp> revertRangeChangeAction,
            bool breakOnSkippedNode,
            bool experimental)
        {
            if (applyRangeChangeAction == null)
            {
                throw new ArgumentNullException("applyRangeChangeAction");
            }

            if (revertRangeChangeAction == null)
            {
                throw new ArgumentNullException("revertRangeChangeAction");
            }

            bool allStablized = true;

            if (!experimental)
            {
                foreach (GameTreeNode node in this.nodesWithRange)
                {
                    var localNodeStablized = this.OptimizeNodeRange(node, applyRangeChangeAction,
                        revertRangeChangeAction, breakOnSkippedNode);

                    if (!localNodeStablized &&
                        (node.LastRemovals.Count > 1 && node.LastRemovals[node.LastRemovals.Count - 1] == node.LastRemovals[node.LastRemovals.Count - 2] &&
                        node.LastAdditions.Count > 1 && node.LastAdditions[node.LastAdditions.Count - 1] == node.LastAdditions[node.LastAdditions.Count - 2]) ||
                        (node.LastRemovals.Count > 2 && node.LastRemovals[node.LastRemovals.Count - 1] == node.LastRemovals[node.LastRemovals.Count - 3] &&
                        node.LastAdditions.Count > 2 && node.LastAdditions[node.LastAdditions.Count - 1] == node.LastAdditions[node.LastAdditions.Count - 3]))
                    {
                        localNodeStablized = true;
                    }
                    allStablized = allStablized && localNodeStablized;
                }
            }
            else
            {
                var localNodeStablized1 = true;//this.OptimizeNodeRange(nodesWithRange[0], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);
                var localNodeStablized2 = true;//this.OptimizeNodeRange(nodesWithRange[1], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);
                var localNodeStablized3 = true;//this.OptimizeNodeRange(nodesWithRange[3], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);
                var localNodeStablized4 = true;//this.OptimizeNodeRange(nodesWithRange[2], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);
                var localNodeStablized5 = true;//this.OptimizeNodeRange(nodesWithRange[4], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);
                var localNodeStablized6 = this.OptimizeNodeRange(nodesWithRange[0], applyRangeChangeAction, revertRangeChangeAction, breakOnSkippedNode);

                allStablized = allStablized && localNodeStablized1 && localNodeStablized2 && localNodeStablized3 && localNodeStablized4 && localNodeStablized5 && localNodeStablized6;
            }

            return allStablized;
        }

        private bool OptimizeNodeRange(GameTreeNode node,
            Action<GameTreeNode, PHand, DifferentialHandOp> applyRangeChangeAction,
            Action<GameTreeNode, PHand, DifferentialHandOp> revertRangeChangeAction,
            bool breakOnSkippedNode)
        {
            bool rangeStablized = true;



#if DEBUG
            Console.WriteLine("{0}: Pre unrestricted range {1:P2}", node.Descripton, node.Range.Percentage);
            int iteration = 0;
            iteration++;
            Console.WriteLine("Iteration {0}", iteration);
#endif

            var hands = node.Range.OrderedHands.ToArray();
            double[] currentEquity = this.GetCurrentEquity();


            // do reduction from back
            int totalMissed = 0;
            bool found = false;
            for (int i = node.Range.OrderedHands.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                    continue;

                if (totalMissed > Globals.TotalMissesTolerated)
                    break;

                // try reduce 
                if (node.Range.IsHandToggled(hands[i]))
                {
                    node.Range.ToggleHandInRange(hands[i], false);

                    applyRangeChangeAction.Invoke(node, hands[i], DifferentialHandOp.Remove);

                    double[] newEquity = this.GetCurrentEquity();

                    var preIcm = Icm.GetEquity(currentEquity, node.HandInfo.Payouts, node.Position.PlayerPosition);
                    var postIcm = Icm.GetEquity(newEquity, node.HandInfo.Payouts, node.Position.PlayerPosition);

                    if (postIcm - preIcm >= 0 && !newEquity.Any(double.IsNaN))
                    {
                        node.LastRemovals.Add(hands[i]);
                        currentEquity = newEquity;
                        found = true;
#if DEBUG
                        Console.WriteLine("{0}: -{1}, {2:p2} ", node.Descripton, hands[i].HandGroupString, node.Range.Percentage);
#endif
                       rangeStablized = false;
                        break;
                    }
                    else
                    {
                        totalMissed++;

                        node.Range.ToggleHandInRange(hands[i], true);
                        if (revertRangeChangeAction != null)
                        {
                            revertRangeChangeAction.Invoke(node, hands[i], DifferentialHandOp.Add);
                        }

                        if (breakOnSkippedNode)
                        {
                            break;
                        }
                    }
                }
            }

            //if (!breakOnSkippedNode && !found && totalMissed > 0)
            //{
            //    Console.WriteLine("Reduces: Total misses and not found: {0}", totalMissed);
            //}
            

            totalMissed = 0;
            found = false;
            // do addition from front
            for (int i = 0; i < node.Range.OrderedHands.Count; i++)
            {
                if (i == 0)
                    continue;

                if (totalMissed > Globals.TotalMissesTolerated)
                    break;

                if (!node.Range.IsHandToggled(hands[i]))
                {
                    node.Range.ToggleHandInRange(hands[i], true);

                    applyRangeChangeAction.Invoke(node, hands[i], DifferentialHandOp.Add);

                    double[] newEquity = this.GetCurrentEquity();

                    var preIcm = Icm.GetEquity(currentEquity, node.HandInfo.Payouts, node.Position.PlayerPosition);
                    var postIcm = Icm.GetEquity(newEquity, node.HandInfo.Payouts, node.Position.PlayerPosition);

#if DEBUG
                    Debug.Assert(!newEquity.Any(double.IsNaN));
#endif

                    if (postIcm - preIcm >= 0 && !newEquity.Any(double.IsNaN))
                    {
                        node.LastAdditions.Add(hands[i]);
                        found = false;
#if DEBUG
                        Console.WriteLine("{0}: +{1}, {2:p2} ", node.Descripton, hands[i].HandGroupString,
                            node.Range.Percentage);
#endif
                    
                        rangeStablized = false;
                        break;
                    }
                    else
                    {
                        totalMissed++;
                        node.Range.ToggleHandInRange(hands[i], false);
                        if (revertRangeChangeAction != null)
                        {
                            revertRangeChangeAction.Invoke(node, hands[i], DifferentialHandOp.Remove);
                        }
                        if (breakOnSkippedNode )
                        {
                            break;
                        }
                    }
                }

              
            
            }

            //if (!breakOnSkippedNode && !found && totalMissed > 0)
            //{
            //    Console.WriteLine("Additions: Total misses and not found: {0}", totalMissed);
            //}



#if DEBUG
            Console.WriteLine();
            Console.WriteLine("{0}: Post unrestricted range {1:P2}", node.Descripton, node.Range.Percentage);
#endif
            return rangeStablized;

        }

        private void ApplyLinearRangeChange(GameTreeNode node, PHand handGroup, DifferentialHandOp differentialHandOp)
        {
            List<TerminalCondition> affectedTerminalConditons = this.rangeNodeMap[node];

            foreach (TerminalCondition condition in affectedTerminalConditons)
            {
                condition.DifferentialCalc.CalculateLinearEquity(
                    node,
                    new[] { handGroup },
                    differentialHandOp);
            }
        }

        private void RevertLinearRangeChange(GameTreeNode node, PHand handGroup, DifferentialHandOp differentialHandOp)
        {
            List<TerminalCondition> affectedTerminalConditons = this.rangeNodeMap[node];

            foreach (TerminalCondition condition in affectedTerminalConditons)
            {
                condition.DifferentialCalc.RevertLastLinearOp(node,
                    new[] { handGroup },
                    differentialHandOp);
            }
        }

        private void ApplyUnrestrictedRangeChange(GameTreeNode node, PHand handGroup, DifferentialHandOp differentialHandOp)
        {
            List<TerminalCondition> affectedTerminalConditons = this.rangeNodeMap[node];

            foreach (TerminalCondition condition in affectedTerminalConditons)
            {
                condition.DifferentialCalc.CalculateDifferentialEquity(
                    node,
                    new[] { handGroup },
                    differentialHandOp);

            }
        }

        private void RevertUnrestrictedRangeChange(GameTreeNode node, PHand handGroup, DifferentialHandOp differentialHandOp)
        {
            List<TerminalCondition> affectedTerminalConditons = this.rangeNodeMap[node];

            foreach (TerminalCondition condition in affectedTerminalConditons)
            {
                condition.DifferentialCalc.RevertLastDiffOp(
                    node,
                    new[] { handGroup },
                    differentialHandOp);
            }
        }

        private void PrepopulateInitialRange(GameTreeNode node, List<GameTreeNode> previousRangeNodes)
        {
            if (node == null)
            {
                return;
            }

            if (node.IsTerminal)
            {
                if (previousRangeNodes.Count > 3)
                {
                    var rangeA = previousRangeNodes[0].Range.GenerateCurrentRangeHands();
                    var rangeB = previousRangeNodes[0].Range.GenerateCurrentRangeHands();
                    var rangeC = previousRangeNodes[0].Range.GenerateCurrentRangeHands();

                    if (rangeA.Count == 1 && rangeB.Count == 1 && rangeC.Count == 1 && !previousRangeNodes[0].Range.IsHandToggled(HandRange.Instance.LookupHandGroup("KK")))
                    {
                        previousRangeNodes[0].Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
                    }
                }
            }
            else
            {
                // this node has range
                if (!nodesWithRange.Contains(node))
                {
                    nodesWithRange.Add(node);
                    rangeNodeMap.Add(node, new List<TerminalCondition>());
                }

                List<GameTreeNode> clone = new List<GameTreeNode>(previousRangeNodes);
                this.PrepopulateInitialRange(node.FoldBranch, clone);

                clone = new List<GameTreeNode>(previousRangeNodes);
                clone.Add(node);
                this.PrepopulateInitialRange(node.PushBranch, clone);
            }
        }

        public static int Count = 0;
        private void PrepGameTree(GameTreeNode node, List<GameTreeNode> previousPushNodes, List<GameTreeNode> previousFoldNodes)
        {
            if (node == null)
            {
                throw new Exception("shouldnt get here");
            }

            if (node.IsTerminal)
            {

                this.leafNodePushMap.Add(node, previousPushNodes);
                this.leafNodeFoldMap.Add(node, previousFoldNodes);

                //leaf nodes
                if (previousPushNodes.Count < 2)
                {
                    // no range calculation needed
                    return;
                }
                if (previousPushNodes.Count > 3)
                {
                    throw new NotSupportedException();
                }
                // initialize diff calc

                Dictionary<GameTreeNode, int> nodeIndexLut = new Dictionary<GameTreeNode, int>();
                for (int i = 0; i < previousPushNodes.Count; i++)
                {
                    nodeIndexLut.Add(previousPushNodes[i], i);

                }

                var diffCalc = new DifferentialRangeEquityCalculator(nodeIndexLut);
                var terminalConditon = new TerminalCondition(node, diffCalc);
                if (previousPushNodes.Count == 2)
                {
                    node.RangeEquityResult = diffCalc.CaculateTwoWayBaseEquity(
                        previousPushNodes[0].Range.GenerateCurrentRangeHands(),
                        previousPushNodes[1].Range.GenerateCurrentRangeHands());
                }
                else
                {
                    node.RangeEquityResult = diffCalc.CaculateThreeWayBaseEquity(
                       previousPushNodes[0].Range.GenerateCurrentRangeHands(),
                       previousPushNodes[1].Range.GenerateCurrentRangeHands(),
                       previousPushNodes[2].Range.GenerateCurrentRangeHands());
                }

                for (int i = 0; i < previousPushNodes.Count; i++)
                {
                    this.rangeNodeMap[previousPushNodes[i]].Add(terminalConditon);
                }
            }
            else
            {
                // this node has range
                if (!nodesWithRange.Contains(node))
                {
                    nodesWithRange.Add(node);
                    rangeNodeMap.Add(node, new List<TerminalCondition>());
                }

                List<GameTreeNode> pushNodesClone = new List<GameTreeNode>(previousPushNodes);
                List<GameTreeNode> foldNodesClone = new List<GameTreeNode>(previousPushNodes);
                foldNodesClone.Add(node);
                this.PrepGameTree(node.FoldBranch, pushNodesClone, foldNodesClone);

                pushNodesClone = new List<GameTreeNode>(previousPushNodes);
                foldNodesClone = new List<GameTreeNode>(previousPushNodes);
                pushNodesClone.Add(node);
                this.PrepGameTree(node.PushBranch, pushNodesClone, foldNodesClone);
            }
        }



    }
}
