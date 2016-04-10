using System;
using System.Collections.Generic;

namespace GameTreeDraft.GameTree
{
    public class GameTreeFactory
    {
        public static GameTreeNode Create(GameInfo gameInfo)
        {
            if (gameInfo == null)
            {
                throw new ArgumentNullException("gameInfo");
            }
            if (gameInfo.Stacks == null || gameInfo.Stacks.Length <= 1 || gameInfo.Stacks.Length > 10)
            {
                throw new ArgumentException("Invalid game stack.");
            }

            if (gameInfo.Payouts == null || gameInfo.Payouts.Length <= 0 ||
                gameInfo.Payouts.Length > gameInfo.Stacks.Length)
            {
                throw new ArgumentException("Invalid payout structure.");
            }

            Position pos = new Position(0, gameInfo.Stacks.Length);
            GameTreeNode root = new GameTreeNode(gameInfo, pos, 0, gameInfo.Stacks[0]);
            Queue<GameTreeNode> queue = new Queue<GameTreeNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                GameTreeNode node = queue.Dequeue();

                // fold
                GameTreeNode child = null;
                if (!node.Position.IsLastToAct)
                {
                    var nextPos = new Position(node.Position.PlayerPosition + 1, gameInfo.Stacks.Length);
                    if (!(nextPos.IsLastToAct && node.PreviousAllInCount == 0))
                    {
                        child = new GameTreeNode(
                            gameInfo,
                            new Position(node.Position.PlayerPosition + 1, gameInfo.Stacks.Length),
                            node.PreviousAllInCount,
                            node.PreviousMaxMallIn
                            );

                    }
                }

                if (child == null)
                {
                    child = new GameTreeNode(gameInfo, null, node.PreviousAllInCount, node.PreviousMaxMallIn);
                }
                node.FoldBranch = child;
                if (!child.IsTerminal)
                {
                    queue.Enqueue(child);
                }

                // push or call
                child = null;
                int totalAllInCount = node.PreviousAllInCount + 1;
                double totalMaxAllInAfterThisPos = 0;
                Position positionParam = null;
                if (!node.Position.IsLastToAct && totalAllInCount < 3)
                {
                    positionParam = new Position(node.Position.PlayerPosition + 1, gameInfo.Stacks.Length);

                }

                IRange range = null;
                if (node.Position.IsLastToAct)
                {
                    // if last to act, can only call
                    totalMaxAllInAfterThisPos = node.PreviousMaxMallIn;
                    range = CallRange.CreateDefault(node);
                }
                else
                {
                    //if (node.PreviousMaxMallIn >= gameInfo.Stacks[node.Position.PlayerPosition + 1])
                    if (node.PreviousAllInCount != 0)
                    {
                        // call
                        totalMaxAllInAfterThisPos = node.PreviousMaxMallIn;
                        range = CallRange.CreateDefault(node);

                    }
                    else
                    {
                        totalMaxAllInAfterThisPos = gameInfo.Stacks[node.Position.PlayerPosition];
                        range = PushRange.CreateDefault(node);
                    }
                }

                child = new GameTreeNode(gameInfo,
                    positionParam,
                    totalAllInCount,
                    totalMaxAllInAfterThisPos);

                node.PushBranch = child;
                node.Range = range;
                if (!child.IsTerminal)
                {
                    queue.Enqueue(child);
                }
            }

            return root;
        }


    }
}
