using System;

namespace Namiko {
    internal static class Constants {
        private static Random rnd = new Random();

        
        //waifu costs - set in tiers
        public const int tier3 = 5000;
        public const int tier2 = 10000;
        public const int tier1 = 20000;
        public const int tier0 = 100000;
        
        //Begging
        public const int begChance = 4;
        public static bool beg { get { return rnd.Next(Constants.begChance) == 0; } }
        public static int begAmount { get { return 5 + rnd.Next(6); } }
        
        //daily and weekly caps
        public const int dailycap = 2500;
        public static int weeklycap { get { return 3500 - rnd.Next(1000); } }

        //quote + aggregate user caps
        public const int quoteCap = 400; 
        public const int aggregateCap = 4;

        //colour, dont laugh at my coments
        public const int colour = 0;

        //maximum number of partners 
        public const int MarriageLimit = 1;


        // --------------LOOTBOXES----------------- //

        // Vote
        public const int voteWaifuChance = 10;
        public static int voteToastieAmount { get { return 200 + (rnd.Next(9) * 100); } }
    }
}