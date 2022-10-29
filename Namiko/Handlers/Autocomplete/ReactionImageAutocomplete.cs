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

public class ReactionImageAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction.GetInput(parameter) ?? "";
        var images = Images.ReactionImageCommands[0].ToHashSet();
        
        if (context?.Guild != null && Images.ReactionImageCommands.TryGetValue(context.Guild.Id, out var images2))
        {
            foreach (var image in images2)
            {
                if (!images.Contains(image))
                {
                    images.Add(image);
                }
            }
        }

        var result = images.Where(x => x.ToUpper().Contains(value.ToUpper())).Select(x => new AutocompleteResult
        {
            Name = x,
            Value = x
        });
        
        return AutocompletionResult.FromSuccess(result.OrderBy(x => x.Value).Take(25));
    }
}