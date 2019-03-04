using System;

namespace Namiko.Core {
    internal static class Cost {
        private static Random rnd = new Random();


         //Costs
        //waifu costs - set in tiers
        public const int tier3 = 5000;
        public const int tier2 = 10000;
        public const int tier1 = 20000;
        public const int tier0 = 100000;
        
        //colour, dont laugh at my coments
        public const int colour = 0;



         //Caps
        //daily and weekly caps
        public const int dailycap = 2500;
        public static int weeklycap { get { return 3500 - rnd.Next(1000); } }

        //quote + aggregate user caps
        public const int quoteCap = 400; 
        public const int aggregateCap = 4;
    }
}