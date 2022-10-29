using System.Linq;
using System.Runtime.CompilerServices;
using Discord;
using Discord.Interactions;

namespace Namiko.Handlers.Extenstions;

public static class IAutocompleteInteractionExtensions
{
    public static string GetInput(this IAutocompleteInteraction interaction, IParameterInfo parameter)
    {
        return interaction?.Data?.Options?.FirstOrDefault(x => x.Name == parameter.Name)?.Value.ToString();
    }
}