﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Extenstions;

namespace Namiko.Handlers.Autocomplete;

public class CommandsAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction.GetInput(parameter) ?? "";
        var commands = services.GetService<SlashCommandService>().Interaction.SlashCommands;

        var result = commands.Where(x => x.Name.ToUpper().Contains(value.ToUpper())).Select(x => new AutocompleteResult
        {
            Name = x.Name + " - " + x.Description,
            Value = x.Name
        }).ToList();
        
        return AutocompletionResult.FromSuccess(result.OrderBy(x => x.Value).Take(25));
    }
}