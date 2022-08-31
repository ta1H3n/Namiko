using Discord;
using Discord.Interactions;
using Model;
using Namiko.Handlers;

namespace Namiko
{
    public class WaifuEditModal : IModalBase
    {
        public string Title { get; }


        public string Name { get; set; }
        
        [InputLabel($"Full name (with kanji)")]
        [ModalTextInput($"{nameof(LongName)}")]
        public string LongName { get; set; }
        
        [InputLabel($"{nameof(Description)}")]
        [ModalTextInput($"{nameof(Description)}", TextInputStyle.Paragraph)]
        public string Description { get; set; }
        
        [InputLabel($"Character source (Show/Game title)")]
        [ModalTextInput($"{nameof(Source)}")]
        public string Source { get; set; }
        
        [InputLabel($"{nameof(Tier)} - number from 1 to 3. Or different for special waifus.")]
        [ModalTextInput($"{nameof(Tier)}")]
        public int Tier { get; set; }
        
        [InputLabel($"URL to the source of the used image")]
        [ModalTextInput($"{nameof(ImageSource)}")]
        public string ImageSource { get; set; }
        
        public WaifuEditModal(string title, Waifu waifu = null)
        {
            if (waifu != null)
            {
                FromWaifu(waifu);
            }

            Title = title;
        }

        public WaifuEditModal()
        {
            
        }

        public WaifuEditModal FromWaifu(Waifu waifu)
        {
            Name = waifu.Name;
            LongName = waifu.LongName;
            Description = waifu.Description;
            Source = waifu.Source;
            Tier = waifu.Tier;
            ImageSource = waifu.ImageSource;
            return this;
        }

        public Waifu ToWaifu(Waifu waifuToUpdate)
        {
            waifuToUpdate.LongName = LongName;
            waifuToUpdate.Description = Description;
            waifuToUpdate.Source = Source;
            waifuToUpdate.Tier = Tier;
            waifuToUpdate.ImageSource = ImageSource;
            return waifuToUpdate;
        }
        
        public Modal ToModal()
        {
            return new ModalBuilder()
                .WithTitle(Title)
                .WithCustomId($"waifu-edit-modal:{Name}")
                .AddTextInput("Full name (with kanji)", 
                    nameof(LongName), value: LongName)
                .AddTextInput("Description", 
                    nameof(Description), TextInputStyle.Paragraph, value: Description)
                .AddTextInput("Character source (Show/Game title)", 
                    nameof(Source), value: Source)
                .AddTextInput($"{nameof(Tier)} - usually 1, 2 or 3", 
                    nameof(Tier), value: Tier.ToString())
                .AddTextInput($"URL to the source of the used image", 
                    nameof(ImageSource), value: ImageSource)
                .Build();
        }
    }
}