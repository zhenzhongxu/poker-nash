namespace GameTreeDraft.Equity
{
    using System;

    public class RangeEquityResult
    {
        //public int RangeCount { get; private set; }
        private long total;
        public long[] Outcomes { get; internal set; }

        public long Total {
            get { return total; }
            internal set
            {
#if DEBUG
                if (value < 0)
                {
                    Console.WriteLine("Error");
                }
#endif
                this.total = value;
            }
        }

        //hide default constructor
        private RangeEquityResult()
        {
        }

        private RangeEquityResult(long[] outcomes, long total)
        {
            //this.RangeCount = win.Length;
            this.Total = total;
            this.Outcomes = outcomes;
            //this.CalculateEquity();
        }



        public static RangeEquityResult CreateNew(long[] outcomes, long total)
        {
            if (outcomes == null)
            {
                throw new ArgumentNullException();
            }
            if (outcomes.Length != 3 && outcomes.Length != 13)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new RangeEquityResult(outcomes, total);
        }

        public void PerformDifferentialOp(long[] outcomes, long total, DifferentialHandOp differentialHandOp)
        {
            if (outcomes == null)
            {
                throw new ArgumentNullException();
            }

            if (outcomes.Length != 3 && outcomes.Length != 13)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (differentialHandOp == DifferentialHandOp.Add)
            {
                for (int i = 0; i < outcomes.Length; i++)
                {
                    this.Outcomes[i] += outcomes[i];
                }
                long result =checked(this.Total + total);
                ;
                this.Total = result;
            }
            else
            {
                for (int i = 0; i < outcomes.Length; i++)
                {
                    this.Outcomes[i] -= outcomes[i];
                }
                this.Total -= total;
            }

            //this.CalculateEquity();
        }

        private void CalculateEquity()
        {
            throw new NotImplementedException();
            //if (this.RangeCount == 2)
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        this.WinEquity[i] = this.Win[i] / (double)this.Total;
            //        this.TieEquity[i] = this.Tie[i] / (double)this.Total / 2;
            //        this.TotalEquity[i] = this.WinEquity[i] + this.TieEquity[i];
            //    }
            //}
            //else if (this.RangeCount == 3)
            //{
            //    long[] tieToOtherTwo = new long[3];
            //    long[] tieToAllThree = new long[3];
            //    tieToOtherTwo[0] = this.Loss[0] - this.Win[1] - this.Win[2];
            //    tieToOtherTwo[1] = this.Loss[1] - this.Win[0] - this.Win[2];
            //    tieToOtherTwo[2] = this.Loss[2] - this.Win[0] - this.Win[1];

            //    tieToAllThree[0] = this.Tie[0] - tieToOtherTwo[1] - tieToOtherTwo[2];
            //    tieToAllThree[1] = this.Tie[1] - tieToOtherTwo[0] - tieToOtherTwo[2];
            //    tieToAllThree[2] = this.Tie[2] - tieToOtherTwo[0] - tieToOtherTwo[1];

            //    this.TieEquity[0] = (tieToOtherTwo[1] / (double)2 + tieToOtherTwo[2] / (double)2 + tieToAllThree[0] / (double)3) /
            //                   this.Total;
            //    this.TieEquity[1] = (tieToOtherTwo[0] / (double)2 + tieToOtherTwo[2] / (double)2 + tieToAllThree[1] / (double)3) /
            //                   this.Total;
            //    this.TieEquity[2] = (tieToOtherTwo[0] / (double)2 + tieToOtherTwo[1] / (double)2 + tieToAllThree[2] / (double)3) /
            //                   this.Total;

            //    this.WinEquity[0] = this.Win[0] / (double)this.Total;
            //    this.WinEquity[1] = this.Win[1] / (double)this.Total;
            //    this.WinEquity[2] = this.Win[2] / (double)this.Total;

            //    this.TotalEquity[0] = this.WinEquity[0] + this.TieEquity[0];
            //    this.TotalEquity[1] = this.WinEquity[1] + this.TieEquity[1];
            //    this.TotalEquity[2] = this.WinEquity[2] + this.TieEquity[2];
            //}
            //else
            //{
            //    throw new Exception("unsupported");
            //}
        }
    }
}
