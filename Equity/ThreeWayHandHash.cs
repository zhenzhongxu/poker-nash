using System;
using System.Collections.Generic;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class ThreeWayHandHash
    {
        private static readonly ThreeWayHandHash handHash = new ThreeWayHandHash();
        private Dictionary<HandType, Dictionary<HandType, Dictionary<HandType, Dictionary<int, HashSet<int>>>>> suitComboBitMaskDict;

        private ThreeWayHandHash()
        {
            this.InitSuitComboDict();
        }

        private void InitSuitComboDict()
        {
            DateTime start = DateTime.Now;

            suitComboBitMaskDict = new Dictionary<HandType, Dictionary<HandType, Dictionary<HandType, Dictionary<int, HashSet<int>>>>>();
            HandType[] handTypes = { HandType.Pair, HandType.Offsuit, HandType.Suited };

            for (int i = 0; i < 3; i++)
            {
                suitComboBitMaskDict.Add(handTypes[i], new Dictionary<HandType, Dictionary<HandType, Dictionary<int, HashSet<int>>>>());
                for (int j = i; j < 3; j++)
                {
                    suitComboBitMaskDict[handTypes[i]].Add(handTypes[j], new Dictionary<HandType, Dictionary<int, HashSet<int>>>());
                    for (int k = j; k < 3; k++)
                    {
                        suitComboBitMaskDict[handTypes[i]][handTypes[j]].Add(handTypes[k],
                            new Dictionary<int, HashSet<int>>
                                {
                                    {0, new HashSet<int>()},
                                    {1, new HashSet<int>()},
                                    {2, new HashSet<int>()},
                                    {3, new HashSet<int>()}
                                });
                    }
                }
            }


            int count = 0;
            int lastHandGroupIndex = -1;
            HandType? lastHandType = null;

            for (int i = 0; i < HandRange.Instance.HandRangeAll.Count; i++)
            {
                PHand hand1 = HandRange.Instance.HandRangeAll[i];
                if (lastHandGroupIndex == -1 || (lastHandType != null && hand1.HandType != lastHandType))
                {
                    lastHandGroupIndex = hand1.HandGroupIndex;
                    lastHandType = hand1.HandType;
                }

                if (lastHandGroupIndex == hand1.HandGroupIndex)
                {
                    for (int j = i + 1; j < HandRange.Instance.HandRangeAll.Count; j++)
                    {
                        for (int k = j + 1; k < HandRange.Instance.HandRangeAll.Count; k++)
                        {

                            PHand hand2 = HandRange.Instance.HandRangeAll[j];
                            PHand hand3 = HandRange.Instance.HandRangeAll[k];
                            if (!HandUtility.HasConflict(hand1, hand2, hand3))
                            {

                                int suitBitMask = this.GetHash(hand1, hand2, hand3) & 4095;
                                int handDupIndex = this.GetHandGroupDupIndex(hand1, hand2, hand3);
                                if (
                                    suitComboBitMaskDict[hand1.HandType][hand2.HandType][hand3.HandType][handDupIndex]
                                        .Add(
                                            suitBitMask))
                                {
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            DateTime end = DateTime.Now;
        }


        public static ThreeWayHandHash Instance
        {
            get { return handHash; }
        }


        public int GetHash(PHand handA, PHand handB, PHand handC)
        {
            if (handA.HandIndex > handB.HandIndex)
            {
                return GetHash(handB, handA, handC);
            }
            if (handB.HandIndex > handC.HandIndex)
            {
                return GetHash(handA, handC, handB);
            }

            int hashBitMask = 0;// ((((((handA.FirstRank << 5) | handA.SecondRank) << 5) | (handB.FirstRank)) << 5) | handB.SecondRank) << 2;


            int[] suitArray = new[] { handA.FirstSuit, handA.SecondSuit, handB.FirstSuit, handB.SecondSuit, handC.FirstSuit, handC.SecondSuit };

            int currentSuitIndex = 0;
            for (int i = 0; i < suitArray.Length; i++)
            {
                if (suitArray[i] > currentSuitIndex)
                {
                    for (int j = i + 1; j < suitArray.Length; j++)
                    {
                        if (suitArray[j] == suitArray[i])
                        {
                            suitArray[j] = currentSuitIndex;
                        }
                        else if (suitArray[j] == currentSuitIndex)
                        {
                            suitArray[j] = suitArray[i];
                        }
                    }

                    suitArray[i] = currentSuitIndex;
                    currentSuitIndex++;
                }
                else if (suitArray[i] == currentSuitIndex)
                {
                    currentSuitIndex++;
                }
            }


            hashBitMask = (((((((((hashBitMask | suitArray[0]) << 2) | suitArray[1]) << 2 | suitArray[2]) << 2) | suitArray[3]) << 2)
                | suitArray[4]) << 2) | suitArray[5];


            return hashBitMask;
        }

        public void GenerateHandFromHash(int handARank1, int handARank2, int handBRank1, int handBRank2, int handCRank1, int handCRank2,
            int hash, out PHand handA, out PHand handB, out PHand handC)
        {
            //TODO: optimize
            var cardRankA1 = HandUtility.CardRankArray[handARank1];
            var cardRankA2 = HandUtility.CardRankArray[handARank2];

            var cardRankB1 = HandUtility.CardRankArray[handBRank1];
            var cardRankB2 = HandUtility.CardRankArray[handBRank2];

            var cardRankC1 = HandUtility.CardRankArray[handCRank1];
            var cardRankC2 = HandUtility.CardRankArray[handCRank2];

            var suitA1 = HandUtility.SuitArray[hash >> 10 & 3];
            var suitA2 = HandUtility.SuitArray[hash >> 8 & 3];
            var suitB1 = HandUtility.SuitArray[hash >> 6 & 3];
            var suitB2 = HandUtility.SuitArray[hash >> 4 & 3];

            var suitC1 = HandUtility.SuitArray[hash >> 2 & 3];
            var suitC2 = HandUtility.SuitArray[hash & 3];

            handA = HandRange.Instance.LookupHand(cardRankA1.ToString() + suitA1.ToString() + cardRankA2.ToString() +
                                              suitA2.ToString());
            handB = HandRange.Instance.LookupHand(cardRankB1.ToString() + suitB1.ToString() + cardRankB2.ToString() +
                                              suitB2.ToString());

            handC = HandRange.Instance.LookupHand(cardRankC1.ToString() + suitC1.ToString() + cardRankC2.ToString() +
                                              suitC2.ToString());

        }

        public HashSet<int> GenerateSuitHashBitMasks(PHand handA, PHand handB, PHand handC)
        {
            if (handA.HandGroupIndex > handB.HandGroupIndex)
            {
                return this.GenerateSuitHashBitMasks(handB, handA, handC);
            }
            if (handB.HandGroupIndex > handC.HandGroupIndex)
            {
                return this.GenerateSuitHashBitMasks(handA, handC, handB);
            }
            int index = GetHandGroupDupIndex(handA, handB, handC);
            return this.suitComboBitMaskDict[handA.HandType][handB.HandType][handC.HandType][index];

        }

        public int GetHandGroupDupIndex(PHand handA, PHand handB, PHand handC)
        {
            if (handA.HandGroupIndex == handB.HandGroupIndex && handA.HandGroupIndex == handC.HandGroupIndex)
            {
                return 0;
            }
            else if (handA.HandGroupIndex != handB.HandGroupIndex && handA.HandGroupIndex != handC.HandGroupIndex
                     && handB.HandGroupIndex != handC.HandGroupIndex)
            {
                return 1;
            }
            else if (handA.HandGroupIndex != handB.HandGroupIndex)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}
