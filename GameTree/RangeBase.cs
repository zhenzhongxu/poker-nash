using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.GameTree
{
    public class ByHandGroupId : IComparer<PHand>
    {
       
        public int Compare(PHand x, PHand y)
        {
            return x.HandGroupIndex.CompareTo(y.HandGroupIndex);
        }
    }

    public class RangeBase : IRange
    {
        private Dictionary<Hands.PHand, bool> AllRangeIndicator { get; set; }
        private List<PHand> handsList { get; set; }

        private static readonly int TotalHands = 52 * 51 / 2;

        private int handsToggledOn = 0;

        public double Percentage
        {
            get
            {
                return handsToggledOn / (double)TotalHands;
            }
        }

        public string RangeStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (PHand hand in this.AllRangeIndicator.Keys)
                {
                    if (this.AllRangeIndicator[hand])
                    {
                        sb.Append(hand.HandGroupString + ",");
                    }
                }
                return sb.ToString();

            }
        }

        public string RangeShortStr
        {
            get
            {
                return RangeVisualizer.GetRangeShortStr(this);


            }
        }

        public List<PHand> OrderedHands
        {
            get
            {
                return handsList;
            }
        }

        public RangeBase(List<PHand> hands, GameTreeNode parentNode)
        {
            this.ParentNode = parentNode;
            this.AllRangeIndicator = new Dictionary<PHand, bool>();
            this.handsList = hands;

            for (int i = 0; i < hands.Count; i++)
            {
                if (i == 0)
                {
                    this.AllRangeIndicator.Add(hands[i], false);
                    this.ToggleHandInRange(hands[i], true);
                }
                else
                {
                    this.AllRangeIndicator.Add(hands[i], false);
                }
            }
        }


        public void ToggleHandInRange(PHand hand, bool value)
        {
            if (this.AllRangeIndicator[hand] == value)
            {
                return;
            }
            else
            {
                this.AllRangeIndicator[hand] = value;
                if (value)
                {
                    switch (hand.HandType)
                    {
                        case HandType.Offsuit:
                            handsToggledOn += 12;
                            break;
                        case HandType.Pair:
                            handsToggledOn += 6;
                            break;
                        case HandType.Suited:
                            handsToggledOn += 4;
                            break;
                    }
                }
                else
                {
                    switch (hand.HandType)
                    {
                        case HandType.Offsuit:
                            handsToggledOn -= 12;
                            break;
                        case HandType.Pair:
                            handsToggledOn -= 6;
                            break;
                        case HandType.Suited:
                            handsToggledOn -= 4;
                            break;
                    }
                }
            }
        }

        public bool IsHandToggled(PHand hand)
        {
            return this.AllRangeIndicator[hand];
        }

        public SortedSet<PHand> GenerateCurrentRangeHands()
        {
            SortedSet<PHand> ranges = new SortedSet<PHand>(new ByHandGroupId());
            foreach (PHand hand in AllRangeIndicator.Keys)
            {
                if (AllRangeIndicator[hand])
                {
                    ranges.Add(HandRange.Instance.LookupHandGroup(hand.HandGroupString));
                }
            }

            return ranges;
        }


        //public PHand[] GenerateCurrentRangeFullHands()
        //{
        //    HashSet<PHand> ranges = new HashSet<PHand>();
        //    foreach (PHand hand in AllRangeIndicator.Keys)
        //    {
        //        if (AllRangeIndicator[hand])
        //        {
        //            foreach (PHand phand in HandRange.Instance.LookupHandGroupHands(hand.HandGroupString))
        //            {
        //                ranges.Add(phand);
        //            }
        //        }
        //    }

        //    return ranges.ToArray();
        //}


        public GameTreeNode ParentNode
        {
            get;
            private set;
        }
    }
}
