using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.GameTree
{
    public enum PlayerAction
    {
        Raise,
        Fold,
        Call
    }

    public interface IAction
    {
        IGameTreeNode Parent { get; }
        IGameTreeNode Child { get; }
    }

}
