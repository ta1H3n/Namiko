using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.AspNetCore.Diagnostics;
using Model;
using Namiko.Handlers.Extenstions;

namespace Namiko.Handlers.Autocomplete;

public class ShopWaifuAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction.GetInput(parameter) ?? "";

        var shopwaifus = (await WaifuShopDb.GetAllShopWaifus(context.Guild.Id)).DistinctBy(x => x.WaifuName);
        var waifus = await WaifuDb.SearchWaifus(value, false, shopwaifus.Select(x => x.Waifu));

        var result = waifus.OrderBy(x => x.Source).ThenBy(x => x.Name).Select(x => new AutocompleteResult
        {
            Name = x.Name + " - " + x.Source,
            Value = "key:" + x.Name
        });
        
        return AutocompletionResult.FromSuccess(result.Take(25));
    }
}