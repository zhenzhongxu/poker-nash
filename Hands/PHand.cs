using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.Hands
{
    public enum HandType
    {
        Pair,
        Suited,
        Offsuit
    }

    public class PHand
    {
        public static Dictionary<int, Dictionary<int, Int64>> MaskDict = new Dictionary<int, Dictionary<int, Int64>>();
        public static Int64 LastMask = 0;

        public Int64 CardAMask { get; private set; }
        public Int64 CardBMask { get; private set; }
        public int SuitMask { get; private set; }

        public int FirstRank { get; private set; }

        public int SecondRank { get; private set; }
        public int FirstSuit { get; private set; }
        public int SecondSuit { get; private set; }
        public string FullHandStr { get; private set; }
        public HandType HandType { get; private set; }
        public int HandIndex { get; private set; }

        public int HandGroupIndex { get; private set; }

        public PHand HandGroupRepresentative { get; set; }

        public PHand(string handStr, HandType handType, int suitMask, int firstSuit, int secondSuit, int handIndex, int handGroupIndex)
        {
            this.HandType = handType;
            this.SuitMask = suitMask;
            this.FirstSuit = firstSuit;
            this.SecondSuit = secondSuit;
            this.HandIndex = handIndex;
            this.FirstRank = HandUtility.CardRankDict[handStr[0]];
            this.SecondRank = HandUtility.CardRankDict[handStr[1]];
            this.HandGroupIndex = handGroupIndex;

            this.FullHandStr = handStr[0].ToString() + HandUtility.SuitArray[firstSuit].ToString() + handStr[1].ToString() + HandUtility.SuitArray[secondSuit].ToString();

            if (handType == HandType.Offsuit)
            {
                this.HandGroupString = handStr+"o";
            }
            else if (handType == HandType.Suited)
            {
                this.HandGroupString = handStr + "s";
            }
            else
            {
                this.HandGroupString = handStr;
            }
            if (!MaskDict.ContainsKey(this.FirstRank))
            {
                Dictionary<int, Int64> childDict = new Dictionary<int, Int64>();
                childDict.Add(this.FirstSuit, GetCurrentMask());
                MaskDict.Add(this.FirstRank, childDict);
            }
            else if (!MaskDict[this.FirstRank].ContainsKey(this.FirstSuit))
            {
                MaskDict[this.FirstRank].Add(this.FirstSuit, GetCurrentMask());
            }

            this.CardAMask = MaskDict[this.FirstRank][this.FirstSuit];


            if (!MaskDict.ContainsKey(this.SecondRank))
            {
                Dictionary<int, Int64> childDict = new Dictionary<int, Int64>();
                childDict.Add(this.SecondSuit, GetCurrentMask());
                MaskDict.Add(this.SecondRank, childDict);
            }
            else if (!MaskDict[this.SecondRank].ContainsKey(this.SecondSuit))
            {
                MaskDict[this.SecondRank].Add(this.SecondSuit, GetCurrentMask());
            }

            this.CardBMask = MaskDict[this.SecondRank][this.SecondSuit];
        }

        public string HandGroupString
        {
            get;
            private set;
        }

        private static Int64 GetCurrentMask()
        {
            if (LastMask == 0)
            {
                LastMask = 1;
                return 1;
            }
            else
            {
                LastMask = LastMask << 1;
                return LastMask;
            }
        }
    }
}
