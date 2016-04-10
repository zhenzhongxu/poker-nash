using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.Hands
{
    public class HandRange
    {
        private static readonly HandRange handRange = new HandRange();
        private readonly List<PHand> range = new List<PHand>();
        private readonly List<PHand> Hand169Range = new List<PHand>();
        private readonly Dictionary<string, PHand> Hand169RangeDict = new Dictionary<string, PHand>();
        private readonly Dictionary<string, List<PHand>> handGroupDict = new Dictionary<string, List<PHand>>();


        private readonly Dictionary<string, PHand> handDict = new Dictionary<string, PHand>();

        private HandRange()
        {

            string cardStr = "AKQJT98765432";
            int index = 0;
            int handGroup = 0;

            for (int i = 0; i < cardStr.Length; i++)
            {
                this.AddRange(this.ProducePairRange(cardStr[i], ref index, handGroup));
                handGroup++;
            }

            for (int i = 0; i < cardStr.Length; i++)
            {
                for (int j = i + 1; j < cardStr.Length; j++)
                {
                    this.AddRange(this.ProduceOffsuitRange(cardStr[i], cardStr[j], ref index, handGroup));
                    handGroup++;
                }
            }

            for (int i = 0; i < cardStr.Length; i++)
            {
                for (int j = i + 1; j < cardStr.Length; j++)
                {
                    this.AddRange(this.ProduceSuitedRange(cardStr[i], cardStr[j], ref index, handGroup));
                    handGroup++;
                }
            }
        }



        public List<PHand> HandRangeAll
        {
            get { return range; }
        }

        public List<PHand> Hand169RangeAll
        {
            get { return Hand169Range; }
        }


        private List<PHand> ProducePairRange(char p, ref int index, int handGroupIndex)
        {
            List<PHand> result = new List<PHand>();
            PHand representative = null;
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    result.Add(new PHand(
                        p.ToString() + p.ToString(),
                        HandType.Pair,
                        HandUtility.SuitMaskArray[i] | HandUtility.SuitMaskArray[j],
                        i,
                        j,
                        index,
                        handGroupIndex));
                    index++;
                }
            }
            return result;
        }

        private IEnumerable<PHand> ProduceOffsuitRange(char p1, char p2, ref int index, int handGroupIndex)
        {
            List<PHand> result = new List<PHand>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i != j)
                    {
                        result.Add(new PHand(
                            p1.ToString() + p2.ToString(),
                            HandType.Offsuit,
                            HandUtility.SuitMaskArray[i] | HandUtility.SuitMaskArray[j],
                            i,
                            j,
                            index,
                            handGroupIndex));
                        index++;
                    }

                }
            }
            return result;
        }


        private IEnumerable<PHand> ProduceSuitedRange(char p1, char p2, ref int index, int handGroupIndex)
        {
            List<PHand> result = new List<PHand>();
            for (int i = 0; i < 4; i++)
            {
                result.Add(
                    new PHand(p1.ToString() + p2.ToString(),
                        HandType.Suited,
                        HandUtility.SuitMaskArray[i],
                        i,
                        i,
                        index,
                        handGroupIndex));
                index++;
            }
            return result;
        }



        private void AddRange(IEnumerable<PHand> handRange)
        {

            range.AddRange(handRange);

            var handGroup = handRange.ToArray()[0];
            Hand169Range.Add(handGroup);
            this.handGroupDict.Add(handGroup.HandGroupString, handRange.ToList());

            Hand169RangeDict.Add(handGroup.HandGroupString, handGroup);

            foreach (PHand hand in handRange)
            {
                this.handDict.Add(hand.FullHandStr, hand);
                
                hand.HandGroupRepresentative = handRange.ToArray()[0];
            }
        }

        public static HandRange Instance
        {
            get
            {
                return handRange;
            }
        }

        public PHand LookupHandGroup(string handGroupStr)
        {
            return Hand169RangeDict[handGroupStr];
        }

        public List<PHand> LookupHandGroupHands(string handGroupStr)
        {
            return this.handGroupDict[handGroupStr];
        }

        public PHand LookupHand(string handStr)
        {
            if (this.handDict.ContainsKey(handStr))
            {
                return handDict[handStr];
            }
            else
            {
                return handDict[handStr.Substring(2, 2) + handStr.Substring(0, 2)];
            }
        }
    }
}
