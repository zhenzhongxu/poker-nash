using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameTreeDraft.GameTree
{
    public class GameTreeUtility
    {
        public static Dictionary<EndScenario, double[]> GetLeafPossibleEndScenarios(GameTreeNode leaf, List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {

            if (pushNodes.Count > 3)
            {
                throw new ArgumentException("invalid push count");
            }

            if (pushNodes.Count == 0)
            {
                return GetLeafPossibleEndScenariosNoPusher(leaf, pushNodes, foldNodes);
            }
            else if (pushNodes.Count == 1)
            {
                return GetLeafPossibleEndScenarios1Pusher(leaf, pushNodes, foldNodes);
            }
            else if (pushNodes.Count == 2)
            {
                return GetLeafPossibleEndScenarios2Pushers(leaf, pushNodes, foldNodes);

            }
            else if (pushNodes.Count == 3)
            {
                return GetLeafPossibleEndScenarios3Pushers(leaf, pushNodes, foldNodes);
            }
            else
            {
                throw new InvalidOperationException("invalid");
            }
        }


        private static Dictionary<EndScenario, double[]> GetLeafPossibleEndScenariosNoPusher(GameTreeNode leaf, List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {
            // sb folds to bb
            //Debug.Assert(foldNodes.Count == 1);
            //Debug.Assert(pushNodes.Count == 0);

            Dictionary<EndScenario, double[]> retVal = new Dictionary<EndScenario, double[]>();

            double[] endstack = new double[leaf.HandInfo.Stacks.Length];
            leaf.HandInfo.Stacks.CopyTo(endstack, 0);

            double totalFolded = 0;
            foreach (GameTreeNode foldNode in foldNodes)
            {
                endstack[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;

                totalFolded += foldNode.MandatoryPutInPot;
            }

            endstack[endstack.Length - 1] += totalFolded;

            retVal.Add(EndScenario.F12, endstack);

            return retVal;
        }

        private static Dictionary<EndScenario, double[]> GetLeafPossibleEndScenarios1Pusher(GameTreeNode leaf, List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {
            Dictionary<EndScenario, double[]> retVal = new Dictionary<EndScenario, double[]>();

            //blinds fold to pusher
            double totalFolded = 0;

            double[] endstack = new double[leaf.HandInfo.Stacks.Length];
            leaf.HandInfo.Stacks.CopyTo(endstack, 0);

            foreach (GameTreeNode foldNode in foldNodes)
            {
                totalFolded += foldNode.MandatoryPutInPot;
                endstack[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;
            }
            endstack[pushNodes[0].Position.PlayerPosition] += totalFolded;

            retVal.Add(EndScenario.F12, endstack);

            return retVal;

        }

        private static Dictionary<EndScenario, double[]> GetLeafPossibleEndScenarios2Pushers(GameTreeNode leaf, List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {
            Dictionary<EndScenario, double[]> retVal = new Dictionary<EndScenario, double[]>();

            var foldNode = foldNodes.FirstOrDefault(t => t.MandatoryPutInPot > 0);

            for (int i = 1; i <= 3; i++)
            {
                double[] stack = new double[leaf.HandInfo.Stacks.Length];
                leaf.HandInfo.Stacks.CopyTo(stack, 0);

                retVal.Add((EndScenario)((i)), stack);
            }

            PopulateStack(retVal[EndScenario.F11], foldNode, pushNodes.ToArray(), null, null, 2);
            PopulateStack(retVal[EndScenario.F12], foldNode, new[] { pushNodes[0] }, new[] { pushNodes[1] }, null, 2);
            PopulateStack(retVal[EndScenario.F21], foldNode, new[] { pushNodes[1] }, new[] { pushNodes[0] }, null, 2);


            ////blinds fold to pusher
            //double totalFolded = 0;
            //double totalPushed = 0;

            //double[] endstack1 = new double[leaf.HandInfo.Stacks.Length];
            //double[] endstack2 = new double[leaf.HandInfo.Stacks.Length];
            //double[] endstack3 = new double[leaf.HandInfo.Stacks.Length];
            //leaf.HandInfo.Stacks.CopyTo(endstack1, 0);
            //leaf.HandInfo.Stacks.CopyTo(endstack2, 0);
            //leaf.HandInfo.Stacks.CopyTo(endstack3, 0);

            //foreach (GameTreeNode foldNode in foldNodes)
            //{
            //    totalFolded += foldNode.MandatoryPutInPot;
            //    endstack1[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;
            //    endstack2[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;
            //    endstack3[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;
            //}

            ////   F12, F21, F11,

            //endstack1[pushNodes[0].Position.PlayerPosition] += (totalFolded + pushNodes[1].PutInPot);
            //endstack1[pushNodes[1].Position.PlayerPosition] -= pushNodes[1].PutInPot;


            //endstack2[pushNodes[1].Position.PlayerPosition] += (totalFolded + pushNodes[0].PutInPot);
            //endstack2[pushNodes[0].Position.PlayerPosition] -= pushNodes[0].PutInPot;

            //endstack3[pushNodes[0].Position.PlayerPosition] += (totalFolded / 2);
            //endstack3[pushNodes[1].Position.PlayerPosition] += (totalFolded / 2);

            ////TODO: these scenarios are  not complete, still need to take care of short stacks and side pot scenarios etc
            //retVal.Add(EndScenario.F12, endstack1);
            //retVal.Add(EndScenario.F21, endstack2);
            //retVal.Add(EndScenario.F11, endstack3);

            return retVal;
        }


        private static Dictionary<EndScenario, double[]> GetLeafPossibleEndScenarios3Pushers(GameTreeNode leaf, List<GameTreeNode> pushNodes, List<GameTreeNode> foldNodes)
        {
            Debug.Assert(foldNodes.Count(t => t.MandatoryPutInPot > 0) <= 1);

            Dictionary<EndScenario, double[]> retVal = new Dictionary<EndScenario, double[]>();


            var foldNode = foldNodes.FirstOrDefault(t => t.MandatoryPutInPot > 0);

            for (int i = 0; i < 13; i++)
            {
                double[] stack = new double[leaf.HandInfo.Stacks.Length];
                leaf.HandInfo.Stacks.CopyTo(stack, 0);

                retVal.Add((EndScenario)((i + 10)), stack);
            }

            PopulateStack(retVal[EndScenario.F111], foldNode, pushNodes.ToArray(), null, null, 3);
            PopulateStack(retVal[EndScenario.F113], foldNode, new[] { pushNodes[0], pushNodes[1] }, new[] { pushNodes[2] }, null, 3);
            PopulateStack(retVal[EndScenario.F131], foldNode, new[] { pushNodes[0], pushNodes[2] }, new[] { pushNodes[1] }, null, 3);
            PopulateStack(retVal[EndScenario.F122], foldNode, new[] { pushNodes[0] }, new[] { pushNodes[2], pushNodes[1] }, null, 3);
            PopulateStack(retVal[EndScenario.F123], foldNode, new[] { pushNodes[0] }, new[] { pushNodes[1] }, new[] { pushNodes[2] }, 3);
            PopulateStack(retVal[EndScenario.F132], foldNode, new[] { pushNodes[0] }, new[] { pushNodes[2] }, new[] { pushNodes[1] }, 3);
            PopulateStack(retVal[EndScenario.F212], foldNode, new[] { pushNodes[1] }, new[] { pushNodes[0], pushNodes[2] }, null, 3);
            PopulateStack(retVal[EndScenario.F221], foldNode, new[] { pushNodes[2] }, new[] { pushNodes[0], pushNodes[1] }, null, 3);
            PopulateStack(retVal[EndScenario.F213], foldNode, new[] { pushNodes[1] }, new[] { pushNodes[0] }, new[] { pushNodes[2] }, 3);
            PopulateStack(retVal[EndScenario.F231], foldNode, new[] { pushNodes[2] }, new[] { pushNodes[0] }, new[] { pushNodes[1] }, 3);
            PopulateStack(retVal[EndScenario.F311], foldNode, new[] { pushNodes[1], pushNodes[2] }, new[] { pushNodes[0] }, null, 3);
            PopulateStack(retVal[EndScenario.F312], foldNode, new[] { pushNodes[1] }, new[] { pushNodes[2] }, new GameTreeNode[] { pushNodes[0] }, 3);
            PopulateStack(retVal[EndScenario.F321], foldNode, new[] { pushNodes[2] }, new[] { pushNodes[1] }, new[] { pushNodes[0] }, 3);

            foreach (var key in retVal.Keys)
            {
                double[] endstack = retVal[key];
                Debug.Assert(Math.Abs(endstack.Sum() - leaf.HandInfo.Stacks.Sum()) < 0.001);
                if (Math.Abs(endstack.Sum() - leaf.HandInfo.Stacks.Sum()) > 0.001)
                {
                    throw new Exception();
                }
            }
            return retVal;
        }

        private static void PopulateStack(double[] endStack, GameTreeNode foldNode, GameTreeNode[] firstPlaces,
            GameTreeNode[] secondPlaces, GameTreeNode[] thirdPlaces, int pusherCount)
        {

            double[] effectiveStack = new double[endStack.Length];

            double remainingFoldedCount = foldNode == null ? 0d : foldNode.MandatoryPutInPot;
            double[] remainingPushedCount = new double[pusherCount];
            Dictionary<int, List<int>> remainingPusherPos = new Dictionary<int, List<int>>();

            if (foldNode != null)
            {
                endStack[foldNode.Position.PlayerPosition] -= foldNode.MandatoryPutInPot;
            }

            List<GameTreeNode> allParticipateNodes = new List<GameTreeNode>();
            allParticipateNodes.AddRange(firstPlaces);
            if (secondPlaces != null)
            allParticipateNodes.AddRange(secondPlaces);
            if (thirdPlaces != null)
                allParticipateNodes.AddRange(thirdPlaces);

            if (foldNode != null)
            allParticipateNodes.Add(foldNode);

            for (int i = 0; i < endStack.Length; i++)
            {
                if (allParticipateNodes.Any(t => t.Position.PlayerPosition == i))
                {
                    double effective = 0;
                    for (int j = 0; j < endStack.Length; j++)
                    {
                        if (j != i && allParticipateNodes.Any(t=>t.Position.PlayerPosition == j))
                        {
                            if (effective < endStack[j])
                            {
                                effective = endStack[j];
                            }
                        }
                    }
                    effectiveStack[i] = Math.Min(endStack[i], effective);
                }
            }




            int index = 0;
            var firstPlacesNodes = firstPlaces.OrderBy(t => t.PutInPot).ToArray();
            for (int i = 0; i < firstPlacesNodes.Length; i++)
            {
                endStack[firstPlacesNodes[i].Position.PlayerPosition] -= Math.Min(firstPlacesNodes[i].PutInPot, effectiveStack[firstPlacesNodes[i].Position.PlayerPosition]);
                remainingPushedCount[index] = Math.Min(firstPlacesNodes[i].PutInPot, effectiveStack[firstPlacesNodes[i].Position.PlayerPosition]);
                remainingPusherPos.Add(index, new List<int>());
                for (int j = i; j < firstPlacesNodes.Length; j++)
                {
                    remainingPusherPos[index].Add(firstPlacesNodes[j].Position.PlayerPosition);
                }
                index++;
            }

            if (secondPlaces != null)
            {
                var secondPlacesNodes = secondPlaces.OrderBy(t => t.PutInPot).ToArray();
                for (int i = 0; i < secondPlacesNodes.Length; i++)
                {
                    endStack[secondPlacesNodes[i].Position.PlayerPosition] -= Math.Min(secondPlacesNodes[i].PutInPot, effectiveStack[secondPlacesNodes[i].Position.PlayerPosition]);
                    remainingPushedCount[index] = Math.Min(secondPlacesNodes[i].PutInPot, effectiveStack[secondPlacesNodes[i].Position.PlayerPosition]);
                    remainingPusherPos.Add(index, new List<int>());
                    for (int j = i; j < secondPlacesNodes.Length; j++)
                    {
                        remainingPusherPos[index].Add(secondPlacesNodes[j].Position.PlayerPosition);
                    }
                    index++;
                }

                if (thirdPlaces != null)
                {
                    Debug.Assert(thirdPlaces.Length == 1);
                    endStack[thirdPlaces[0].Position.PlayerPosition] -= Math.Min(thirdPlaces[0].PutInPot, effectiveStack[thirdPlaces[0].Position.PlayerPosition]);
                    remainingPushedCount[index] = Math.Min(thirdPlaces[0].PutInPot, effectiveStack[thirdPlaces[0].Position.PlayerPosition]);
                    remainingPusherPos.Add(index, new List<int>());
                    remainingPusherPos[index].Add(thirdPlaces[0].Position.PlayerPosition);
                }
            }


            for (int i = 0; i < remainingPushedCount.Length; i++)
            {
                double deductAmount = remainingPushedCount[i];
                for (int j = i; j < remainingPushedCount.Length; j++)
                {
                    double totalWon = 0;
                    if (remainingPushedCount[j] >= deductAmount)
                    {
                        totalWon += deductAmount;
                        remainingPushedCount[j] -= deductAmount;
                    }
                    else
                    {
                        totalWon += remainingPushedCount[j];
                        remainingPushedCount[j] = 0d;
                    }

                    if (remainingFoldedCount >= deductAmount)
                    {
                        totalWon += deductAmount;
                        remainingFoldedCount -= deductAmount;
                    }
                    else
                    {
                        totalWon += remainingFoldedCount;
                        remainingFoldedCount = 0d;
                    }

                    foreach (int pos in remainingPusherPos[i])
                    {
                        endStack[pos] += (totalWon / remainingPusherPos[i].Count);
                    }
                }
            }

            if (remainingFoldedCount > 0)
            {
                endStack[foldNode.Position.PlayerPosition] += remainingFoldedCount;
            }

            for (int i = 0; i < remainingPushedCount.Length; i++)
            {
                if (remainingPushedCount[i] > 0)
                {
                    endStack[remainingPusherPos[i][0]] += remainingPushedCount[i];
                }
            }

            for (int i = 0; i < endStack.Length; i++)
            {
                Debug.Assert(endStack[i] >= 0);
                if (endStack[i] < 0)
                {
                    throw new Exception();
                }
            }
        }
    }
}
