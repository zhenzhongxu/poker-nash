using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.ICM
{
    public class Icm
    {
        public static double GetEquity(double[] stacks, double[] payouts, int player)
        {
            double total = 0;
            for (int i = 0; i < stacks.Length; i++)
                total += stacks[i];
            return GetEquity(stacks, payouts, total, player, 0);
        }

        //Recursive method doing the actual calculation.
        public static double GetEquity(double[] stacks, double[] payouts, double total, int player, int depth)
        {
            double eq = stacks[player] / total * payouts[depth];

            if (depth + 1 < payouts.Length)
                for (int i = 0; i < stacks.Length; i++)
                    if (i != player && stacks[i] > 0.0)
                    {
                        double c = stacks[i];
                        stacks[i] = 0.0F;
                        eq += GetEquity(stacks, payouts, total - c, player, depth + 1) * c / total;
                        stacks[i] = c;
                    }

            return eq;
        }
    }
}
