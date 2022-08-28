using Discord.Interactions;

namespace Namiko.Modules.Leaderboard
{
    public class LeaderboardUtil
    {
        
    }

    public enum LeaderboardType
    {
        Rep,
        Vote,
        Toastie,
        [ChoiceDisplay("Daily streak")]
        DailyStreak,
        [ChoiceDisplay("Waifu value")]
        WaifuValue,
        [ChoiceDisplay("Top waifus")]
        TopWaifus
    }

    public enum LeaderboardScope
    {
        [ChoiceDisplay("This server")]
        Server,
        Global
    }
}
