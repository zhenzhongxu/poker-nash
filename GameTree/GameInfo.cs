using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.GameTree
{
    public class GameInfo
    {
        public double[] Stacks { get; set; }
        public double[] Payouts { get; set; }
        public int Sb { get; set; }
        public int Bb { get; set; }

        public double Ante { get; set; }
    }
}
