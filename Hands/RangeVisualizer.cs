using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.GameTree;

namespace GameTreeDraft.Hands
{
    public class RangeVisualizer
    {
        public static void PrintRange(IRange range, StringBuilder sb)
        {
            if (range == null)
            {
                return;
            }

            bool[,] boolMap;
            string[,] handMap;
            GetHandMap(range, out boolMap, out handMap);

            //sb.AppendFormat("-------------------------- Begin Range {0} --------------------", range.ParentNode.Descripton);
            //sb.AppendLine();
            //for (int i = 0; i < 13; i++)
            //{
            //    for (int j = 0; j < 13; j++)
            //    {
            //        sb.AppendFormat("| {0,3}:{1} ", handMap[i, j], boolMap[i, j] ? "Y" : "N");

            //    }
            //    sb.AppendLine("|");
            //}
            sb.AppendLine(GetRangeShortStr(boolMap, handMap));
            //sb.AppendFormat("-------------------------- End Range {0} --------------------", range.ParentNode.Descripton);
            sb.AppendLine();

        }

        public static string GetRangeString(IRange range)
        {
            if (range == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            bool[,] boolMap;
            string[,] handMap;
            GetHandMap(range, out boolMap, out handMap);

            sb.AppendLine(String.Format("-------------------------- Begin Range {0} --------------------", range.ParentNode.Descripton));
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    sb.AppendLine(String.Format("| {0,3}:{1} ", handMap[i, j], boolMap[i, j] ? "Y" : "N"));
                }
                sb.AppendLine("|");
            }
            sb.AppendLine(String.Format("-------------------------- End Range {0} --------------------", range.ParentNode.Descripton));

            return sb.ToString();
        }


        public static void GetHandMap(IRange range, out bool[,] booleanMap, out string[,] handMap)
        {
            booleanMap = new bool[13, 13];
            handMap = new string[13, 13];

            foreach (PHand hand in range.OrderedHands)
            {
                int indexA, indexB;
                if (hand.HandType == HandType.Pair)
                {
                    indexA = 12 - hand.FirstRank;
                    indexB = 12 - hand.SecondRank;
                }
                else if (hand.HandType == HandType.Suited)
                {
                    indexA = 12 - hand.FirstRank;
                    indexB = 12 - hand.SecondRank;
                }
                else
                {
                    indexA = 12 - hand.SecondRank;
                    indexB = 12 - hand.FirstRank;
                }
                handMap[indexA, indexB] = hand.HandGroupString;
                booleanMap[indexA, indexB] = range.IsHandToggled(hand);
            }

        }

        internal static string GetRangeShortStr(bool[,] boolMap, string[,] handMap)
        {
            StringBuilder sb = new StringBuilder();

            string startHand = null;
            string endHand = null;
            //pair
            for (int i = 0; i < 13; i++)
            {
                if (boolMap[i, i])
                {
                    if (startHand == null)
                    {
                        startHand = handMap[i, i];
                        endHand = startHand;
                    }
                    else
                    {
                        endHand = handMap[i, i];
                    }
                }
                if ((i == 12 || !boolMap[i, i]) && startHand != null)
                {
                    if (startHand != endHand)
                    {
                        if (startHand == handMap[0, 0])
                        {
                            sb.AppendFormat("{0}+,", endHand);
                        }
                        else
                        {
                            sb.AppendFormat("{0}-{1},", startHand, endHand);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0},", startHand);
                    }
                    startHand = null;
                    endHand = null;
                }
            }

            //suited
            for (int i = 0; i < 13; i++)
            {
                string suitedString = string.Empty;
                string unsuitedString = string.Empty;
                bool suitedFull = false;
                bool unsuitedFull = false;
               
                startHand = null;
                endHand = null;
                for (int j = i + 1; j < 13; j++)
                {
                    if (boolMap[i, j])
                    {
                        if (startHand == null)
                        {
                            startHand = handMap[i, j];
                            endHand = startHand;
                        }
                        else
                        {
                            endHand = handMap[i, j];
                        }

                    }

                    if ((j == 12 || !boolMap[i, j]) && startHand != null)
                    {
                        if (startHand != endHand)
                        {
                            if (startHand == handMap[i, i + 1])
                            {
                                suitedString = String.Format("{0}+,", endHand);
                                if (handMap[i, 12] == endHand)
                                {
                                    suitedFull = true;
                                }
                            }
                            else
                            {
                                suitedString = String.Format("{0}-{1},", startHand, endHand);
                            }
                        }
                        else
                        {
                            suitedString = string.Format("{0},", startHand);
                        }
                        startHand = null;
                        endHand = null;
                    }
                }

                startHand = null;
                endHand = null;
                for (int j = i + 1; j < 13; j++)
                {
                    if (boolMap[j, i])
                    {
                        if (startHand == null)
                        {
                            startHand = handMap[j, i];
                            endHand = startHand;
                        }
                        else
                        {
                            endHand = handMap[j, i];
                        }
                    }
                    if ((j == 12 || !boolMap[j, i]) && startHand != null)
                    {
                        if (startHand != endHand)
                        {
                            if (startHand == handMap[i + 1, i])
                            {
                                unsuitedString = String.Format("{0}+,", endHand);
                                if (handMap[12, i] == endHand)
                                {
                                    unsuitedFull = true;
                                }
                            }
                            else
                            {
                                unsuitedString = string.Format("{0}-{1},", startHand, endHand);
                            }
                        }
                        else
                        {
                            unsuitedString = string.Format("{0},", startHand);
                        }
                        startHand = null;
                        endHand = null;
                    }
                }

                if (suitedFull && unsuitedFull)
                {
                    sb.AppendFormat("{0}x+,", suitedString.Substring(0, 1));
                }
                else
                {
                    sb.Append(suitedString);
                    sb.Append(unsuitedString);
                }
            }

            return sb.ToString();
        }

        internal static string GetRangeShortStr(IRange range)
        {
            bool[,] boolMap;
            string[,] handMap;
            GetHandMap(range, out boolMap, out handMap);

            return GetRangeShortStr(boolMap, handMap);

        }
    }
}
