using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Core
{
    [Flags]
    public enum LootBoxType : int
    {
        Vote = 1
    }

    [Flags]
    public enum ChannelType
    {
        Reddit = 1
    }
}
