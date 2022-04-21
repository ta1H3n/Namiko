using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Namiko.Addons.Handlers.Paginator
{
    public class PaginatedMessage
    {
        public IEnumerable<object> Pages { get; set; }
        public IEnumerable<FieldPages> Fields { get; set; }

        public string MessageText { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string ImageUrl { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string Title { get; set; } = "";
        public string Footer { get; set; } = "";
        public int PageCount { get; set; }

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;

        public PaginatedMessage()
        {
            Options = new PaginatedAppearanceOptions();
            Color = BasicUtil.RandomColor();
        }
        public PaginatedMessage(List<string> pages) : this()
        {
            Pages = pages;
        }
        public PaginatedMessage(IEnumerable<object> source, int size = 10, Func<object, string> toString = null, bool counter = true)
            => new PaginatedMessage(PaginatedMessage.PagesArray(source, size, toString, counter));

        //Splits an IEnumerable into batches based on the size
        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> source, int size = 10)
        {
            var result = source.Select((s, i) => new { Value = s, Index = i })
                    .GroupBy(item => item.Index / size, item => item.Value);

            return result;
        }

        //Returns pages of items in the list based on their .ToString() size = amount per page, toString = custom entry delegate, counter = adds a count to each entry #1, #2 etc.
        public static List<string> PagesArray<T>(IEnumerable<T> source, int size = 10, Func<T, string> toString = null, bool counter = true)
        {
            var lists = Split(source, size);
            var strList = new List<string>();
            int count = 0;

            toString ??= delegate (T x) { return $"{x.ToString()}\n"; };

            foreach (var x in lists)
            {
                var str = "";
                foreach (var y in x)
                {
                    if (counter)
                    {
                        count++;
                        str += $"#{count} ";
                    }
                    str += toString(y);
                }
                strList.Add(str);
            }

            return strList;
        }

        public int CountPages() 
        {
            int count = 0;

            try
            {
                count = Fields.Max(x => x.Pages.Count());
            }
            catch { }
            try
            {
                count = count > Pages.Count() ? count : Pages.Count();
            }
            catch { }

            return count;
        }
    }

    public class PaginatedMessage<T> : PaginatedMessage
    {
        public PaginatedMessage(IEnumerable<T> source, int size = 10, Func<T, string> toString = null, bool counter = true) : base(PagesArray(source, size, toString, counter)) { }
    }

    public class UserAmountView
    {
        public IUser User { get; set; }
        public int Amount { get; set; }

        public override string ToString()
        {
            return $"{User.Mention} - {Amount}";
        }
    }
}
