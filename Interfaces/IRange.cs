using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.GameTree
{
    public interface IRange
    {
        double Percentage { get; }

        string RangeStr { get; }

        string RangeShortStr { get; }

        List<PHand> OrderedHands { get; }

        SortedSet<PHand> GenerateCurrentRangeHands();

        //PHand[] GenerateCurrentRangeFullHands();

        GameTreeNode ParentNode { get; }

        void ToggleHandInRange(PHand hand, bool value);


        bool IsHandToggled(PHand hand);

    }
}
