using System;
using System.IO;
using System.Linq;
using GameTreeDraft.Hands;
using GameTreeDraft.Utility;

namespace GameTreeDraft.Equity
{

    public class PreComputedLinearRangeOutcomeCalculator 
    {
        private static readonly PreComputedLinearRangeOutcomeCalculator instance = new PreComputedLinearRangeOutcomeCalculator();

        private long[] rawData2Way;
        private long[] rawData3Way;

        private PreComputedLinearRangeOutcomeCalculator()
        {
            
            var defaultPath = Path.Combine(Globals.DefaultPath, @"169LinearRangeOutcome2Way.dat");
            FileInfo fi = new FileInfo(defaultPath);

            rawData2Way = Utils.ReadFileIntoLongArray(fi);

            defaultPath = Path.Combine(Globals.DefaultPath, @"169LinearRangeOutcome3Way.dat");
            fi = new FileInfo(defaultPath);
            rawData3Way = Utils.ReadFileIntoLongArray(fi);
            
        }

        public static PreComputedLinearRangeOutcomeCalculator Instance
        {
            get
            {
                return instance;
            }

        }

        


        //TODO: change to hashset
        public long[] CaculateTwoWay(PHand[] handRangeA, PHand[] handRangeB, out long total)
        {
            long[] outcomes = new long[3];

            int index = (handRangeA.Count() - 1) * 676 + (handRangeB.Count() - 1) * 4;
            outcomes[0] = rawData2Way[index];
            outcomes[1] = rawData2Way[index+1];
            outcomes[2] = rawData2Way[index+2];
            total = rawData2Way[index + 3];

            return outcomes;
        }

        public long[] CalculateThreeWay(PHand[] handRangeA, PHand[] handRangeB, PHand[] handRangeC, out long total)
        {
            long[] outcomes = new long[13];

            int index = (handRangeA.Count() - 1) * 399854 + (handRangeB.Count() - 1) * 2366 + (handRangeC.Count() - 1) * 14;
            outcomes[0] = rawData3Way[index];
            outcomes[1] = rawData3Way[index + 1];
            outcomes[2] = rawData3Way[index + 2];
            outcomes[3] = rawData3Way[index + 3];
            outcomes[4] = rawData3Way[index + 4];
            outcomes[5] = rawData3Way[index + 5];
            outcomes[6] = rawData3Way[index + 6];
            outcomes[7] = rawData3Way[index + 7];
            outcomes[8] = rawData3Way[index + 8];
            outcomes[9] = rawData3Way[index + 9];
            outcomes[10] = rawData3Way[index + 10];
            outcomes[11] = rawData3Way[index + 11];
            outcomes[12] = rawData3Way[index + 12];

            total = rawData3Way[index + 13];

            return outcomes;
        }
    }
}
