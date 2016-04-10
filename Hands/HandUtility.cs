using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.Hands
{
    public class HandUtility
    {
        public static char[] SuitArray = new[] { 'c', 'd', 'h', 's' };
        public static int[] SuitMaskArray = new[] { 8, 4, 2, 1 };

        public static char[] CardRankArray = new[] { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };

        public static readonly Dictionary<char, int> CardRankDict;

        private Dictionary<int, char> suitBitMaskLut;
        private Dictionary<char, int> suitBitMaskCharLut;

        private Dictionary<int, char> suitValLut;
        private Dictionary<int, char> suitValCharLut;


        private Dictionary<int, char> rankLut;
        private Dictionary<char, int> rankByCharLut;

        private static readonly HandUtility instance = new HandUtility();

        static HandUtility()
        {
            CardRankDict = new Dictionary<char, int>()
            {
                {'2', 0},
                {'3', 1},
                {'4', 2},
                {'5', 3},
                {'6', 4},
                {'7', 5},
                {'8', 6},
                {'9', 7},
                {'T', 8},
                {'J', 9},
                {'Q', 10},
                {'K', 11},
                {'A', 12}
            };
        }

        private HandUtility()
        {
            this.InitializeLookupTables();
        }



        public static HandUtility Instance
        {
            get { return instance; }
        }



        private void InitializeLookupTables()
        {
            suitBitMaskLut = new Dictionary<int, char>();
            suitBitMaskLut.Add(8, 'c');
            suitBitMaskLut.Add(4, 'd');
            suitBitMaskLut.Add(2, 'h');
            suitBitMaskLut.Add(1, 's');

        }


        public static bool HasConflict(PHand handA, PHand handB)
        {

            if (handA.FirstRank == handB.FirstRank && handA.FirstSuit == handB.FirstSuit)
            {
                return true;
            }

            else if (handA.FirstRank == handB.SecondRank && handA.FirstSuit == handB.SecondSuit)
            {
                return true;
            }

            else if (handA.SecondRank == handB.FirstRank && handA.SecondSuit == handB.FirstSuit)
            {
                return true;
            }

            else if (handA.SecondRank == handB.SecondRank && handA.SecondSuit == handB.SecondSuit)
            {
                return true;
            }
            return false;


        }


        public static bool HasConflict(PHand handA, PHand handB, PHand handC)
        {
            var result =  NumberOfSetBits((handA.CardAMask | handA.CardBMask | handB.CardAMask | handB.CardBMask | handC.CardAMask |
                   handC.CardBMask));

            return result != 6;

        }

        private static Int64 NumberOfSetBits(Int64 i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;

        }

        public static List<PHand> GenerateCombos(string hand)
        {

            if (String.IsNullOrEmpty(hand))
            {
                throw new ArgumentException("bad argument");
            }

            List<PHand> result = new List<PHand>();
            if (hand.Length == 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i + 1; j < 4; j++)
                    {
                        result.Add(HandRange.Instance.LookupHand(
                            String.Format("{0}{1}{2}{3}",
                            hand[0], SuitArray[i], hand[1], SuitArray[j])));
                    }
                }
            }
            else if (hand[2] == 'o')
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (i != j)
                        {
                            result.Add(HandRange.Instance.LookupHand(
                            String.Format("{0}{1}{2}{3}",
                            hand[0], SuitArray[i], hand[1], SuitArray[j])));
                        }
                    }
                }
            }
            else if (hand[2] == 's')
            {
                for (int i = 0; i < 4; i++)
                {
                    result.Add(HandRange.Instance.LookupHand(
                           String.Format("{0}{1}{2}{3}",
                           hand[0], SuitArray[i], hand[1], SuitArray[i])));
                }
            }
            else
            {
                throw new Exception("error");
            }

            return result;
        }

    }
}
