using Discord;
using Discord.Interactions;

namespace Namiko.Handlers

{
    public interface IModalBase : IModal
    {
        Modal ToModal();
    }
}