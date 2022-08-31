using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.AspNetCore.Diagnostics;
using Model;

namespace Namiko.Handlers.Autocomplete;

public class WishlistWaifuAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction?.Data?.Options?.FirstOrDefault()?.Value;

        var items = await WaifuWishlistDb.GetWishlist(context.User.Id, context.Guild.Id);
        var waifus = await WaifuDb.SearchWaifus(value.ToString(), false, items);

        var result = waifus.Select(x => new AutocompleteResult
        {
            Name = x.Name + " - " + x.Source,
            Value = x.Source + " " + x.Name
        });
        
        return AutocompletionResult.FromSuccess(result.OrderBy(x => x.Value).Take(25));
    }
}