﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.GameTree
{
    public class PushRange : RangeBase
    {
        public PushRange(List<PHand> hands, GameTreeNode node)
            : base(hands, node)
        {
            if (node != null)
            {
                node.PutInPot = node.OriginalStack;
            }
        }

        public static PushRange CreateDefault(GameTreeNode node)
        {
            FileInfo fi = new FileInfo(Path.Combine(Globals.HandRankingPath, @"HandRanking\nepush.range"));
            string allRangeStr = null;

            using (FileStream fs = fi.OpenRead())
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    allRangeStr = sr.ReadToEnd();
                }
            }

            string[] content = allRangeStr.Split(new char[] { '\n' });

            List<PHand> hands = new List<PHand>();
            foreach (string handStr in content)
            {
                hands.Add(HandRange.Instance.LookupHandGroup(handStr));
            }

            PushRange callRange = new PushRange(hands, node);
            //foreach (PHand phand in callRange.AllRangeIndicator.Keys)
            //{
            //    callRange.AllRangeIndicator[phand] = true;
            //    break;
            //}

            return callRange;
        }
    }
}
