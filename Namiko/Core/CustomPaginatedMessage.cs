using System;
using System.Collections.Generic;
using Discord.Addons.Interactive;
using System.Text;
using Namiko.Core.Util;
using System.Linq;
using Discord;

namespace Namiko.Core
{
    public class CustomPaginatedMessage : PaginatedMessage
    {
        public CustomPaginatedMessage() : base()
        {
            Options = new CustomPaginatedAppearance();
            Color = BasicUtil.RandomColor();
        }

        //Splits an IEnumerable into batches based on the size
        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> source, int size = 10)
        {
            var result = source.Select((s, i) => new { Value = s, Index = i })
                    .GroupBy(item => item.Index / size, item => item.Value);

            return result;
        }

        //Returns pages of items in the list based on their .ToString() size = amount per page, toString = custom entry delegate, counter = adds a count to each entry #1, #2 etc.
        public static IEnumerable<string> PagesArray<T>(IEnumerable<T> source, int size = 10, Func<T, string> toString = null, bool counter = true)
        {
            var lists = Split(source, size);
            var strList = new List<String>();
            int count = 0;

            toString = toString ?? delegate(T x) { return $"{x.ToString()}\n"; };

            foreach (var x in lists)
            {
                var str = "";
                foreach (var y in x)
                {
                    count++;
                    if (counter)
                        str += $"#{count} ";
                    str += toString(y);
                }
                strList.Add(str);
            }

            return strList;
        }
    }

    public class CustomPaginatedAppearance : PaginatedAppearanceOptions
    {
        public CustomPaginatedAppearance() : base()
        {
            Info = null;
            Jump = null;
            Timeout = TimeSpan.FromMinutes(3);
        }
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
