using System;

namespace Namiko {
    internal static class Constants {
        private static Random rnd = new Random();


        //Begging
        public const int begChance = 4;
        public static bool beg { get { return rnd.Next(Constants.begChance) == 0; } }
        public static int begAmount { get { return 5 + rnd.Next(21); } }

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
        public const int PremiumMarriageLimit = 3;


        // WAIFUS

        //waifu costs - set in tiers
        public const int tier3 = 5000;
        public const int tier2 = 10000;
        public const int tier1 = 20000;
        public const int tier0 = 100000;

        //waifushop
        public const int shoplimitedamount = 1;
        public const int shopt1amount = 2;
        public const int shopt2amount = 7;
        public const int shopt3amount = 6;

        //gachashop
        public const int gachatotal = 4;
        public const int gachalimitedamount = 0;
        public const int gachat1amount = 1;
        public const int gachat2amount = 2;
        public const int gachat3amount = 3;
    }
}