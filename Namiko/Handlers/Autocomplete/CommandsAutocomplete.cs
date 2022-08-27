﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.AspNetCore.Diagnostics;
using Model;

namespace Namiko.Handlers.Autocomplete;

public class CommandsAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction?.Data?.Options?.FirstOrDefault()?.Value?.ToString() ?? "";
        var commands = Program.Interactions.SlashCommands;

        var result = commands.Where(x => x.Name.ToUpper().Contains(value.ToUpper())).Select(x => new AutocompleteResult
        {
            Name = x.Name + " - " + x.Description,
            Value = x.Name
        }).ToList();
        
        result.Add(new AutocompleteResult("All", "All"));
        
        return AutocompletionResult.FromSuccess(result.OrderBy(x => x.Value).Take(25));
    }
}