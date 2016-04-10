using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.Equity
{
    internal sealed class LoopCCombinationIndexer
    {
        private readonly int[] currentSizeLevel1;
        private readonly int[] currentSizeLevel2;

        public LoopCCombinationIndexer()
        {
            this.currentSizeLevel1 = new int[169];
            this.currentSizeLevel2 = new int[]
            {
              14365,14196,14028,13861,13695,13530,13366,13203,13041,12880,12720,12561,12403,12246,12090,11935,11781,11628,11476,11325,11175,11026,10878,10731,10585,10440,10296,10153,10011,9870,9730,9591,9453,9316,9180,9045,8911,8778,8646,8515,8385,8256,8128,8001,7875,7750,7626,7503,7381,7260,7140,7021,6903,6786,6670,6555,6441,6328,6216,6105,5995,5886,5778,5671,5565,5460,5356,5253,5151,5050,4950,4851,4753,4656,4560,4465,4371,4278,4186,4095,4005,3916,3828,3741,3655,3570,3486,3403,3321,3240,3160,3081,3003,2926,2850,2775,2701,2628,2556,2485,2415,2346,2278,2211,2145,2080,2016,1953,1891,1830,1770,1711,1653,1596,1540,1485,1431,1378,1326,1275,1225,1176,1128,1081,1035,990,946,903,861,820,780,741,703,666,630,595,561,528,496,465,435,406,378,351,325,300,276,253,231,210,190,171,153,136,120,105,91,78,66,55,45,36,28,21,15,10,6,3,1  
            };

            this.Init();

        }

        private void Init()
        {
            //for (int i = 0; i < 169; i++)
            //{
            //    int n = 169 - i;
            //    int res = checked((int)(Factoria(n + 1)/(Factoria(n - 1)*2)));
            //    currentSizeLevel2[i] = res;
            //}

            for (int i = 1; i < 169; i++)
            {
                currentSizeLevel1[i] = currentSizeLevel1[i - 1] + currentSizeLevel2[i - 1];
            }
        }

        public int GetIndex(int handAIndex, int handBIndex, int handCIndex)
        {
            // handIndexes are 0 baseds
            Debug.Assert(handAIndex <= handBIndex);
            Debug.Assert(handBIndex <= handCIndex);
            int level1 = this.currentSizeLevel1[handAIndex];
            int level2 = handAIndex == handBIndex ? 0 : (this.currentSizeLevel2[handAIndex] - this.currentSizeLevel2[handBIndex]);
            int level3 = handCIndex - handBIndex;

            return level1 + level2 + level3;
        }


    }

    internal sealed class LoopBCombinationIndexer
    {
        private readonly int[] currentSizeLevel1;
        private readonly int[] currentSizeLevel2;

        public LoopBCombinationIndexer()
        {
            this.currentSizeLevel1 = new int[169];
            this.currentSizeLevel2 = new[]
            {
              14365,14196,14028,13861,13695,13530,13366,13203,13041,12880,12720,12561,12403,12246,12090,11935,11781,11628,11476,11325,11175,11026,10878,10731,10585,10440,10296,10153,10011,9870,9730,9591,9453,9316,9180,9045,8911,8778,8646,8515,8385,8256,8128,8001,7875,7750,7626,7503,7381,7260,7140,7021,6903,6786,6670,6555,6441,6328,6216,6105,5995,5886,5778,5671,5565,5460,5356,5253,5151,5050,4950,4851,4753,4656,4560,4465,4371,4278,4186,4095,4005,3916,3828,3741,3655,3570,3486,3403,3321,3240,3160,3081,3003,2926,2850,2775,2701,2628,2556,2485,2415,2346,2278,2211,2145,2080,2016,1953,1891,1830,1770,1711,1653,1596,1540,1485,1431,1378,1326,1275,1225,1176,1128,1081,1035,990,946,903,861,820,780,741,703,666,630,595,561,528,496,465,435,406,378,351,325,300,276,253,231,210,190,171,153,136,120,105,91,78,66,55,45,36,28,21,15,10,6,3,1  
            };

            this.Init();

        }

        private void Init()
        {
            for (int i = 1; i < 169; i++)
            {
                currentSizeLevel1[i] = currentSizeLevel1[i - 1] + currentSizeLevel2[i - 1];
            }
        }

        public int GetIndex(int handAIndex, int handBIndex, int handCIndex)
        {
            // handIndexes are 0 baseds
            Debug.Assert(handAIndex <= handBIndex);
            Debug.Assert(handBIndex <= handCIndex);
            int level1 = this.currentSizeLevel1[handAIndex];
            int level2 = handAIndex == handCIndex ? 0 : (this.currentSizeLevel2[169 - (handCIndex - handAIndex)]);
            int level3 = handBIndex - handAIndex;

            return level1 + level2 + level3;
        }


    }

    internal sealed class LoopACombinationIndexer
    {
        private readonly int[] currentSizeLevel1;
        private readonly int[] currentSizeLevel2;

        public LoopACombinationIndexer()
        {
            this.currentSizeLevel1 = new int[169];
            this.currentSizeLevel2 = new[]
            {
              169,336,501,664,825,984,1141,1296,1449,1600,1749,1896,2041,2184,2325,2464,2601,2736,2869,3000,3129,3256,3381,3504,3625,3744,3861,3976,4089,4200,4309,4416,4521,4624,4725,4824,4921,5016,5109,5200,5289,5376,5461,5544,5625,5704,5781,5856,5929,6000,6069,6136,6201,6264,6325,6384,6441,6496,6549,6600,6649,6696,6741,6784,6825,6864,6901,6936,6969,7000,7029,7056,7081,7104,7125,7144,7161,7176,7189,7200,7209,7216,7221,7224,7225,7224,7221,7216,7209,7200,7189,7176,7161,7144,7125,7104,7081,7056,7029,7000,6969,6936,6901,6864,6825,6784,6741,6696,6649,6600,6549,6496,6441,6384,6325,6264,6201,6136,6069,6000,5929,5856,5781,5704,5625,5544,5461,5376,5289,5200,5109,5016,4921,4824,4725,4624,4521,4416,4309,4200,4089,3976,3861,3744,3625,3504,3381,3256,3129,3000,2869,2736,2601,2464,2325,2184,2041,1896,1749,1600,1449,1296,1141,984,825,664,501,336,169
            };

            this.Init();

        }

        private void Init()
        {
            for (int i = 1; i < 169; i++)
            {
                currentSizeLevel1[i] = currentSizeLevel1[i - 1] + currentSizeLevel2[i - 1];
            }
        }

        public int GetIndex(int handAIndex, int handBIndex, int handCIndex)
        {
            // handIndexes are 0 baseds
            Debug.Assert(handAIndex <= handBIndex);
            Debug.Assert(handBIndex <= handCIndex);
            int level1 = this.currentSizeLevel1[handBIndex];
            int level2 = handBIndex == handCIndex ? 0 : (handBIndex + 1)*(handCIndex - handBIndex);
            int level3 = handAIndex;

            return level1 + level2 + level3;
        }


    }
}
