using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Model;
using Namiko.Addons.Handlers;

namespace Namiko.Handlers.TypeConverters;

public class WaifuConverter : TypeConverter<Waifu>
{
    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var waifus = await WaifuDb.SearchWaifus(option.Value.ToString());

        if (waifus.Count != 1)
        {
            if (context is ICustomContext)
            {
                await ((ICustomContext)context).ReplyAsync(
                    embed: WaifuUtil.FoundWaifusEmbedBuilder(waifus, context.User is IGuildUser ? context.User as IGuildUser : null).Build());

                return TypeConverterResult.FromSuccess(null);
            }
        }
        
        return TypeConverterResult.FromSuccess(waifus.First());
    }
}
