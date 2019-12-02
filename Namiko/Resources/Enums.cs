﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    [Flags]
    public enum LootBoxType : int
    {
        Vote = 1,
        Premium = 2,
        WaifuT1 = 3,
        WaifuT2 = 4,
        WaifuT3 = 5
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
        GuildPlus = 581848782396981250,
        Guild = 581849178381221900,
        ProPlus = 581849224661172244,
        Pro = 581849327274557451
    }

    [Flags]
    public enum ShopType
    {
        Waifu = 1,
        Gacha = 2,
        Mod = 3
    }

    [Flags]
    public enum RoleType
    {
        Music
    }
}
