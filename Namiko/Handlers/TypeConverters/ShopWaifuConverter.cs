using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Model;
using Namiko.Addons.Handlers;

namespace Namiko.Handlers.TypeConverters;

public class ShopWaifuConverter : TypeConverter<ShopWaifu>
{
    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var key = option.Value.ToString().Split(":");
        if (key[0] == "key")
        {
            string name = key[1].Split(" ")[0];
            return TypeConverterResult.FromSuccess(await WaifuShopDb.GetWaifuFromShop(context.Guild.Id, name));
        }
        
        var shopwaifus = (await WaifuShopDb.GetAllShopWaifus(context.Guild.Id)).DistinctBy(x => x.WaifuName);
        var waifus = await WaifuDb.SearchWaifus(option.Value.ToString(), false, shopwaifus.Select(x => x.Waifu));

        if (waifus.Count != 1)
        {
            if (context is ICustomContext)
            {
                await ((ICustomContext)context).ReplyAsync(
                    embed: WaifuUtil.FoundWaifusEmbedBuilder(waifus, context.User is IGuildUser ? context.User as IGuildUser : null).Build());

                return TypeConverterResult.FromSuccess(null);
            }
        }
        
        return TypeConverterResult.FromSuccess(shopwaifus.First(x => x.WaifuName == waifus.First().Name));
    }
}