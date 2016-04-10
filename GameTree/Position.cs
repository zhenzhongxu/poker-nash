using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTreeDraft.GameTree
{
    public class Position
    {
        public static string[] c_posNames2 = { "SB", "BB" };
        public static string[] c_posNames3 = { "BTN", "SB", "BB" };
        public static string[] c_posNames4 = { "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames5 = { "UTG", "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames6 = { "UTG", "MP", "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames7 = { "UTG", "MP", "MP1", "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames8 = { "UTG", "UTG1", "MP", "MP1", "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames9 = { "UTG", "UTG1", "MP", "MP1", "MP2", "CO", "BTN", "SB", "BB" };
        public static string[] c_posNames10 = { "UTG", "UTG1", "UTG2", "MP", "MP1", "MP2", "CO", "BTN", "SB", "BB" };


        public Position(int playerPosition, int totalPlayers)
        {
            if (totalPlayers <= 0 || totalPlayers > 10)
            {
                throw new ArgumentException("Invalid total players count.");
            }
            if (playerPosition >= totalPlayers || playerPosition < 0)
            {
                throw new ArgumentException("player position is invalid");
            }
            this.TotalPlayers = totalPlayers;
            this.PlayerPosition = playerPosition;
        }

        public int TotalPlayers { get; private set; }

        public int PlayerPosition { get; private set; }

        public bool IsLastToAct
        {
            get { return this.PlayerPosition + 1 == this.TotalPlayers; }
        }

        public string Positon
        {
            get
            {
                switch (this.TotalPlayers)
                {
                    case 2:
                        return c_posNames2[PlayerPosition];
                    case 3:
                        return c_posNames3[PlayerPosition];
                    case 4:
                        return c_posNames4[PlayerPosition];
                    case 5:
                        return c_posNames5[PlayerPosition];
                    case 6:
                        return c_posNames6[PlayerPosition];
                    case 7:
                        return c_posNames7[PlayerPosition];
                    case 8:
                        return c_posNames8[PlayerPosition];
                    case 9:
                        return c_posNames9[PlayerPosition];
                    case 10:
                        return c_posNames10[PlayerPosition];
                }
                return "Unknown";
            }
        }
    }
}
