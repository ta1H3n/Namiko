using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    [Flags]
    public enum LootBoxType : int
    {
        Vote = 1,
        Premium = 2
    }

    [Flags]
    public enum ChannelType
    {
        Reddit = 1
    }

    [Flags]
    public enum PremiumType : ulong
    {
        HomeGuildId_NOTAPREMIUMTYPE = 418900885079588884,
        ServerT1 = 581848782396981250,
        ServerT2 = 581849178381221900,
        Toastie = 581849224661172244,
        Waifu = 581849327274557451
    }

    [Flags]
    public enum ShopType
    {
        Waifu = 1,
        Gacha = 2
    }
}
