using System;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;
using Discord;

using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using Namiko.Resources.Database;

namespace Namiko.Core.Currency
{
    public static class Blackjack
    {
        public static Dictionary<SocketUser, Game> games = new Dictionary<SocketUser, Game>();

        public static async Task BlackjackInput(SocketCommandContext Context)
        {
            if (!games.ContainsKey(Context.User))
                return;

            var game = games[Context.User];

            if (game != null && Context.Channel.Equals(game.Channel))
            {
                // Hit
                if (Context.Message.Content.Equals("hit") || Context.Message.Content.Equals("Hit"))
                {
                    await Hit(Context, game);
                    await Context.Message.DeleteAsync();
                }

                // Stand
                else if (Context.Message.Content.Equals("stand") || Context.Message.Content.Equals("Stand"))
                {
                    await Stand(Context, game);
                    await Context.Message.DeleteAsync();
                }

                // Forfeit
                else if (Context.Message.Content.Equals("forfeit") || Context.Message.Content.Equals("Forfeit"))
                {
                    await Forfeit(Context, game);
                    await Context.Message.DeleteAsync();
                }

                // Double
                else if (Context.Message.Content.Equals("double") || Context.Message.Content.Equals("Double"))
                {
                    await DoubleDown(Context, game);
                    await Context.Message.DeleteAsync();
                }
            }
        }
        public static async Task DoubleDown(SocketCommandContext Context, Game game)
        {
            try
            {
                await ToastieDb.AddToasties(Context.User.Id, -game.Pats);
                game.Pats *= 2;
                game.Hit();
                await Stand(Context, game);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }
        public static async Task Forfeit(SocketCommandContext Context, Game game)
        {
            var user = Context.User;
            var ch = Context.Channel;

            await ToastieDb.AddToasties(user.Id, game.Pats / 2);
            await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, game.Pats / 2);

            EmbedBuilder eb = new EmbedBuilder();

            eb.WithAuthor(user.Username + " | You Lose", null, user.GetAvatarUrl());
            eb.WithDescription("You forfeit. You get half your pats back. Lost `" + game.Pats / 2 + "`" + ToastieUtil.RandomEmote() + "\n" +
            "New balance `" + ToastieDb.GetToasties(user.Id) + "`" + ToastieUtil.RandomEmote());
            eb.AddField("Your hand (" + game.SumHand(game.Hand) + ")", HandToString(game.Hand, false), true);
            eb.AddField("Namiko's hand (" + game.SumHand(game.Dealer) + ")", HandToString(game.Dealer, false), true);
            eb.WithColor(Color.DarkBlue);

            await Send(game, eb);

            games.Remove(user);
        }
        public static async Task Hit(SocketCommandContext Context, Game game)
        {
            int value = game.Hit();

            if (value < 22)
                await GameContinue(Context, game);
            else
            {
                await GameEnd(Context, game);
            }
        }
        public static async Task Stand(SocketCommandContext Context, Game game)
        {
            game.Stand();
            await GameEnd(Context, game);
        }

        public static async Task GameEnd(SocketCommandContext Context, Game game)
        {
            EmbedBuilder eb = new EmbedBuilder();
            var user = Context.User;
            var ch = Context.Channel;

            if (game.SumHand(game.Hand) > 21)
            {
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, game.Pats);
                eb.WithAuthor(user.Username + " | You Lose", user.GetAvatarUrl());
                eb.WithDescription("Your hand is a bust. You lose `" + game.Pats + "` " + ToastieUtil.RandomEmote() + "\n" +
                "New balance `" + ToastieDb.GetToasties(user.Id) + "` " + ToastieUtil.RandomEmote());
                eb.WithColor(Color.DarkRed);
            }

            else if (game.SumHand(game.Hand) > game.SumHand(game.Dealer) || game.SumHand(game.Dealer) > 21)
            {
                await ToastieDb.AddToasties(user.Id, game.Pats * 2);
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, -game.Pats);
                eb.WithAuthor(user.Username + " | You Win", user.GetAvatarUrl());
                eb.WithDescription("Your score is higher than Namiko's. You win `" + game.Pats + "` " + ToastieUtil.RandomEmote() + "\n" +
                "New balance `" + ToastieDb.GetToasties(user.Id) + "` " + ToastieUtil.RandomEmote());
                eb.WithColor(Color.Gold);
            }

            else if (game.SumHand(game.Hand) == game.SumHand(game.Dealer))
            {
                await ToastieDb.AddToasties(user.Id, game.Pats);
                eb.WithAuthor(user.Username + " | Tie", user.GetAvatarUrl());
                eb.WithDescription("Your score is tied with Namiko's. You get your " + ToastieUtil.RandomEmote() + " back!\n" +
                "Your balance `" + ToastieDb.GetToasties(user.Id) + "` " + ToastieUtil.RandomEmote());
                eb.WithColor(Color.DarkGreen);
            }

            else
            {
                await ToastieDb.AddToasties(Context.Client.CurrentUser.Id, game.Pats);
                eb.WithAuthor(user.Username + " | You Lose", user.GetAvatarUrl());
                eb.WithDescription("Namiko's score is higher. You lose `" + game.Pats + "` " + ToastieUtil.RandomEmote() + "\n" +
                "New balance `" + ToastieDb.GetToasties(user.Id) + "` " + ToastieUtil.RandomEmote());
                eb.WithColor(Color.DarkRed);
            }

            eb.AddField("Your hand (" + game.SumHand(game.Hand) + ")", HandToString(game.Hand, false), true);
            eb.AddField("Namiko's hand (" + game.SumHand(game.Dealer) + ")", HandToString(game.Dealer, false), true);

            await Send(game, eb);

            games.Remove(user);
        }
        public static async Task GameContinue(SocketCommandContext Context, Game game)
        {
            EmbedBuilder eb = new EmbedBuilder();
            var user = Context.User;
            var ch = Context.Channel;

            eb.WithAuthor(user.Username + " | Blackjack", user.GetAvatarUrl());
            eb.WithDescription("You started a game of blackjack against Namiko! The one closest to 21 wins!\n" +
            String.Format("{0,10} - to draw another card.\n", "**hit**") +
            String.Format("{0,10} - to finalize and let Namiko draw.\n", "**stand**") +
            String.Format("{0,10} - to double your bet and draw only 1 card.\n", "**double**") +
            String.Format("{0,10} - to forfeit and get half your pats back.", "**forfeit**"));
            eb.AddField("Your hand (" + game.SumHand(game.Hand) + ")", HandToString(game.Hand, false), true);
            eb.AddField("Namiko's hand (" + game.CardValue(game.Dealer[0]) + ")", HandToString(game.Dealer, true), true);
            eb.WithColor(Color.Green);

            await Send(game, eb);
        }
        public static String HandToString(List<Card> hand, Boolean dealer)
        {
            String cards = "";
            if (dealer)
            {
                cards += hand[0].ValueStr;
                cards += " ?";
            }
            else
                foreach (Card card in hand)
                {
                    cards += card.ValueStr + " ";
                }
            return cards;
        }
        public static async Task Send(Game game, EmbedBuilder eb)
        {
            if (game.Message == null)
            {
                game.Message = await game.Channel.SendMessageAsync("", false, eb.Build());
            }
            else
            {
                await game.Message.ModifyAsync(msg => msg.Embed = eb.Build());
            }
        }
    }
    public class Game
    {
        public int Pats { get; set; }
        public List<Card> Hand { get; set; }
        public List<Card> Dealer { get; set; }
        public Deck Deck { get; set; }
        public long TimeStarted { get; set; }
        public IUserMessage Message { get; set; }
        public ISocketMessageChannel Channel { get; set; }

        public Game(int pats, ISocketMessageChannel channel)
        {
            this.Pats = pats;
            this.Channel = channel;
            SetUp();
        }

        public int Stand()
        {
            int sum = SumHand(Dealer);

            while (sum < 17)
            {
                Dealer.Add(Deck.Draw());
                sum = SumHand(Dealer);
            }

            return sum;
        }

        public int Hit()
        {
            Hand.Add(Deck.Draw());
            return SumHand(Hand);
        }

        public int SumHand(List<Card> hand)
        {
            int sum = 0;
            int ace = 0;

            foreach (Card card in hand)
            {
                int value = CardValue(card);
                sum += value;
                if (value == 11)
                    ace++;
            }
            while (sum > 21 && ace > 0)
            {
                sum -= 10;
                ace -= 1;
            }

            return sum;
        }

        public int CardValue(Card card)
        {
            int value = card.Value;

            if (value > 10)
                value = 10;
            if (value == 1)
                value = 11;

            return value;
        }

        private void SetUp()
        {
            Deck = new Deck();
            Hand = new List<Card>();
            Dealer = new List<Card>();

            Deck.Shuffle();
            Hand.Add(Deck.Draw());
            Hand.Add(Deck.Draw());
            Dealer.Add(Deck.Draw());
            Dealer.Add(Deck.Draw());
        }
    }
    public class Deck
    {
        public List<Card> Cards { get; set; }

        public Deck()
        {
            Build();
        }

        private void Build()
        {
            Cards = new List<Card>();

            String[] category = new String[4];
            category[0] = "Hearts";
            category[1] = "Diamonds";
            category[2] = "Clubs";
            category[3] = "Spades";

            for (int i = 0; i < 4; i++)
            {

                for (int j = 2; j < 11; j++)
                {
                    Cards.Add(new Card(category[i], j.ToString(), j));
                }

                Cards.Add(new Card(category[i], "J", 11));
                Cards.Add(new Card(category[i], "Q", 12));
                Cards.Add(new Card(category[i], "K", 13));
                Cards.Add(new Card(category[i], "A", 1));
            }
        }

        public Card Draw()
        {
            int i = Cards.Count - 1;
            Card card = Cards[i];
            Cards.Remove(card);
            return card;
        }

        public void ListCards()
        {
            foreach (Card card in Cards)
            {
                Console.WriteLine(card.Value + " of " + card.Category);
            }
        }

        public void Shuffle()
        {
            Random rng = new Random();
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }
    }
    public class Card
    {
        public string Category { get; set; }
        public string ValueStr { get; set; }
        public int Value { get; set; }

        public Card(string cat, string sVal, int val)
        {
            Category = cat;
            ValueStr = GetEmote() + sVal;
            Value = val;
        }

        public String GetEmote()
        {
            String name = Category.ToLower();
            return ":" + name + ":";
        }
    }
}
