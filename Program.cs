using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using GameTreeDraft.Equity;
using GameTreeDraft.GameTree;
using GameTreeDraft.Hands;
using GameTreeDraft.ICM;
using GameTreeDraft.Utility;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft
{

    class Program
    {
        private static void exp()
        {
            const int total = 169 * 169 * 169 * 14;
            long[] longArr = new long[total];
            for (int i = 0; i < total; i++)
            {
                longArr[i] = i;
            }

            Random rnd = new Random();
            Stopwatch w = new Stopwatch();

            w.Start();

            long sum = 0;
            for (int i = 0; i < 100000000; i++)
            {
                int index = rnd.Next(169 * 169 * 169 * 14);
                var test = longArr[index];
            }


            w.Stop();
            Console.WriteLine("sum = {0}", sum);
            var duration = w.Elapsed;
            Console.WriteLine("duration: {0}", duration);


            w.Reset();
            w.Start();

            sum = 0;
            unsafe
            {
                fixed (long* ptr = longArr)
                {
                    for (int i = 0; i < 100000000; i++)
                    {
                        int index = rnd.Next(169 * 169 * 169 * 14);
                        var test = *(ptr + index);
                    }
                }
            }


            w.Stop();
            Console.WriteLine("sum = {0}", sum);
            duration = w.Elapsed;
            Console.WriteLine("duration: {0}", duration);




            Console.ReadLine();

            return;

        }

        private static void Main(string[] args)
        {


            Globals.HandRankingPath = Environment.CurrentDirectory;
            Globals.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            Globals.TotalMissesTolerated = 169;
            PreComputedLinearRangeEquityCalculator.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            PreComputedHandEquityCalculator.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            PreComputedEquityCalculator.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            PreComputedEquityCalculator3Way.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            ThreeWayFullOutcomeCalculator.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            TwoWayFullOutcomeCalculator.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");
            PreComputedOutcomeCaculator2Way.DefaultPath = Path.Combine(Environment.CurrentDirectory, "Data");

            //var handA = HandRange.Instance.LookupHandGroupHands("AQs").ToArray();
            //List<PHand> handB = new List<PHand>();
            //handB.Add(HandRange.Instance.LookupHandGroup("AA"));
            //handB.Add(HandRange.Instance.LookupHandGroup("76s"));

            //long total;
            //var result1 = PreComputedOutcomeCaculator2Way.Instance.CaculateTwoWay(
            //    new HashSet<PHand>(handA),
            //    new HashSet<PHand>(handB),
            //    out total
            //    );

            //for (int i = 0; i < result1.Length; i++)
            //{
            //    Console.WriteLine(result1[i]);
            //}

            int totalIterations = 0;
            for (int a = 0; a < 10; a++)
            {
                DateTime now = DateTime.UtcNow;
                double[] stack = { 15, 20, 8};
                double[] payouts = { 10};
                double ante = 0d;


                GameTreeNode.GlobalId = 0;
                var root = GameTreeFactory.Create(new GameInfo() { Sb = 1, Bb = 2, Payouts = payouts, Stacks = stack, Ante = ante });
                GameTreeNode.GlobalId = 0;



                root.PopulatePossibleEndScenarios();

                NashOptimizer nashCalc = new NashOptimizer(root);


                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool linearConvered = false;
                for (int i = 0; i < 300; i++)
                {
                    if (!linearConvered)
                    {
                        linearConvered = nashCalc.CalculateOptimalLinearRange(false);
                    }


                    if (linearConvered)
                    {
                        bool unstrictedConverged = false;
                        unstrictedConverged = nashCalc.CalculateOptimalUnrestrictedRange(false);
                        if (unstrictedConverged)
                        {
                            break;
                        }
                    }
                    totalIterations++;
                }

                stopwatch.Stop();
                var result = nashCalc.GetCurrentEquity();


                for (int i = 0; i < result.Length; i++)
                {
                    var equity = Icm.GetEquity(result, payouts, i);
                    Console.WriteLine("Player {0} Equity: {1} ({2})", i + 1, equity, equity / payouts.Sum());
                }

                Console.WriteLine("Elapsed: {0}ms", stopwatch.ElapsedMilliseconds);
                Console.WriteLine("Total iterations: {0}", totalIterations);


                Console.Write(root.PrintEquity());
            }
        }



        private static void PresetRangeTree(GameTreeNode root)
        {
            //15 20 7 linear
            PresetBtn(root);
            PresetSb1(root.FoldBranch);
            PresetSb2(root.PushBranch);
            PresetBB4(root.FoldBranch.PushBranch);
            PresetBB5(root.PushBranch.FoldBranch);
            PresetBB6(root.PushBranch.PushBranch);
        }

        private static void PresetBtn(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }


        private static void PresetSb1(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("93s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);

        }
        private static void PresetSb2(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }


        private static void PresetBB4(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }
        private static void PresetBB5(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }

        private static void PresetBB6(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }

        private static void PresetRange2020BB(GameTreeNode node)
        {
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3o"), true);


            node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T2s"), true);
            // node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            ////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            //////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            ////////node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //node.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }

        //private void test()
        //{
        //long[] win, tie, loss, win1, win2, tie1, tie2, loss1, loss2 = new long[2];
        //double[] winEq, tieEq, totalEq, winEq1, winEq2, tieEq1, tieEq2, totalEq1, totalEq2 = new double[2];
        //long totalHand, totalHand1, totalHand2;

        //    var rangeA = root.PushBranch.Range.GenerateCurrentRangeFullHands();
        //    var rangeB = root.Range.GenerateCurrentRangeFullHands();

        //    var hashSetA = new HashSet<PHand>(rangeA);
        //    var hashSetB = new HashSet<PHand>(rangeB);

        //    PreComputedEquityCalculator.Instance.CaculateTwoWay(
        //         rangeA,
        //         rangeB,
        //         out win, out tie, out loss,
        //         out totalHand,
        //         out winEq,
        //         out tieEq,
        //         out totalEq
        //         );

        //    DifferentialRangeEquityCalculatorcs instance = new DifferentialRangeEquityCalculatorcs(PreComputedEquityCalculator.Instance);

        //    var instanceResult = instance.CaculateTwoWayBaseEquity(
        //        hashSetA,
        //        hashSetB);

        //    var newHand = HandRange.Instance.LookupHand("KhKc");


        //    var newRangeB = new List<PHand>(rangeB);
        //    newRangeB.Add(newHand);
        //    PreComputedEquityCalculator.Instance.CaculateTwoWay(
        //     rangeA,
        //     newRangeB.ToArray(),
        //     out win1, out tie1, out loss1,
        //     out totalHand1,
        //     out winEq1,
        //     out tieEq1,
        //     out totalEq1
        //     );


        //    var instanceResult2 = instance.Calculate2WayDifferentialEquity(1, newHand,
        //        DifferentialHandOp.Add);

        //    var instanceResult3 = instance.Calculate2WayDifferentialEquity(1, newHand,
        //        DifferentialHandOp.Remove);
        //}




        //private static void PresetRange(GameTreeNode root)
        //{


        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AA")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KK")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("QQ")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("JJ")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("TT")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("99")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("88")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("77")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("66")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("55")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("44")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("33")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("22")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AKs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AQs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AJs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("ATs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A9s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A8s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A7s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A6s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A5s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A4s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A3s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A2s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AKo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AQo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("AJo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("ATo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A9o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A8o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A7o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A6o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A5o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A4o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A3o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("A2o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KQs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KJs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KTs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K9s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K8s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K7s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K6s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K5s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K4s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K3s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K2s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KQo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KJo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("KTo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K9o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K8o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K7o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K6o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K5o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K4o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K3o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("K2o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("QJs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("QTs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q9s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q8s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q7s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q6s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q5s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q4s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q3s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q2s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("QJo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("QTo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q9o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q8o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q7o")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q6o")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("Q5o")] = true;


        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("JTs")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J9s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J8s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J7s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J6s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J5s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J4s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J3s")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J2s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("JTo")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J9o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J8o")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("J7o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T9s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T8s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T7s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T6s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T5s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T4s")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T3s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T9o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T8o")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("T7o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("98s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("97s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("96s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("95s")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("94s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("98o")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("97o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("87s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("86s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("85s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("84s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("87o")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("86o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("76s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("75s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("74s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("76o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("65s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("64s")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("63s")] = true;
        //    //root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("65o")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("54s")] = true;
        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("53s")] = true;

        //    root.Range.AllRangeIndicator[HandRange.Instance.LookupHandGroup("43s")] = true;
        //}

        private static void PresetRange2020Root(GameTreeNode root)
        {


            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);


            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }

        private static void PresetRange1(GameTreeNode root)
        {


            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AA"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KK"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QQ"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JJ"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("TT"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("99"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("88"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("77"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("66"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("55"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("44"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("33"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("22"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AKo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AQo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("AJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("ATo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A8o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A7o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A6o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A5o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A4o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A3o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("A2o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KQo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("KTo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K9o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K8o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K7o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K6o"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K5o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K4o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K3o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("K2o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q4s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q3s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QJo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("QTo"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q9o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q8o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q7o"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q6o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("Q5o"), true);


            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTs"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J6s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J5s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J4s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J3s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J2s"), true);
            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("JTo"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J9o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J8o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("J7o"), true);

            root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T6s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T5s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T4s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T3s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T9o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T8o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("T7o"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("96s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("95s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("94s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("98o"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("97o"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("85s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("84s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("87o"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("86o"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("75s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("74s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("76o"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("64s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("63s"), true);
            ////root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("65o"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("54s"), true);
            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("53s"), true);

            //root.Range.ToggleHandInRange(HandRange.Instance.LookupHandGroup("43s"), true);
        }

        private static void Generate1692WayHandOutcomes()
        {
            int count = 0;
            int total = 169 * 169;

            FileInfo fi = new FileInfo(@"D:\169HandOutcomes2Way.dat");
            if (fi.Exists)
            {
                fi.Delete();
            }

            int handAIndex = 0, handBIndex = 0;
            using (FileStream fs = fi.OpenWrite())
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    DateTime start = DateTime.UtcNow;
                    foreach (PHand handA in HandRange.Instance.Hand169RangeAll)
                    {
                        handAIndex++;
                        handBIndex = 0;
                        foreach (PHand handB in HandRange.Instance.Hand169RangeAll)
                        {
                            handBIndex++;

                            long totalHand = 0;

                            long[] outcomes = TwoWayFullOutcomeCalculator.Instance.CalculateTwoWay(
                                HandRange.Instance.LookupHandGroupHands(handA.HandGroupString).ToArray(),
                                HandRange.Instance.LookupHandGroupHands(handB.HandGroupString).ToArray(),
                                out totalHand
                                );

                            foreach (long outcome in outcomes)
                            {
                                bw.Write(outcome);
                            }
                            bw.Write(totalHand);

                            count++;

                            if (count % 5 == 0)
                            {
                                DateTime end = DateTime.UtcNow;
                                Console.WriteLine(
                                    "Processing {0} / {1} hands {2} vs {3} .. Avg processing speed {4:N1} millseconds / hand. Currently {5:P2} done.",
                                    count, total, handAIndex, handBIndex,
                                    (end - start).TotalMilliseconds / count, count / (double)total);
                            }
                        }

                    }
                }
            }
        }

        private static void Generate1693WayHandOutcomes()
        {
            int count = 0;
            int total = 790244;

            FileInfo fi = new FileInfo(@"D:\169HandOutcomes3Way.dat");
            if (fi.Exists)
            {
                fi.Delete();
            }

            int indexCount = 0;
            using (FileStream fs = fi.OpenWrite())
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    DateTime start = DateTime.UtcNow;
                    List<PHand> hand169Range = HandRange.Instance.Hand169RangeAll;
                    for (int i = 0; i < hand169Range.Count; i++)
                    {
                        for (int j = i; j < hand169Range.Count; j++)
                        {
                            for (int k = j; k < hand169Range.Count; k++)
                            {
                                long[] outcomes;
                                long totalHand = 0;

                                outcomes = ThreeWayFullOutcomeCalculator.Instance.CalculateThreeWay(
                                    HandRange.Instance.LookupHandGroupHands(hand169Range[i].HandGroupString).ToArray(),
                                    HandRange.Instance.LookupHandGroupHands(hand169Range[j].HandGroupString).ToArray(),
                                    HandRange.Instance.LookupHandGroupHands(hand169Range[k].HandGroupString).ToArray(),
                                    out totalHand
                                    );

                                foreach (long outcome in outcomes)
                                {
                                    bw.Write(outcome);
                                    indexCount++;
                                }
                                bw.Write(totalHand);

                                count++;

                                if (count % 5 == 0)
                                {
                                    DateTime end = DateTime.UtcNow;
                                    Console.WriteLine(
                                        "Processing {0} / {1} hands {2} vs {3} vs {4} .. Avg processing speed {5:N1} millseconds / hand. Currently {6:P2} done.",
                                        count, total, i, j, k,
                                        (end - start).TotalMilliseconds / count, count / (double)total);
                                }
                            }
                        }
                    }
                }
            }
        }

        //private static void Generate1693WayHandEquity()
        //{
        //    int count = 0;
        //    int total = 169 * 169 * 169;

        //    FileInfo fi = new FileInfo(@"D:\169HandEquity3Way.dat");
        //    if (fi.Exists)
        //    {
        //        fi.Delete();
        //    }

        //    int handAIndex= 0, handBIndex = 0, handCindex = 0;
        //    using (FileStream fs = fi.OpenWrite())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(fs))
        //        {
        //            DateTime start = DateTime.UtcNow;
        //            foreach (PHand handA in HandRange.Instance.Hand169RangeAll)
        //            {
        //                handAIndex++;
        //                handBIndex = 0;
        //                handCindex = 0;
        //                foreach (PHand handB in HandRange.Instance.Hand169RangeAll)
        //                {
        //                    handBIndex++;
        //                    handCindex = 0;
        //                    foreach (PHand handC in HandRange.Instance.Hand169RangeAll)
        //                    {

        //                        handCindex ++;
        //                        long[] win, tie, loss = new long[3];
        //                        double[] winEq, tieEq, totalEq = new double[3];
        //                        long totalHand;

        //                        PreComputedEquityCalculator3Way.Instance.CalculateThreeWay(
        //                            HandRange.Instance.LookupHandGroupHands(handA.HandGroupString).ToArray(),
        //                            HandRange.Instance.LookupHandGroupHands(handB.HandGroupString).ToArray(),
        //                            HandRange.Instance.LookupHandGroupHands(handC.HandGroupString).ToArray(),
        //                            out win, out tie, out loss,
        //                            out totalHand,
        //                            out winEq,
        //                            out tieEq,
        //                            out totalEq
        //                            );

        //                        bw.Write(win[0]);
        //                        bw.Write(win[1]);
        //                        bw.Write(win[2]);
        //                        bw.Write(tie[0]);
        //                        bw.Write(tie[1]);
        //                        bw.Write(tie[2]);
        //                        bw.Write(loss[0]);
        //                        bw.Write(loss[1]);
        //                        bw.Write(loss[2]);

        //                        bw.Write(totalHand);

        //                        count++;

        //                        if (count%50 == 0)
        //                        {
        //                            DateTime end = DateTime.UtcNow;
        //                            Console.WriteLine(
        //                                "Processing {0} / {1} hands {2} vs {3} vs {4}... Avg processing speed {5:N1} millseconds / hand. Currently {6:P2} done.",
        //                                count, total, handAIndex, handBIndex, handCindex,
        //                                (end - start).TotalMilliseconds/count, count/(double) total);
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    }
        //}

        //private static void Generate1692Way()
        //{
        //    // linear range outcomes
        //    int count = 0;
        //    int total = 169 * 169;

        //    FileInfo fi = new FileInfo(@"D:\169LinearRangeOutcome2Way.dat");
        //    if (fi.Exists)
        //    {
        //        fi.Delete();
        //    }

        //    using (FileStream fs = fi.OpenWrite())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(fs))
        //        {
        //            DateTime start = DateTime.UtcNow;
        //            // generate for push range
        //            var pushRangeA = PushRange.CreateDefault(null);
        //            foreach (PHand handA in pushRangeA.OrderedHands)
        //            {

        //                pushRangeA.ToggleHandInRange(handA, true);

        //                var pushRangeB = CallRange.CreateDefault(null);
        //                foreach (PHand handB in pushRangeB.OrderedHands)
        //                {
        //                    pushRangeB.ToggleHandInRange(handB, true);

        //                    long[] outcomes;
        //                    long totalHand;

        //                    outcomes = PreComputedOutcomeCaculator2Way.Instance.CaculateTwoWay(
        //                        pushRangeA.GenerateCurrentRangeHands(),
        //                        pushRangeB.GenerateCurrentRangeHands(),
        //                        out totalHand
        //                        );

        //                    Debug.Assert(outcomes.Sum() == totalHand);
        //                    bw.Write(outcomes[0]);
        //                    bw.Write(outcomes[1]);
        //                    bw.Write(outcomes[2]);
        //                    bw.Write(totalHand);

        //                    count++;

        //                    if (count % 50 == 0)
        //                    {
        //                        DateTime end = DateTime.UtcNow;
        //                        Console.WriteLine(
        //                            "Processing {0} / {1} hands {2:P2} vs {3:P2}... Avg processing speed {4:N1} millseconds / hand. Currently {5:P2} done.",
        //                            count, total, pushRangeA.Percentage, pushRangeB.Percentage,
        //                            (end - start).TotalMilliseconds / count, count / (double)total);
        //                    }

        //                }
        //            }
        //        }
        //    }
        //}

        private static void Generate1693WayCombo()
        {
            int count = 0;
            int total = 818805;

            FileInfo fi = new FileInfo(@"D:\169HandOutcomes3Way_LoopB.dat");
            if (fi.Exists)
            {
                fi.Delete();
            }

            int indexCount = 0;
            using (FileStream fs = fi.OpenWrite())
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                DateTime start = DateTime.UtcNow;
                List<PHand> hand169Range = HandRange.Instance.Hand169RangeAll;


                for (int j = 0; j < hand169Range.Count; j++)
                {
                    count = 0;
                    for (int k = j; k < hand169Range.Count; k++)
                    {
                        for (int i = 0; i < hand169Range.Count; i++)
                        {

                            if (j < i)
                                continue;

                            //if (i == 0 && j == 0 && k == 111)
                            //{

                            //    long[] outcomes;
                            //    long totalHand = 0;

                            //    outcomes = ThreeWayFullOutcomeCalculator.Instance.CalculateThreeWay(
                            //        HandRange.Instance.LookupHandGroupHands(hand169Range[i].HandGroupString).ToArray(),
                            //        HandRange.Instance.LookupHandGroupHands(hand169Range[j].HandGroupString).ToArray(),
                            //        HandRange.Instance.LookupHandGroupHands(hand169Range[k].HandGroupString).ToArray(),
                            //        out totalHand
                            //        );

                            //    foreach (long outcome in outcomes)
                            //    {

                            //        int intOutcome = checked((int)outcome);
                            //        bw.Write(intOutcome);
                            //        indexCount++;
                            //    }
                            //}
                            //bw.Write(totalHand);

                            count++;

                            //if (count % 5 == 0)
                            //{
                            //    DateTime end = DateTime.UtcNow;
                            //    Console.WriteLine(
                            //        "Processing {0} / {1} hands {2} vs {3} vs {4} .. Avg processing speed {5:N1} millseconds / hand. Currently {6:P2} done.",
                            //        count, total, i, j, k,
                            //        (end - start).TotalMilliseconds / count, count / (double)total);
                            //}
                        }

                    }
                    Console.Write(count + ",");
                }
            }


            Console.ReadLine();

        }

        //private static void Generate1693Way()
        //{
        //    // linear range outcomes
        //    int count = 0;
        //    int total = 169 * 169 * 169;

        //    FileInfo fi = new FileInfo(@"D:\169LinearRangeOutcome3Way.dat");
        //    if (fi.Exists)
        //    {
        //        fi.Delete();
        //    }

        //    long[] totalOutcomes = new long[13];
        //    long totalHands = 0;

        //    using (FileStream fs = fi.OpenWrite())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(fs))
        //        {
        //            DateTime start = DateTime.UtcNow;
        //            // generate for push range
        //            var pushRangeA = PushRange.CreateDefault(null);
        //            foreach (PHand handA in pushRangeA.OrderedHands)
        //            {
        //                pushRangeA.ToggleHandInRange(handA, true);
        //                var callRangeB = CallRange.CreateDefault(null);

        //                foreach (PHand handB in callRangeB.OrderedHands)
        //                {
        //                    callRangeB.ToggleHandInRange(handB, true);
        //                    var callRangeC = CallRange.CreateDefault(null);

        //                    totalOutcomes = new long[13];
        //                    totalHands = 0;

        //                    foreach (PHand handC in callRangeC.OrderedHands)
        //                    {
        //                        callRangeC.ToggleHandInRange(handC, true);

        //                        long[] outcomes = new long[13];
        //                        long tot = 0;

        //                        outcomes = PreComputedOutcomeCalculator3Way.Instance.CalculateThreeWay(
        //                         pushRangeA.GenerateCurrentRangeHands().ToArray(),
        //                         callRangeB.GenerateCurrentRangeHands().ToArray(),
        //                         new [] { handC },
        //                         out tot
        //                         );

        //                        for (int i = 0; i < 13; i++)
        //                        {
        //                            totalOutcomes[i] += outcomes[i];
        //                            bw.Write(totalOutcomes[i]);
        //                        }

        //                        totalHands += tot;
        //                        bw.Write(totalHands);

        //                        count++;

        //                        if (count % 50 == 0)
        //                        {
        //                            DateTime end = DateTime.UtcNow;
        //                            Console.WriteLine(
        //                                "Processing {0} / {1} hands {2:P2} vs {3:P2} vs {4:P2} ... Avg processing speed {5:N1} millseconds / hand. Currently {6:P2} done.",
        //                                count, total, pushRangeA.Percentage, callRangeB.Percentage,
        //                                callRangeC.Percentage,
        //                                (end - start).TotalMilliseconds / count, count / (double)total);
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    }

        //}


        private static void CombineFiles()
        {
            DirectoryInfo di = new DirectoryInfo(@"D:\Poker\Equity\3wayOutcomes");
            FileInfo[] files = di.GetFiles().OrderBy(t => t.FullName).ToArray();

            FileInfo output = new FileInfo(@"D:\Poker\Equity\3WayFullOutcomes.dat");

            if (output.Exists)
            {
                output.Delete();
            }

            using (FileStream ofs = output.OpenWrite())
            {
                using (BinaryWriter bw = new BinaryWriter(ofs))
                {
                    foreach (FileInfo fi in files)
                    {
                        string[] content = fi.Name.Split(new char[] { '_', '.' });
                        int startIndex = Convert.ToInt32(content[1]);
                        int endIndex = Convert.ToInt32(content[2]);

                        var expected = GetExpectedFileSize(startIndex, endIndex);
                        if (expected != fi.Length && endIndex > 57499)
                        {
                            throw new Exception("bad file");
                        }
                        else
                        {
                            if (endIndex <= 57499)
                            {
                                using (FileStream ifs = fi.OpenRead())
                                {
                                    using (BinaryReader br = new BinaryReader(ifs))
                                    {

                                        while (ifs.Position < ifs.Length)
                                        {
                                            bw.Write(checked((int)br.ReadInt64()));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream ifs = fi.OpenRead())
                                {
                                    ifs.CopyTo(bw.BaseStream);
                                }
                            }
                        }
                    }
                }
            }
        }


        private static void ValidateFiles()
        {
            DirectoryInfo di = new DirectoryInfo(@"D:\Poker\Equity\3wayOutcomes");
            FileInfo[] files = di.GetFiles().OrderBy(t => t.FullName).ToArray();

            List<string> fileNames = new List<string>();
            List<string> goodFiles = new List<string>();

            foreach (FileInfo fi in files)
            {
                string[] content = fi.Name.Split(new char[] { '_', '.' });
                int startIndex = Convert.ToInt32(content[1]);
                int endIndex = Convert.ToInt32(content[2]);

                var expected = GetExpectedFileSize(startIndex, endIndex);
                if (expected != fi.Length)
                {
                    fileNames.Add(fi.Name);
                }
                else
                {
                    goodFiles.Add(fi.Name);
                }

            }
        }

        private static int GetExpectedFileSize(int startIndex, int endIndex)
        {
            int index = 0;
            int count = 0;
            for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
            {
                PHand handA = HandRange.Instance.Hand169RangeAll[i];
                for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                {
                    PHand handB = HandRange.Instance.Hand169RangeAll[j];

                    for (int k = j; k < HandRange.Instance.Hand169RangeAll.Count; k++)
                    {
                        PHand handC = HandRange.Instance.Hand169RangeAll[k];

                        if (index >= startIndex && index <= endIndex)
                        {
                            var hashes = ThreeWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB, handC);
                            foreach (int hash in hashes)
                            {
                                PHand testHandA, testHandB, testHandC;

                                ThreeWayHandHash.Instance.GenerateHandFromHash(handA.FirstRank, handA.SecondRank,
                                    handB.FirstRank,
                                    handB.SecondRank,
                                    handC.FirstRank,
                                    handC.SecondRank,
                                    hash, out testHandA, out testHandB, out testHandC);

                                if (!HandUtility.HasConflict(testHandA, testHandB, testHandC))
                                {

                                    count++;
                                }
                            }
                        }

                        index++;
                    }
                }
            }

            return count * 14 * 4;
        }


    }
}
