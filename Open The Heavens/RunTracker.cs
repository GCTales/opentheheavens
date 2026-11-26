using System;
using System.Collections.Generic;
using System.Text;

namespace Open_The_Heavens
{
    public static class RunTracker
    {
        private static float _teamCleansedHexes;
        public static float teamCleansedHexes
        {
            get { return _teamCleansedHexes; }
            set { _teamCleansedHexes = value; }
        }

        //public static int gildedIdolsTotal;
        //public static bool usedReincarnation;

        internal static float debugDiffCoeff = 0;

        internal static void Init()
        {
            teamCleansedHexes = 0;
            //gildedIdolsTotal = 0;
            debugDiffCoeff = 0;
        }
    }
}
