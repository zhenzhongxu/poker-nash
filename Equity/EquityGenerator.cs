using System;
using System.IO;
using GameTreeDraft.GameTree;
using GameTreeDraft.Hands;
using POPOKR.Poker.XHandEval.PokerSource.Holdem;

namespace GameTreeDraft.Equity
{
    public class EquityGenerator
    {
        public void GenerateTwoWayEquity(FileInfo fi)
        {
            DateTime startTime = DateTime.UtcNow;
            int count = 0;
            long[] wins = new long[2];
            long[] ties = new long[2];
            long[] losses = new long[2];
            long tot = 0;
            if (fi.Exists)
            {
                fi.Delete();
            }

            using (FileStream fs = fi.OpenWrite())
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                    {
                        for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                        {

                            PHand handA = HandRange.Instance.Hand169RangeAll[i];
                            PHand handB = HandRange.Instance.Hand169RangeAll[j];

                            var hashes = TwoWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB);

                            foreach (int hash in hashes)
                            {
                                PHand testHandA, testHandB;
                                TwoWayHandHash.Instance.GenerateHandFromHash(handA.FirstRank, handA.SecondRank,
                                    handB.FirstRank,
                                    handB.SecondRank,
                                    hash, out testHandA, out testHandB);


                                Hand.HandOdds(new[] { testHandA.FullHandStr, testHandB.FullHandStr },
                                    string.Empty,
                                    string.Empty,
                                    wins,
                                    ties,
                                    losses,
                                    ref tot
                                    );

                                bw.Write(wins[0]);
                                bw.Write(wins[1]);
                                bw.Write(ties[0]);
                                bw.Write(ties[1]);
                                bw.Write(losses[0]);
                                bw.Write(losses[1]);
                                bw.Write(tot);

                                if (count % 20 == 0)
                                {
                                    DateTime endTime = DateTime.UtcNow;
                                    Console.WriteLine("Processed {0} hands, {1} vs {2}. Avg {3} milsec /hand", count, handA.FullHandStr, handB.FullHandStr,
                                        (endTime - startTime).TotalMilliseconds / count);
                                }
                                count++;
                            }
                        }
                    }
                }
            }
        }


        public void GenerateTwoWayOutcomes(FileStream fs)
        {
            DateTime startTime = DateTime.UtcNow;
            long tot = 0;
            int count = 0;
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                {
                    for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                    {
                        PHand handA = HandRange.Instance.Hand169RangeAll[i];
                        PHand handB = HandRange.Instance.Hand169RangeAll[j];

                        var hashes = TwoWayHandHash.Instance.GenerateSuitHashBitMasks(handA, handB);

                        foreach (int hash in hashes)
                        {

                            PHand testHandA, testHandB;
                            TwoWayHandHash.Instance.GenerateHandFromHash(
                                handA.FirstRank,
                                handA.SecondRank,
                                handB.FirstRank,
                                handB.SecondRank,
                                hash, out testHandA, out testHandB);

                            if (!HandUtility.HasConflict(testHandA, testHandB))
                            {
                                var outcomes = Hand.TwoHandOutcomes(
                                    new[] { testHandA.FullHandStr, testHandB.FullHandStr },
                                    ref tot
                                    );

                                foreach (long outcome in outcomes)
                                {
                                    bw.Write(checked((int)outcome));
                                }
                                bw.Write(checked((int)tot));
                                count++;
                                if (count % 25 == 0)
                                {
                                    DateTime endTime = DateTime.UtcNow;
                                    Console.WriteLine("Processed {0} hands, {1} vs {2}. Avg {3} milsec /hand", count, handA.FullHandStr, handB.FullHandStr,
                                    (endTime - startTime).TotalMilliseconds / count);
                                }
                            }
                        }
                    }
                }
            }
        }



        public void GenerateThreeWayOutcomes(FileStream fs, int startIndex, int endIndex)
        {
            DateTime startTime = DateTime.UtcNow;
            long tot = 0;

            int index = 0;
            int count = 0;
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < HandRange.Instance.Hand169RangeAll.Count; i++)
                {
                    for (int j = i; j < HandRange.Instance.Hand169RangeAll.Count; j++)
                    {
                        for (int k = j; k < HandRange.Instance.Hand169RangeAll.Count; k++)
                        {
                            if (index >= startIndex && index <= endIndex)
                            {
                                PHand handA = HandRange.Instance.Hand169RangeAll[i];
                                PHand handB = HandRange.Instance.Hand169RangeAll[j];
                                PHand handC = HandRange.Instance.Hand169RangeAll[k];

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
                                        var outcomes = Hand.ThreeHandOutcomes(
                                            new[] { testHandA.FullHandStr, testHandB.FullHandStr, testHandC.FullHandStr },
                                            ref tot
                                            );

                                        for (int o = 0; o < outcomes.Length; o++)
                                        {
                                            bw.Write(((int)outcomes[o]));
#if DEBUG
                                            Console.WriteLine("{0}:{1:p3}", ((EndScenario)EndScenario.F111 + o), outcomes[o] / (double)tot);

#endif
                                        }
                                        bw.Write(checked((int)tot));
                                        count++;
                                        if (count % 2000 == 0)
                                        {
                                            DateTime endTime = DateTime.UtcNow;
                                            Console.WriteLine("Processed {0} hands, {1} vs {2} vs {3}. Avg {4} milsec /hand", count, handA.FullHandStr, handB.FullHandStr, handC.FullHandStr,
                                            (endTime - startTime).TotalMilliseconds / count);
                                        }
                                    }
                                }
                            }
                            index++;

                        }
                    }
                }
            }
        }

        public void GenerateThreeWayOutcomes(FileInfo fi)
        {
            using (FileStream fs = fi.OpenWrite())
            {
                GenerateThreeWayOutcomes(fs, 0, 818804);
            }
        }
    }
}
