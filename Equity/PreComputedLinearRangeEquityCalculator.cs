using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    class PreComputedLinearRangeEquityCalculator : IEquityCalculator
    {
        private static PreComputedLinearRangeEquityCalculator instance;
        private static readonly object syncObj = new object();

        private long[] rawData;

        private PreComputedLinearRangeEquityCalculator()
        {
            rawData = new long[169*169*5];
            var defaultPath = Path.Combine(DefaultPath, @"169LinearRangeEquity2Way.dat");
            FileInfo fi = new FileInfo(defaultPath);

            int index = 0;
            try
            {
                using (FileStream fs = fi.OpenRead())
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        while (index < this.rawData.Length)
                        {
                            rawData[index] = br.ReadInt64();
                            index++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Please try again later, data file is still being downloaded.");
            }

        }

        public static PreComputedLinearRangeEquityCalculator Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new PreComputedLinearRangeEquityCalculator();
                    }
                }
                return instance;
            }

        }

        public static string DefaultPath { get; set; }


        public void CaculateTwoWay(PHand[] handRangeA, PHand[] handRangeB, out long[] win, out long[] tie, out long[] loss, out long total, out double[] winEquity, out double[] tieEquity, out double[] totalEquity)
        {
            win = new long[2];
            tie = new long[2];
            loss = new long[2];
            winEquity = null;
            tieEquity = null;
            totalEquity = null;

            int index = ((handRangeA.Count() - 1)*169 + (handRangeB.Count() - 1))*5;
            win[0] = rawData[index];
            win[1] = rawData[index + 1];
            tie[0] = rawData[index + 2];
            tie[1] = rawData[index + 3];
            total = rawData[index + 4];
        }

        public void CalculateThreeWay(PHand[] handRangeA, PHand[] handRangeB, PHand[] handRangeC, out long[] win, out long[] tie, out long[] loss, out long total, out double[] winEquity, out double[] tieEquity, out double[] totalEquity)
        {
            throw new NotImplementedException();
        }
    }
}
