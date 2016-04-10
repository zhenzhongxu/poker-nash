using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public class TwoWayHandHash
    {
        private static readonly TwoWayHandHash handHash = new TwoWayHandHash();
        private Dictionary<HandType, Dictionary<HandType, Dictionary<bool, HashSet<int>>>> suitComboBitMaskDict;

        private TwoWayHandHash()
        {
            this.InitSuitComboDict();
        }

        private void InitSuitComboDict()
        {
            suitComboBitMaskDict = new Dictionary<HandType, Dictionary<HandType, Dictionary<bool, HashSet<int>>>>
            {
                {
                    HandType.Pair, new Dictionary<HandType, Dictionary<bool, HashSet<int>>>()
                    {
                        {
                            HandType.Pair, new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }
                        
                        },
                        {
                            HandType.Offsuit, new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }
                        },
                        {HandType.Suited,new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }}
                    }
                },
                {
                    HandType.Offsuit, new Dictionary<HandType, Dictionary<bool, HashSet<int>>>()
                    {
                        {
                            HandType.Offsuit, new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }
                        },
                        {HandType.Suited,new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }},
                    }
                },
                {
                    HandType.Suited, new Dictionary<HandType, Dictionary<bool, HashSet<int>>>()
                    {
                         {HandType.Suited,new Dictionary<bool, HashSet<int>>
                            {
                                {false, new HashSet<int>()},
                                {true, new HashSet<int>()}
                            }}
                    }
                }
            };

            for (int i = 0; i < HandRange.Instance.HandRangeAll.Count; i++)
            {
                for (int j = i + 1; j < HandRange.Instance.HandRangeAll.Count; j++)
                {
                    PHand hand1 = HandRange.Instance.HandRangeAll[i];
                    PHand hand2 = HandRange.Instance.HandRangeAll[j];
                    if (!HandUtility.HasConflict(hand1, hand2))
                    {
                        int suitBitMask = this.GetHash(hand1, hand2) & 255;
                        bool sameHand = (hand1.HandGroupIndex == hand2.HandGroupIndex);
                        suitComboBitMaskDict[hand1.HandType][hand2.HandType][sameHand].Add(suitBitMask);
                    }
                }
            }

            //for (int i = 0; i < 2; i++)
            //{
            //    currentMax = i;
            //    for (int a = 0; a < 3; a++)
            //    {
            //        if (a > currentMax)
            //        {
            //            currentMax = a;
            //        }
            //        for (int b = 0; b < 4; b++)
            //        {
            //            if (b <= currentMax + 1)
            //            {
            //                if (i == 0)
            //                {
            //                    //hand A suited
            //                    if (a == b)
            //                    {
            //                        // hand B suited
            //                        if (i == a)
            //                        {
            //                            suitComboBitMaskDict[HandType.Suited][HandType.Suited][0].Add(bitMask);
            //                        }
            //                        suitComboBitMaskDict[HandType.Suited][HandType.Suited][1].Add(bitMask);
            //                    }
            //                    else
            //                    {
            //                        // hand B offsuit


            //                    }
            //                }
            //                else
            //                {

            //                }
            //            }

            //        }
            //    }
            //}



        }


        public static TwoWayHandHash Instance
        {
            get { return handHash; }
        }


        public int GetHash(PHand handA, PHand handB)
        {
            if (handA.HandIndex > handB.HandIndex)
            {
                PHand tmp = handB;
                handB = handA;
                handA = tmp;
            }

            int hashBitMask = 0; //((((((handA.FirstRank << 5) | handA.SecondRank) << 5) | (handB.FirstRank)) << 5) |handB.SecondRank) << 2;
            

            int[] suitArray = new[] { handA.FirstSuit, handA.SecondSuit, handB.FirstSuit, handB.SecondSuit };

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


            hashBitMask = ((((((hashBitMask | suitArray[0]) << 2) | suitArray[1]) << 2 | suitArray[2]) << 2) |
                           suitArray[3]) ;


            return hashBitMask;
        }

        public void GenerateHandFromHash(int handARank1, int handARank2, int handBRank1, int handBRank2, int hash, out PHand handA, out PHand handB)
        {
            //TODO: optimize
            var cardRankA1 = HandUtility.CardRankArray[handARank1];
            var cardRankA2 = HandUtility.CardRankArray[handARank2];

            var cardRankB1 = HandUtility.CardRankArray[handBRank1];
            var cardRankB2 = HandUtility.CardRankArray[handBRank2];

            var suitA1 = HandUtility.SuitArray[hash >> 6 & 3];
            var suitA2 = HandUtility.SuitArray[hash >> 4 & 3];
            var suitB1 = HandUtility.SuitArray[hash >> 2 & 3];
            var suitB2 = HandUtility.SuitArray[hash & 3];

            handA = HandRange.Instance.LookupHand(cardRankA1.ToString() + suitA1.ToString() + cardRankA2.ToString() +
                                              suitA2.ToString());
            handB = HandRange.Instance.LookupHand(cardRankB1.ToString() + suitB1.ToString() + cardRankB2.ToString() +
                                              suitB2.ToString());

        }


        public HashSet<int> GenerateSuitHashBitMasks(PHand handA, PHand handB)
        {
            if (handA.HandGroupIndex > handB.HandGroupIndex)
            {
                return this.GenerateSuitHashBitMasks(handB, handA);
            }

            return
                this.suitComboBitMaskDict[handA.HandType][handB.HandType][handA.HandGroupIndex == handB.HandGroupIndex];

        }


    }
}
