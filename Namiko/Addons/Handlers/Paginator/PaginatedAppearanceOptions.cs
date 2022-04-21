using Discord;
using System;

namespace Namiko.Addons.Handlers.Paginator
{
    public class PaginatedAppearanceOptions
    {
        public static PaginatedAppearanceOptions Default = new PaginatedAppearanceOptions();

        public ButtonBuilder BackHundred = new ButtonBuilder("x100", nameof(BackHundred), ButtonStyle.Secondary, emote: new Emoji("⏪"));
        public ButtonBuilder BackTen = new ButtonBuilder("x10", nameof(BackTen), ButtonStyle.Secondary, emote: new Emoji("◀"));
        public ButtonBuilder Back = new ButtonBuilder("x1", nameof(Back), ButtonStyle.Secondary, emote: Emote.Parse("<:KannaPointingLeft:543086063057502219>"));
        public ButtonBuilder Next = new ButtonBuilder("x1", nameof(Next), ButtonStyle.Secondary, emote: Emote.Parse("<:KannaPointingRight:543086063380332555>"));
        public ButtonBuilder NextTen = new ButtonBuilder("x10", nameof(NextTen), ButtonStyle.Secondary, emote: new Emoji("▶"));
        public ButtonBuilder NextHundred = new ButtonBuilder("x100", nameof(NextHundred), ButtonStyle.Secondary, emote: new Emoji("⏩"));

        public string FooterFormat = "Page {0}/{1} =w=";

        public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.WithManageMessages;
    }

    public enum JumpDisplayOptions
    {
        Never,
        WithManageMessages,
        Always
    }
}
