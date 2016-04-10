using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GameTreeDraft.Hands;

namespace GameTreeDraft.Equity
{
    public interface IEquityCalculator
    {
        void CaculateTwoWay(PHand[] handRangeA, PHand[] handRangeB, out long[] win, out long[] tie, out long[] loss,
            out long total, out double[] winEquity, out double[] tieEquity, out double[] totalEquity);

        void CalculateThreeWay(PHand[] handRangeA, PHand[] handRangeB, PHand[] handRangeC, out long[] win, out long[] tie, out long[] loss,
            out long total, out double[] winEquity, out double[] tieEquity, out double[] totalEquity);
    }
}
